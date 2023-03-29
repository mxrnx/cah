module Main exposing (..)

import Browser
import Html
import Html.Attributes
import Html.Events
import Http exposing (Error(..), jsonBody)
import Json.Encode as Encode
import Json.Decode exposing (Decoder, field, string, list, bool, map3)
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

type Status
  = Error String
  | NotLoggedIn
  | LoggingIn
  | LoggedIn
  | RoundPickCards

type alias Model =
    { userName : String       -- username of the current player
    , id       : Maybe UUID   -- id of the current player
    , status   : Status       -- status object
    , players  : List Player  -- list of all players
    , czar     : Maybe UUID   -- UUID of the current card czar
    }


init : () -> (Model, Cmd Msg)
init _ =
  ( { userName = ""
    , id       = Nothing
    , status   = NotLoggedIn
    , players  = []
    , czar     = Nothing
    }
  , Cmd.none
  )



-- DECODERS

player : Decoder Player
player =
    map3 Player
        (field "id" UUID.jsonDecoder)
        (field "name" string)
        (field "czar" bool)



-- UPDATE

type Msg
  = LogInAnswer (Result Http.Error Player)
  | LogOutAnswer (Result Http.Error ())
  | PlayerListAnswer (Result Http.Error (List Player))
  | CzarAnswer (Result Http.Error UUID)
  | DealCardsAnswer (Result Http.Error ())
  | EditName String
  | LogIn
  | LogOut
  | StartGame
  | Tick Time.Posix


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
                    , expect = Http.expectWhatever LogOutAnswer
                    }
                )
    StartGame ->
        ( { model | status = RoundPickCards }
        , Http.post
            { body = jsonBody (Encode.null)
            , url = "https://localhost:5001/Game"
            , expect = Http.expectWhatever DealCardsAnswer
            }
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
    LogOutAnswer result ->
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
    CzarAnswer result ->
      case result of
        Ok czarId ->
          ( { model | czar = Just czarId }
          , Cmd.none 
          )
        Err httpErr ->
          ( { model | status = stringHttpError httpErr }
          , Cmd.none
          )
    DealCardsAnswer _ -> (model, Cmd.none) -- TODO
    Tick _ ->
      ( model
      , Http.get
          { url = "https://localhost:5001/Player"
          ,  expect = Http.expectJson PlayerListAnswer (list player)
          }
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
  else Time.every 1000 Tick



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
        , if List.length model.players >= 3
          then Html.button [ Html.Events.onClick StartGame ] [ Html.text "Start game!" ]
          else Html.i [] [ Html.text "Waiting for 3 or more players to start..." ]
        ]

    Error msg ->
      Html.div []
      [ Html.text ("oh no, error: " ++ msg)
      ]

    RoundPickCards -> Html.text "to do" -- TODO


formatPlayerName : Player -> Html.Html Msg
formatPlayerName p = if p.czar
                     then Html.b [] [ Html.text p.name ]
                     else Html.text p.name
