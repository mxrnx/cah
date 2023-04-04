module Main exposing (..)

import Browser
import Html
import Html.Attributes
import Html.Events
import Http exposing (Error(..), jsonBody)
import Json.Encode as Encode
import Json.Decode exposing (Decoder, bool, field, list, map2, map3, maybe, string)
import List.Extra
import Time
import UUID exposing (UUID)



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
    { id     : UUID
    , name   : String
    , czar   : Bool
    }

type alias AnswerCard =
    { id    : UUID
    , text  : String
    }

type Status
  = Error String
  | NotLoggedIn
  | LoggingIn
  | LoggedIn
  | RoundPickCards

type alias Model =
    { userName : String            -- username of the current player
    , id       : Maybe UUID        -- id of the current player
    , status   : Status            -- status object
    , players  : List Player       -- list of all players
    , hand     : List AnswerCard   -- this player's hand of answer cards
    }


init : () -> (Model, Cmd Msg)
init _ =
  ( { userName = ""
    , id       = Nothing
    , status   = NotLoggedIn
    , players  = []
    , hand     = []
    }
  , Http.get
    { url = "https://localhost:5001/Player/Me"
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

answerCard : Decoder AnswerCard
answerCard =
    map2 AnswerCard
        (field "id" UUID.jsonDecoder)
        (field "text" string)


-- UPDATE

type Msg
  = PlayerInitAnswer (Result Http.Error (Maybe Player))
  | LogInAnswer (Result Http.Error Player)
  | NoContentAnswer (Result Http.Error ())
  | PlayerListAnswer (Result Http.Error (List Player))
  | GameAnswer (Result Http.Error (List AnswerCard))
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
        ( { model | status = LoggingIn }
        , Http.post
            { body = jsonBody (Encode.string model.userName)
            , url = "https://localhost:5001/Player"
            , expect = Http.expectJson LogInAnswer player
            }
        )
    LogOut ->
        case model.id of
            Nothing -> ( model , Cmd.none )
            Just id ->
                ( { model | status = LoggingIn }
                , Http.request
                    { method = "DELETE"
                    , headers = []
                    , timeout = Nothing
                    , tracker = Nothing
                    , body = jsonBody (Encode.string (UUID.toString id))
                    , url = "https://localhost:5001/Player"
                    , expect = Http.expectWhatever NoContentAnswer
                    }
                )
    StartGame ->
        ( { model | status = RoundPickCards }
        , Http.post
            { body = jsonBody (Encode.int 5) -- TODO: make number of necessary wins configurable
            , url = "https://localhost:5001/Game"
            , expect = Http.expectWhatever NoContentAnswer
            }
        )
    PlayerInitAnswer result ->
      case result of
        Ok maybePlayer ->
          case maybePlayer of
              Nothing -> ( model, Cmd.none )
              Just existingPlayer ->
                ( { model | status = LoggedIn, id = Just existingPlayer.id }
                , Http.get
                  { url = "https://localhost:5001/Player"
                  , expect = Http.expectJson PlayerListAnswer (list player)
                  }
                )
        Err httpErr ->
          ( { model | status = stringHttpError httpErr }
          , Cmd.none
          )
    LogInAnswer result ->
      case result of
        Ok newPlayer ->
          ( { model | status = LoggedIn , id = Just newPlayer.id }
          , Http.get
              { url = "https://localhost:5001/Player"
              , expect = Http.expectJson PlayerListAnswer (list player)
              }

          )
        Err httpErr ->
          ( { model | status = stringHttpError httpErr }
          , Cmd.none
          )
    NoContentAnswer result ->
      case result of
        Ok _ ->
          ( { model | status = NotLoggedIn , id = Nothing }
          , Cmd.none 
          )
        Err httpErr ->
          ( { model | status = stringHttpError httpErr }
          , Cmd.none
          )
    PlayerListAnswer result ->
      case result of
        Ok newPlayers ->
          ( { model | players = newPlayers }
          , Cmd.none
          )
        Err httpErr ->
          ( { model | status = stringHttpError httpErr }
          , Cmd.none
          )
    GameAnswer result ->
      case result of
        Ok newHand ->
          ( { model | hand = newHand }
          , Cmd.none
          )
        Err httpErr ->
          ( { model | status = stringHttpError httpErr }
          , Cmd.none
          )
    WaitingTick _ ->
      ( model
      , Http.get
        { url = "https://localhost:5001/Player"
        ,  expect = Http.expectJson PlayerListAnswer (list player)
        }
      )
    GameTick _ ->
      ( model
      , Cmd.batch
        [ Http.get
          { url = "https://localhost:5001/Player"
          ,  expect = Http.expectJson PlayerListAnswer (list player)
          }
        , Http.get
          { url = "https://localhost:5001/Game"
          ,  expect = Http.expectJson GameAnswer (list answerCard)
          }
        ]
      )


stringHttpError : Http.Error -> Status
stringHttpError err =
    case err of
        BadUrl str -> Error ("Bad url: " ++ str)
        Timeout -> Error "Timeout"
        NetworkError -> Error "Network error"
        BadStatus stat -> Error ("Status " ++ String.fromInt stat)
        BadBody str -> Error ("Bad body: " ++ str)
        

-- SUBSCRIPTIONS


subscriptions : Model -> Sub Msg
subscriptions model =
  if model.status == NotLoggedIn || model.status == LoggingIn
  then Sub.none
  else
    if model.status == RoundPickCards
    then Time.every 1000 GameTick
    else Time.every 1000 WaitingTick



-- VIEW

view : Model -> Html.Html Msg
view model =
  case model.status of
    NotLoggedIn ->
        Html.div []
        [ Html.input [ Html.Attributes.placeholder "Nickname", Html.Attributes.value model.userName, Html.Events.onInput EditName ] []
        , Html.button [ Html.Events.onClick LogIn ] [ Html.text "Log in" ]
        ]

    LoggingIn ->
      Html.text "Logging in..."

    LoggedIn ->
        Html.div []
        [ Html.ul []
            (List.map (\p -> Html.li [] [ formatPlayerName p ]) model.players)
        , Html.button [ Html.Events.onClick LogOut ] [ Html.text "Log out" ]
        , case currentPlayer model of
            Nothing -> Html.text "Something went terribly wrong" -- TODO
            Just p ->
                if p.czar
                then
                  if List.length model.players >= 3
                  then Html.button [ Html.Events.onClick StartGame ] [ Html.text "Start game!" ]
                  else Html.i [] [ Html.text "Waiting for 3 or more players to start..." ]
                else Html.i [] [ Html.text "Waiting for czar to start the game..." ]
        ]

    Error msg ->
      Html.div []
      [ Html.text ("oh no, error: " ++ msg)
      ]

    RoundPickCards -> Html.div [] (List.map (\card -> Html.p [] [ Html.text card.text ]) model.hand)


formatPlayerName : Player -> Html.Html Msg
formatPlayerName p = if p.czar
                     then Html.b [] [ Html.text p.name ]
                     else Html.text p.name
