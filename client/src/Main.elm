module Main exposing (..)

import Browser
import Html
import Html.Attributes exposing (class)
import Html.Events
import Http exposing (Error(..), jsonBody)
import Json.Encode as Encode
import Json.Decode exposing (Decoder, andThen, bool, field, list, map, map2, map3, maybe, string)
import List.Extra
import Time
import UUID exposing (UUID)
import Url.Builder exposing (crossOrigin)



-- MAIN

main =
  Browser.element
    { init = init
    , update = update
    , subscriptions = subscriptions
    , view = view
    }



-- MODEL

type alias Player =
  { id        : UUID
  , name      : String
  , czar      : Bool
  }

type alias GameState =
  { handCards : List AnswerCard
  }

type alias AnswerCard =
  { id        : UUID
  , text      : String
  }

type LoginStatus
  = Error String
  | NotLoggedIn
  | LoggingIn
  | LoggedIn

type GamePhase
  = WaitingToStart
  | PickingAnswers
  | ShowingAnswers
  | PickingWinner

type alias Model =
  { userName    : String            -- username of the current player
  , id          : Maybe UUID        -- id of the current player
  , loginStatus : LoginStatus            -- status object
  , gamePhase   : GamePhase
  , players     : List Player       -- list of all players
  , handCards   : List AnswerCard   -- this player's hand of answer cards
  }


init : () -> (Model, Cmd Msg)
init _ =
  ( { userName    = ""
    , id          = Nothing
    , loginStatus = NotLoggedIn
    , gamePhase   = WaitingToStart
    , players     = []
    , handCards   = []
    }
  , Http.get
    { url = url [ "Player", "Me"]
    , expect = Http.expectJson PlayerInitAnswer (maybe player)
    }
  )

currentPlayer : Model -> Maybe Player
currentPlayer model =
  List.Extra.find (\p -> Just p.id == model.id) model.players



-- DECODERS

player : Decoder Player
player =
  map3 Player
    (field "id" UUID.jsonDecoder)
    (field "name" string)
    (field "czar" bool)

gameState : Decoder GameState
gameState = map GameState
              (field "handCards" (list answerCard))

answerCard : Decoder AnswerCard
answerCard =
    map2 AnswerCard
        (field "id" UUID.jsonDecoder)
        (field "text" string)

gamePhase : Decoder GamePhase
gamePhase =
  string |> andThen (\str ->
    case str of
      "WaitingToStart" -> Json.Decode.succeed WaitingToStart
      "PickingAnswers" -> Json.Decode.succeed PickingAnswers
      "ShowingAnswers" -> Json.Decode.succeed ShowingAnswers
      "PickingWinner" -> Json.Decode.succeed PickingWinner
      _ -> Json.Decode.fail "Invalid GamePhase"
  )



-- UPDATE

type Msg
  = PlayerInitAnswer (Result Http.Error (Maybe Player))
  | LogInAnswer (Result Http.Error Player)
  | NoContentAnswer (Result Http.Error ())
  | PlayerListAnswer (Result Http.Error (List Player))
  | GameAnswer (Result Http.Error GameState)
  | GamePhaseAnswer (Result Http.Error GamePhase)

  | EditName String

  | LogIn
  | LogOut
  | StartGame

  | WaitingTick Time.Posix
  | GameTick Time.Posix


update : Msg -> Model -> (Model, Cmd Msg)
update msg model =
  case msg of
    EditName newName ->
        ( { model | userName = newName }
        , Cmd.none
        )
    LogIn ->
        ( { model | loginStatus = LoggingIn }
        , Http.post
            { body = jsonBody (Encode.string model.userName)
            , url = url [ "Player" ]
            , expect = Http.expectJson LogInAnswer player
            }
        )
    LogOut ->
        case model.id of
            Nothing -> ( model , Cmd.none )
            Just id ->
                ( { model | loginStatus = NotLoggedIn }
                , Http.request
                    { method = "DELETE"
                    , headers = []
                    , timeout = Nothing
                    , tracker = Nothing
                    , body = jsonBody (Encode.string (UUID.toString id))
                    , url = url [ "Player" ]
                    , expect = Http.expectWhatever NoContentAnswer
                    }
                )
    StartGame ->
        ( model
        , Http.post
            { body = jsonBody (Encode.int 5) -- TODO: make number of necessary wins configurable
            , url = url [ "Game" ]
            , expect = Http.expectWhatever NoContentAnswer
            }
        )
    PlayerInitAnswer result ->
      case result of
        Ok maybePlayer ->
          case maybePlayer of
              Nothing -> ( model, Cmd.none )
              Just existingPlayer ->
                ( { model | loginStatus = LoggedIn, id = Just existingPlayer.id }
                , Http.get
                  { url = url [ "Player" ]
                  , expect = Http.expectJson PlayerListAnswer (list player)
                  }
                )
        Err httpErr ->
          ( { model | loginStatus = stringHttpError httpErr }
          , Cmd.none
          )
    LogInAnswer result ->
      case result of
        Ok newPlayer ->
          ( { model | loginStatus = LoggedIn , id = Just newPlayer.id }
          , Http.get
              { url = url [ "Player" ]
              , expect = Http.expectJson PlayerListAnswer (list player)
              }

          )
        Err httpErr ->
          ( { model | loginStatus = stringHttpError httpErr }
          , Cmd.none
          )
    NoContentAnswer _ -> ( model , Cmd.none )
    PlayerListAnswer result ->
      case result of
        Ok newPlayers ->
          ( { model | players = newPlayers }
          , Cmd.none
          )
        Err httpErr ->
          ( { model | loginStatus = stringHttpError httpErr }
          , Cmd.none
          )
    GameAnswer result ->
      case result of
        Ok state ->
          ( { model | handCards = state.handCards }
          , Cmd.none
          )
        Err httpErr ->
          ( { model | loginStatus = stringHttpError httpErr }
          , Cmd.none
          )
    GamePhaseAnswer result ->
      case result of
        Ok phase ->
          ( { model | gamePhase = phase }
          , Cmd.none
          )
        Err httpErr ->
          ( { model | loginStatus = stringHttpError httpErr }
          , Cmd.none
          )
    WaitingTick _ ->
      ( model
      , Cmd.batch
        [ Http.get
          { url = url [ "Player" ]
          ,  expect = Http.expectJson PlayerListAnswer (list player)
          }
        , Http.get
          { url = url [ "Game", "Phase" ]
          ,  expect = Http.expectJson GamePhaseAnswer gamePhase
          }
        ]
      )
    GameTick _ ->
      ( model
      , Cmd.batch
        [ Http.get
          { url = url [ "Player" ]
          ,  expect = Http.expectJson PlayerListAnswer (list player)
          }
        , Http.get
          { url = url [ "Game" ]
          ,  expect = Http.expectJson GameAnswer gameState
          }
        ]
      )

stringHttpError : Http.Error -> LoginStatus
stringHttpError err =
    case err of
        BadUrl str -> Error ("Bad url: " ++ str)
        Timeout -> Error "Timeout"
        NetworkError -> Error "Network error"
        BadStatus stat -> Error ("Status " ++ String.fromInt stat)
        BadBody str -> Error ("Bad body: " ++ str)

url : List String -> String
url path = crossOrigin "https://localhost:5001" path []



-- SUBSCRIPTIONS


subscriptions : Model -> Sub Msg
subscriptions model =
  if model.loginStatus == NotLoggedIn || model.loginStatus == LoggingIn
  then Sub.none
  else
    if model.gamePhase == WaitingToStart
    then Time.every 1000 WaitingTick
    else Time.every 1000 GameTick



-- VIEW

view : Model -> Html.Html Msg
view model =
  case model.loginStatus of
    NotLoggedIn ->
        Html.div [ class "columns" ]
        [ Html.input [ class "input column is-two-fifths", Html.Attributes.placeholder "Nickname", Html.Attributes.value model.userName, Html.Events.onInput EditName  ] []
        , Html.button [ class "button column is-one-fifth", Html.Events.onClick LogIn ] [ Html.text "Log in" ]
        ]

    LoggingIn ->
      Html.text "Logging in..."
    LoggedIn ->
      viewLayout model (viewContent model)
    Error msg ->
      Html.div []
      [ Html.text ("Oh no, error: " ++ msg)
      ]


viewLayout : Model -> Html.Html Msg -> Html.Html Msg
viewLayout model content = Html.div [ class "columns" ]
                             [ Html.div [ class "column is-four-fifths" ] [ content ]
                             , Html.div [ class "column" ]
                                 [ Html.aside [ class "menu"]
                                   [ Html.ul [ class "menu-list" ]
                                       (Html.p [ class "menu-label" ] [ Html.text "Players" ] ::
                                       (List.map (\p -> Html.li [] [ Html.a [] [ formatPlayerName p ] ]) model.players))
                                   ]
                                 , Html.button [ class "button mt-4", Html.Events.onClick LogOut ] [ Html.text "Log out" ]
                                 ]
                             ]

viewContent : Model -> Html.Html Msg
viewContent model =
  case model.gamePhase of
    WaitingToStart ->
        case currentPlayer model of
          Nothing -> Html.text "Something went terribly wrong" -- TODO
          Just p ->
              if p.czar
              then
                if List.length model.players >= 3
                then Html.button [ class "button", Html.Events.onClick StartGame ] [ Html.text "Start game!" ]
                else Html.i [] [ Html.text "Waiting for 3 or more players to start..." ]
              else Html.i [] [ Html.text "Waiting for czar to start the game..." ]

    PickingAnswers -> Html.div []
                        [ Html.div [] (List.map (\card -> Html.p [] [ Html.text card.text ]) model.handCards)
                        , Html.text "Picking cards"
                        ]

    _ -> Html.text "Not yet implemented" -- TODO


formatPlayerName : Player -> Html.Html Msg
formatPlayerName p = if p.czar
                     then Html.b [] [ Html.text p.name ]
                     else Html.text p.name
