#!/bin/bash
set -e
PWD=$(pwd)
PWD=$CDP_USER_SOURCE_FOLDER_CONTAINER_PATH
echo $PWD
DOCKER_CONTEXT_ROOT=$PWD/MLS.Agent/obj/Docker/publish
WORKSPACES_ROOT=$DOCKER_CONTEXT_ROOT/workspaces
DEPENDENCIES_ROOT=$DOCKER_CONTEXT_ROOT/dependencies

cd /source
pwd
ls .
dotnet restore ./MLS-LS.sln
dotnet publish ./MLS-LS.sln -c Release -o ./obj/Docker/publish
cp ./MLS.Agent/Dockerfile $DOCKER_CONTEXT_ROOT/Dockerfile

export NUGET_PACKAGES=$DOCKER_CONTEXT_ROOT/packages

# acquire O#
mkdir -p ./MLS.Agent/obj/Docker/publish/dependencies/omnisharp
curl -SL https://github.com/OmniSharp/omnisharp-roslyn/releases/download/v1.29.0-beta1/omnisharp-linux-x64.tar.gz | tar -xvz -C $DEPENDENCIES_ROOT/omnisharp

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

# hack to get the emit plugin into the package cache
mkdir -p $WORKSPACES_ROOT/emit
pushd $WORKSPACES_ROOT/emit
dotnet new library
# dotnet objects to trying to add this package, so we use some coercion and return true in order to prevent the resulting error from stopping the Docker build
sed -i 's/netstandard2.0/net46/' emit.csproj
dotnet add package -v 1.29.0-beta2 trydotnet.omnisharp.emit ; true
popd
