#!/bin/bash
set -e
set -x
PWD=$(pwd)
PWD=$CDP_USER_SOURCE_FOLDER_CONTAINER_PATH
echo $PWD
DOCKER_CONTEXT_ROOT=$PWD/MLS.Agent/obj/Docker/publish
WORKSPACES_ROOT=$DOCKER_CONTEXT_ROOT/workspaces
DEPENDENCIES_ROOT=$DOCKER_CONTEXT_ROOT/dependencies
cd /source

echo $BUILD_SOURCEVERSION > build.artifact.commit.sha

pwd
ls .
dotnet restore ./MLS-LS.sln
dotnet publish ./MLS-LS.sln -c Release -o ./obj/Docker/publish
cp ./MLS.Agent/Dockerfile $DOCKER_CONTEXT_ROOT/Dockerfile

export NUGET_PACKAGES=$DOCKER_CONTEXT_ROOT/packages

# prepopulate workspaces
mkdir -p $WORKSPACES_ROOT/console
pushd $WORKSPACES_ROOT/console
dotnet new console
dotnet add package Newtonsoft.Json
dotnet build
popd

mkdir -p $WORKSPACES_ROOT/nodatime.api
pushd $WORKSPACES_ROOT/nodatime.api
dotnet new console
dotnet add package Newtonsoft.Json
dotnet add package NodaTime
dotnet add package NodaTime.Testing
dotnet build
popd
