#!/bin/sh
set -e
set -x

# Get absolute path to source root
REPO_ROOT=`dirname "$0"`; REPO_ROOT=`eval "cd \"$REPO_ROOT/..\" && pwd"`
cd $REPO_ROOT
source $REPO_ROOT/.build/conventions.sh

# Add packages from APK
apk update
apk add xmlstarlet
apk add shadow

# Add dotnet tools
dotnet tool install t-rex --version 1.0.0-preview1-004 --add-source https://www.myget.org/F/wultest/api/v3/index.json --tool-path $DOTNET_TOOLS

# Restore the project
dotnet restore $REPO_ROOT/MLS-LS.sln

export NUGET_PACKAGES=$DOCKER_CONTEXT_ROOT/packages

# prepopulate workspaces

## Console
mkdir -p $WORKSPACES_ROOT/console
pushd $WORKSPACES_ROOT/console
dotnet new console
xmlstarlet ed --inplace --insert "/Project/PropertyGroup/OutputType" --type elem -n "LangVersion" --value "7.3" console.csproj
dotnet add package Newtonsoft.Json
dotnet build /fl /p:ProvideCommandLineArgs=true
popd

## NodaTime
mkdir -p $WORKSPACES_ROOT/nodatime.api
pushd $WORKSPACES_ROOT/nodatime.api
xmlstarlet ed --inplace --insert "/Project/PropertyGroup/OutputType" --type elem -n "LangVersion" --value "7.3" nodatime.api.csproj
dotnet add package Newtonsoft.Json
dotnet add package NodaTime -v 2.3.0
dotnet add package NodaTime.Testing -v 2.3.0
dotnet build /fl /p:ProvideCommandLineArgs=true
popd

mkdir -p $WORKSPACES_ROOT/aspnet.webapi
pushd $WORKSPACES_ROOT/aspnet.webapi
dotnet new webapi
xmlstarlet ed --inplace --insert "/Project/PropertyGroup/OutputType" --type elem -n "LangVersion" --value "7.3" aspnet.webapi.csproj
dotnet build /fl /p:ProvideCommandLineArgs=true
dotnet publish
popd

mkdir -p $WORKSPACES_ROOT/instrumented
pushd $WORKSPACES_ROOT/instrumented
dotnet new console
xmlstarlet ed --inplace --insert "/Project/PropertyGroup/OutputType" --type elem -n "LangVersion" --value "7.3" instrumented.csproj
dotnet add package Newtonsoft.Json
dotnet build /fl /p:ProvideCommandLineArgs=true
popd

mkdir -p $WORKSPACES_ROOT/instrumented
pushd $WORKSPACES_ROOT/instrumented
/workspaces/xamarin.essentials.0.8.0-preview
dotnet new console --name console --output .
xmlstarlet ed --inplace --insert "/Project/PropertyGroup/OutputType" --type elem -n "LangVersion" --value "7.3" console.csproj
dotnet add package Xamarin.Essentials --version 0.8.0-preview
dotnet build /fl /p:ProvideCommandLineArgs=true
popd

mkdir -p $WORKSPACES_ROOT/xunit
pushd $WORKSPACES_ROOT/xunit
dotnet new xunit --name tests --output .
xmlstarlet ed --inplace --insert "/Project/PropertyGroup/OutputType" --type elem -n "LangVersion" --value "7.3" tests.csproj
dotnet build /fl /p:ProvideCommandLineArgs=true
popd
