module Main exposing (..)

import Browser
import Html exposing (Html, text, pre, div, input, button, ul, li)
import Html.Attributes exposing (placeholder, value)
import Html.Events exposing (onInput, onClick)
import Http exposing (jsonBody, Error(..))
import Json.Encode as Encode
import Json.Decode exposing (Decoder, field, string, list, map2)
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
    { id   : UUID
    , name : String
    }

type Status
  = Error String
  | NotLoggedIn
  | LoggingIn
  | LoggedIn

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
    map2 Player
        (field "id" UUID.jsonDecoder)
        (field "name" string)


-- UPDATE


type Msg
  = LogInAnswer (Result Http.Error UUID)
  | LogOutAnswer (Result Http.Error ())
  | PlayerListAnswer (Result Http.Error (List Player))
  | EditName String
  | LogIn
  | LogOut


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
            , expect = Http.expectJson LogInAnswer UUID.jsonDecoder
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
    LogInAnswer result ->
      case result of
        Ok newId ->
          ( { model | status = LoggedIn , id = Just newId }
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
        Ok newId ->
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
  Sub.none



-- VIEW


view : Model -> Html Msg
view model =
  case model.status of
    NotLoggedIn ->
        div []
        [ input [ placeholder "Nickname", value model.userName, onInput EditName ] []
        , button [ onClick LogIn ] [ text "Log in" ]
        ]

    LoggingIn ->
      text "Logging in..."

    LoggedIn ->
        div []
        [ ul []
            (List.map (\p -> li [] [ text p.name ]) model.players)
        , button [ onClick LogOut ] [ text "Log out" ]
        ]

    Error msg ->
      div []
      [ text ("oh no, error: " ++ msg)
      ]
