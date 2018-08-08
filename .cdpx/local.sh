#!/bin/sh
set -e

REPO_ROOT=`dirname "$0"`; REPO_ROOT=`eval "cd \"$REPO_ROOT/..\" && pwd"`
cd $REPO_ROOT
source $REPO_ROOT/.cdpx/conventions.sh

echo restoring...
$REPO_ROOT/.cdpx/restore.sh

echo building...
$REPO_ROOT/.cdpx/build.sh

echo testing...
$REPO_ROOT/.cdpx/test.sh

$DOTNET_TOOLS/t-rex --path $REPO_ROOT --show-test-output