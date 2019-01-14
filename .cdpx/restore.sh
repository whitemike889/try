#!/bin/sh
set -e

# Get absolute path to source root
REPO_ROOT=`dirname "$0"`; REPO_ROOT=`eval "cd \"$REPO_ROOT/..\" && pwd"`
cd $REPO_ROOT
source $REPO_ROOT/.cdpx/conventions.sh

export NUGET_PACKAGES
export NUGET_XMLDOC_MODE=none 

# Add packages from APK
apk update
apk add xmlstarlet
apk add shadow
apk add --update nodejs
apk add --update nodejs-npm

# Add dotnet tools
dotnet tool install -g --version 1.0.87 t-rex

# Restore the project
dotnet restore $REPO_ROOT/MLS-LS.sln

# prepopulate workspaces

## Console
mkdir -p $WORKSPACES_ROOT/console
cd $WORKSPACES_ROOT/console
dotnet new console
xmlstarlet ed --inplace --insert "/Project/PropertyGroup/OutputType" --type elem -n "LangVersion" --value "7.3" console.csproj
dotnet add package Newtonsoft.Json
dotnet build /fl /p:ProvideCommandLineArgs=true

## NodaTime
mkdir -p $WORKSPACES_ROOT/nodatime.api
cd $WORKSPACES_ROOT/nodatime.api
dotnet new console
xmlstarlet ed --inplace --insert "/Project/PropertyGroup/OutputType" --type elem -n "LangVersion" --value "7.3" nodatime.api.csproj
dotnet add package Newtonsoft.Json
dotnet add package NodaTime -v 2.3.0
dotnet add package NodaTime.Testing -v 2.3.0
dotnet build /fl /p:ProvideCommandLineArgs=true

mkdir -p $WORKSPACES_ROOT/aspnet.webapi
cd $WORKSPACES_ROOT/aspnet.webapi
dotnet new webapi
xmlstarlet ed --inplace --insert "/Project/PropertyGroup/OutputType" --type elem -n "LangVersion" --value "7.3" aspnet.webapi.csproj
dotnet build /fl /p:ProvideCommandLineArgs=true
dotnet publish

mkdir -p $WORKSPACES_ROOT/xamarin.essentials.0.8.0-preview
cd $WORKSPACES_ROOT/xamarin.essentials.0.8.0-preview
dotnet new console --name console --output .
xmlstarlet ed --inplace --insert "/Project/PropertyGroup/OutputType" --type elem -n "LangVersion" --value "7.3" console.csproj
dotnet add package Xamarin.Essentials --version 0.8.0-preview
dotnet build /fl /p:ProvideCommandLineArgs=true

mkdir -p $WORKSPACES_ROOT/xunit
cd $WORKSPACES_ROOT/xunit
dotnet new xunit --name tests --output .
xmlstarlet ed --inplace --insert "/Project/PropertyGroup/OutputType" --type elem -n "LangVersion" --value "7.3" tests.csproj
rm UnitTest1.cs
dotnet build /fl /p:ProvideCommandLineArgs=true

mkdir -p $WORKSPACES_ROOT/microsoftml
cd $WORKSPACES_ROOT/microsoftml
dotnet new console 
dotnet add package Microsoft.ML --version 0.3.0
xmlstarlet ed --inplace --insert "/Project/PropertyGroup/OutputType" --type elem -n "LangVersion" --value "7.3" microsoftml.csproj
dotnet build /fl /p:ProvideCommandLineArgs=true
