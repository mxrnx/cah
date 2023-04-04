module InfoHttp exposing (..)

import Http
import Json.Decode exposing (Decoder)

type InfoError
    = BadUrl String
    | Timeout
    | NetworkError
    | BadStatus Http.Metadata String
    | BadBody String

expectJson : (Result InfoError a -> msg) -> Decoder a -> Http.Expect msg
expectJson toMsg decoder =
  Http.expectStringResponse toMsg <|
    \response ->
      case response of
        Http.BadUrl_ uri -> Err (BadUrl uri)
        Http.Timeout_ ->
          Err Timeout

        Http.NetworkError_ ->
          Err NetworkError

        Http.BadStatus_ metadata body ->
          Err (BadStatus metadata body)

        Http.GoodStatus_ _ body ->
          case Json.Decode.decodeString decoder body of
            Ok value ->
              Ok value

            Err err ->
              Err (BadBody (Json.Decode.errorToString err))