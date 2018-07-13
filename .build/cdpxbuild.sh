#!/bin/sh
set -e

# Get absolute path to source root
REPO_ROOT=`dirname "$0"`; REPO_ROOT=`eval "cd \"$REPO_ROOT/..\" && pwd"`
cd $REPO_ROOT
source $REPO_ROOT/.build/conventions.sh

# record commit hash
echo $BUILD_SOURCEVERSION > $REPO_ROOT/build.artifact.commit.sha

dotnet publish $REPO_ROOT/MLS.Agent/MLS.Agent.csproj -c Release -o $APP_ROOT

cp $REPO_ROOT/MLS.Agent/Dockerfile $DOCKER_CONTEXT_ROOT/Dockerfile
cp -r $NUGET_PACKAGES $DOCKER_NUGET_PACKAGES
