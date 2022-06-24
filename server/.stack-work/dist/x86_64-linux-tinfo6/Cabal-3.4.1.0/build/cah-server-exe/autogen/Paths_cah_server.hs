{-# LANGUAGE CPP #-}
{-# LANGUAGE NoRebindableSyntax #-}
{-# OPTIONS_GHC -fno-warn-missing-import-lists #-}
{-# OPTIONS_GHC -Wno-missing-safe-haskell-mode #-}
module Paths_cah_server (
    version,
    getBinDir, getLibDir, getDynLibDir, getDataDir, getLibexecDir,
    getDataFileName, getSysconfDir
  ) where

import qualified Control.Exception as Exception
import Data.Version (Version(..))
import System.Environment (getEnv)
import Prelude

#if defined(VERSION_base)

#if MIN_VERSION_base(4,0,0)
catchIO :: IO a -> (Exception.IOException -> IO a) -> IO a
#else
catchIO :: IO a -> (Exception.Exception -> IO a) -> IO a
#endif

#else
catchIO :: IO a -> (Exception.IOException -> IO a) -> IO a
#endif
catchIO = Exception.catch

version :: Version
version = Version [0,1,0,0] []
bindir, libdir, dynlibdir, datadir, libexecdir, sysconfdir :: FilePath

bindir     = "/home/mpm/git/cah/server/.stack-work/install/x86_64-linux-tinfo6/3a1cbaed123c5cfc9d4b635ccc4d27f1746a9cf483a919905f6506534cfd1027/9.0.2/bin"
libdir     = "/home/mpm/git/cah/server/.stack-work/install/x86_64-linux-tinfo6/3a1cbaed123c5cfc9d4b635ccc4d27f1746a9cf483a919905f6506534cfd1027/9.0.2/lib/x86_64-linux-ghc-9.0.2/cah-server-0.1.0.0-odM7B8Bet69dXKp3cRKzA-cah-server-exe"
dynlibdir  = "/home/mpm/git/cah/server/.stack-work/install/x86_64-linux-tinfo6/3a1cbaed123c5cfc9d4b635ccc4d27f1746a9cf483a919905f6506534cfd1027/9.0.2/lib/x86_64-linux-ghc-9.0.2"
datadir    = "/home/mpm/git/cah/server/.stack-work/install/x86_64-linux-tinfo6/3a1cbaed123c5cfc9d4b635ccc4d27f1746a9cf483a919905f6506534cfd1027/9.0.2/share/x86_64-linux-ghc-9.0.2/cah-server-0.1.0.0"
libexecdir = "/home/mpm/git/cah/server/.stack-work/install/x86_64-linux-tinfo6/3a1cbaed123c5cfc9d4b635ccc4d27f1746a9cf483a919905f6506534cfd1027/9.0.2/libexec/x86_64-linux-ghc-9.0.2/cah-server-0.1.0.0"
sysconfdir = "/home/mpm/git/cah/server/.stack-work/install/x86_64-linux-tinfo6/3a1cbaed123c5cfc9d4b635ccc4d27f1746a9cf483a919905f6506534cfd1027/9.0.2/etc"

getBinDir, getLibDir, getDynLibDir, getDataDir, getLibexecDir, getSysconfDir :: IO FilePath
getBinDir = catchIO (getEnv "cah_server_bindir") (\_ -> return bindir)
getLibDir = catchIO (getEnv "cah_server_libdir") (\_ -> return libdir)
getDynLibDir = catchIO (getEnv "cah_server_dynlibdir") (\_ -> return dynlibdir)
getDataDir = catchIO (getEnv "cah_server_datadir") (\_ -> return datadir)
getLibexecDir = catchIO (getEnv "cah_server_libexecdir") (\_ -> return libexecdir)
getSysconfDir = catchIO (getEnv "cah_server_sysconfdir") (\_ -> return sysconfdir)

getDataFileName :: FilePath -> IO FilePath
getDataFileName name = do
  dir <- getDataDir
  return (dir ++ "/" ++ name)
