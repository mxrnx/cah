module Main exposing (..)

import Browser
import Html exposing (Html, text, pre, div, input, button)
import Html.Attributes exposing (placeholder, value)
import Html.Events exposing (onInput, onClick)
import Http exposing (jsonBody, Error(..))
import Json.Encode as Encode
import Json.Decode exposing (Decoder, field, string)
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
    , status   : Status       -- status object
    , players  : List Player  -- list of all players
    , czar     : Maybe UUID   -- UUID of the current card czar
    }


init : () -> (Model, Cmd Msg)
init _ =
  ( { userName = ""
    , status = NotLoggedIn
    , players = []
    , czar = Nothing
    }
  , Cmd.none
  )



-- UPDATE


type Msg
  = LoginAnswer (Result Http.Error String)
  | EditName String
  | LogIn


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
            , url = "http://localhost:8080/Login"
            , expect = Http.expectJson LoginAnswer loginDecoder
            }
        )
    LoginAnswer result ->
      case result of
        Ok fullText ->
          ( { model | status = LoggedIn }
          , Cmd.none
          )
        Err httpErr ->
          ( { model | status = stringHttpError httpErr }
          , Cmd.none
          )

loginDecoder : Decoder String
loginDecoder = field "acceptedUserName" string

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
      pre [] [ text "LOGGED IN" ]

    Error msg ->
      text ("oh no, error: " ++ msg)
