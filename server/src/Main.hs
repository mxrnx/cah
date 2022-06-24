module Main where

import qualified Control.Concurrent.STM as STM
import Control.Monad.Random
import Data.IORef
import Data.Map (Map)
import qualified Data.Map as Map
import Data.UUID
import System.Random

import Web.Scotty

import Models.Player

type Players = Map UUID Player

data State = State
    { sId      :: UUID
    , sSeed    :: StdGen
    , sPlayers :: Players
    }

newPlayer :: Player -> STM.TVar State -> IO UUID
newPlayer player stateVar = do
    STM.atomically $ do
        state <- STM.readTVar stateVar
        STM.writeTVar
            stateVar
            ( state 
                { sId      = getRandom
                , sPlayers = Map.insert (sId state) player (sPlayers state)
                }
            )
        pure (sId state)

main :: IO ()
main = do
    stateVar <- STM.newTVarIO State { sId = 0, sSeed = mkStdGen 123, sPlayers = Map.empty }
    scotty 8080 $
        post "/Login" $
            html "<h1>hello</h1>"
