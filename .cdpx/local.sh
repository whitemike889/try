#!/bin/sh
set -e

REPO_ROOT=`dirname "$0"`; REPO_ROOT=`eval "cd \"$REPO_ROOT/..\" && pwd"`
cd $REPO_ROOT
source $REPO_ROOT/.cdpx/conventions.sh

mkdir -p ~/.config/NuGet/ && cp NuGet.Config.tmp ~/.config/NuGet/NuGet.Config
mkdir -p ~/.nuget/NuGet/ && cp NuGet.Config.tmp ~/.nuget/NuGet/NuGet.Config

echo restoring...
$REPO_ROOT/.cdpx/restore.sh

echo building...
$REPO_ROOT/.cdpx/build.sh

echo testing...
$REPO_ROOT/.cdpx/test.sh

t-rex --path $REPO_ROOT 
