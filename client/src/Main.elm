module Main exposing (..)

import Browser
import Html
import Html.Attributes exposing (class, style)
import Html.Events
import Http exposing (jsonBody)
import InfoHttp exposing (InfoError(..), expectJson)
import Json.Encode as Encode
import Json.Decode exposing (Decoder, andThen, bool, field, int, list, map2, map3, map4, maybe, string)
import List exposing (filter)
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
  , secret    : Maybe UUID
  }

type alias GameState =
  { handCards  : List AnswerCard
  , roundCards : List AnswerCard
  , gamePhase  : GamePhase
  , promptCard : PromptCard
  }

type alias AnswerCard =
  { id        : UUID
  , text      : String
  }

type alias PromptCard =
  { id         : UUID
  , text       : String
  , fieldCount : Int
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
  { userName     : String            -- username of the current player
  , secret       : Maybe UUID              -- secret to identify the client
  , id           : Maybe UUID        -- id of the current player
  , loginStatus  : LoginStatus       -- the login status of the current player
  , gamePhase    : GamePhase         -- current phase of the game
  , players      : List Player       -- list of all players
  , handCards    : List AnswerCard   -- this player's hand of answer cards
  , roundCards   : List AnswerCard   -- this player's played cards this round
  , selectedCard : Maybe UUID        -- the currently selected card, if any
  , promptCard   : Maybe PromptCard  -- this round's prompt
  }


init : () -> (Model, Cmd Msg)
init _ =
  ( { userName     = ""
    , secret       = Nothing
    , id           = Nothing
    , loginStatus  = NotLoggedIn
    , gamePhase    = WaitingToStart
    , players      = []
    , handCards    = []
    , roundCards   = []
    , selectedCard = Nothing
    , promptCard   = Nothing
    }
  , Cmd.none
  )

currentPlayer : Model -> Maybe Player
currentPlayer model =
  List.Extra.find (\p -> Just p.id == model.id) model.players



-- DECODERS

player : Decoder Player
player =
  map4 Player
    (field "id" UUID.jsonDecoder)
    (field "name" string)
    (field "czar" bool)
    (field "secret" (maybe UUID.jsonDecoder))

gameState : Decoder GameState
gameState = map4 GameState
              (field "handCards" (list answerCard))
              (field "roundCards" (list answerCard))
              (field "gamePhase" gamePhase)
              (field "promptCard" promptCard)

answerCard : Decoder AnswerCard
answerCard =
    map2 AnswerCard
        (field "id" UUID.jsonDecoder)
        (field "text" string)

promptCard : Decoder PromptCard
promptCard =
    map3 PromptCard
        (field "id" UUID.jsonDecoder)
        (field "text" string)
        (field "fieldCount" int)

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
  = PlayerInitAnswer (Result InfoError (Maybe Player))
  | LogInAnswer (Result InfoError Player)
  | NoContentAnswer (Result Http.Error ())
  | PlayerListAnswer (Result InfoError (List Player))
  | GameAnswer (Result InfoError GameState)
  | GamePhaseAnswer (Result InfoError GamePhase)

  | EditName String

  | LogIn
  | LogOut
  | StartGame
  | SelectCard UUID
  | PlayCard

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
            , expect = expectJson LogInAnswer player
            }
        )
    LogOut ->
        case model.secret of
            Nothing -> ( model , Cmd.none )
            Just secret ->
                ( { model | loginStatus = NotLoggedIn }
                , Http.request
                    { method = "DELETE"
                    , headers = []
                    , timeout = Nothing
                    , tracker = Nothing
                    , body = jsonBody (Encode.string (UUID.toString secret))
                    , url = url [ "Player" ]
                    , expect = Http.expectWhatever NoContentAnswer
                    }
                )
    StartGame ->
        case model.secret of
            Nothing -> ( model , Cmd.none ) -- TODO: feedback to user
            Just secret ->
                ( model
                , Http.post
                    { body = jsonBody (Encode.object [ ("secret", (Encode.string (UUID.toString secret)))
                                                     , ("necessaryWins", (Encode.int 5)) -- TODO: make number of necessary wins configurable
                                                     ])
                    , url = url [ "Game" ]
                    , expect = Http.expectWhatever NoContentAnswer
                    }
                )
    SelectCard sel ->
      case model.gamePhase of
        PickingAnswers ->
          ( { model | selectedCard = Just sel }
          , Cmd.none
          )
        _ -> ( model, Cmd.none )
    PlayCard ->
      case model.selectedCard of
        Nothing -> ( model, Cmd.none )
        Just playedCard -> ( { model | handCards = filter (\x -> x.id /= playedCard ) model.handCards } -- TODO: is this necessary?
                           , Http.post
                               { body = jsonBody (Encode.string (UUID.toString playedCard))
                               , url = url [ "Round", "PlayCard" ]
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
                  , expect = expectJson PlayerListAnswer (list player)
                  }
                )
        Err httpErr ->
          ( { model | loginStatus = stringHttpError httpErr }
          , Cmd.none
          )
    LogInAnswer result ->
      case result of
        Ok newPlayer ->
          ( { model | loginStatus = LoggedIn , id = Just newPlayer.id , secret = newPlayer.secret }
          , Http.get
              { url = url [ "Player" ]
              , expect = expectJson PlayerListAnswer (list player)
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
          ( { model | handCards = state.handCards, roundCards = state.roundCards,
                      promptCard = Just state.promptCard, gamePhase = state.gamePhase }
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
          ,  expect = expectJson PlayerListAnswer (list player)
          }
        , Http.get
          { url = url [ "Game", "Phase" ]
          ,  expect = expectJson GamePhaseAnswer gamePhase
          }
        ]
      )
    GameTick _ ->
      case model.secret of
          Nothing -> ( model, Cmd.none )
          Just secret ->
              ( model
              , Cmd.batch
                [ Http.get
                  { url = url [ "Player" ]
                  ,  expect = expectJson PlayerListAnswer (list player)
                  }
                , Http.get
                  { url = url [ "Game?secret=" ++ (UUID.toString secret) ]
                  ,  expect = expectJson GameAnswer gameState
                  }
                ]
              )

url : List String -> String
url path = crossOrigin "https://localhost:5001" path []

stringHttpError : InfoError -> LoginStatus
stringHttpError err =
    case err of
        BadUrl str -> Error ("Bad url: " ++ str)
        Timeout -> Error "Timeout"
        NetworkError -> Error "Network error"
        BadStatus meta body -> Error ("Status " ++ String.fromInt meta.statusCode ++ ": " ++ body)
        BadBody str -> Error ("Bad body: " ++ str)



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
      [ Html.text ("An error occurred: " ++ msg)
      ]


viewLayout : Model -> Html.Html Msg -> Html.Html Msg
viewLayout model content = Html.div [ class "columns" ]
                             [ Html.div [ class "column is-four-fifths" ] [ content ]
                             , Html.div [ class "column" ]
                                 [ formatPlayButton model.gamePhase model.selectedCard
                                 , Html.aside [ class "menu"]
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
                        [ formatPrompt model.promptCard
                        , Html.div [ class "columns is-multiline is-8"]
                            (List.map (\card -> formatCard card (Just card.id == model.selectedCard)) model.handCards)
                        ]

    _ -> Html.text "Not yet implemented" -- TODO

formatPlayButton : GamePhase -> Maybe UUID -> Html.Html Msg
formatPlayButton phase selectedCard =
  let
    active =
      case phase of
        PickingAnswers -> selectedCard /= Nothing
        _ -> False
  in
    Html.div [class "column" ]
      [ Html.button
          ([ class "button mb-4 is-primary is-responsive"
          , Html.Attributes.disabled (not active)
          ] ++ if active
               then [Html.Events.onClick PlayCard]
               else [] )
          [ Html.text "Play this card!" ] ]

formatPrompt : Maybe PromptCard -> Html.Html Msg
formatPrompt maybeCard = case maybeCard of
  Nothing -> Html.div [] []
  Just card -> Html.div [ class "columns " ]
                 [ Html.div [ class "column is-full" ]
                     [ Html.div
                         [ class "box has-background-dark has-text-light" ]
                         [ Html.p [ class "is-size-3" ] [ Html.text card.text ] ] ] ]

formatCard : AnswerCard -> Bool -> Html.Html Msg
formatCard card isSelected = Html.div [ class "column is-one-quarter" ]
                              [ Html.div
                                  [ Html.Events.onClick (SelectCard card.id)
                                  , Html.Attributes.id (UUID.toString card.id)
                                  , class ( if isSelected
                                            then "box is-clickable has-background-primary-light"
                                            else "box is-clickable has-background-light" )
                                  , style "height" "20rem"
                                  ]
                                  [ Html.p [ class "is-size-3" ] [ Html.text card.text ] ] ]

formatPlayerName : Player -> Html.Html Msg
formatPlayerName p = if p.czar
                     then Html.b [] [ Html.text p.name ]
                     else Html.text p.name
