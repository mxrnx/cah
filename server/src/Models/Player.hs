module Models.Player where

import Data.UUID

data Player = Player
    { pId   :: UUID
    , pName :: String
    }

