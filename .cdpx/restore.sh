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

#add Blazor templates
dotnet new -i Microsoft.AspNetCore.Blazor.Templates::0.7.0

# Restore the project
dotnet restore $REPO_ROOT/MLS-LS.sln

# force MLS.WasmCodeRunner package into cache
mkdir -p /tmp/WasmCodeRunner
cd /tmp/WasmCodeRunner
dotnet new console
dotnet add package MLS.WasmCodeRunner --version 1.0.7880001-alpha-c895bf25

# prepopulate workspaces

## Console
mkdir -p $WORKSPACES_ROOT/console
cd $WORKSPACES_ROOT/console
dotnet new console
xmlstarlet ed --inplace --insert "/Project/PropertyGroup/OutputType" --type elem -n "LangVersion" --value "7.3" console.csproj
dotnet add package Newtonsoft.Json
dotnet build /bl:package_fullBuild.binlog

## NodaTime
mkdir -p $WORKSPACES_ROOT/nodatime.api
cd $WORKSPACES_ROOT/nodatime.api
dotnet new console
xmlstarlet ed --inplace --insert "/Project/PropertyGroup/OutputType" --type elem -n "LangVersion" --value "7.3" nodatime.api.csproj
dotnet add package Newtonsoft.Json
dotnet add package NodaTime -v 2.4.0
dotnet add package NodaTime.Testing -v 2.4.0
dotnet build /bl:package_fullBuild.binlog

mkdir -p $WORKSPACES_ROOT/aspnet.webapi
cd $WORKSPACES_ROOT/aspnet.webapi
dotnet new webapi
xmlstarlet ed --inplace --insert "/Project/PropertyGroup/OutputType" --type elem -n "LangVersion" --value "7.3" aspnet.webapi.csproj
dotnet build /bl:package_fullBuild.binlog
dotnet publish

mkdir -p $WORKSPACES_ROOT/xamarin.essentials.0.8.0-preview
cd $WORKSPACES_ROOT/xamarin.essentials.0.8.0-preview
dotnet new console --name console --output .
xmlstarlet ed --inplace --insert "/Project/PropertyGroup/OutputType" --type elem -n "LangVersion" --value "7.3" console.csproj
dotnet add package Xamarin.Essentials --version 0.8.0-preview
dotnet build /bl:package_fullBuild.binlog

mkdir -p $WORKSPACES_ROOT/xunit
cd $WORKSPACES_ROOT/xunit
dotnet new xunit --name tests --output .
xmlstarlet ed --inplace --insert "/Project/PropertyGroup/OutputType" --type elem -n "LangVersion" --value "7.3" tests.csproj
rm UnitTest1.cs
dotnet build /bl:package_fullBuild.binlog

mkdir -p $WORKSPACES_ROOT/microsoftml
cd $WORKSPACES_ROOT/microsoftml
dotnet new console 
dotnet add package Microsoft.ML --version 0.3.0
xmlstarlet ed --inplace --insert "/Project/PropertyGroup/OutputType" --type elem -n "LangVersion" --value "7.3" microsoftml.csproj
dotnet build /bl:package_fullBuild.binlog

#blazor-console
mkdir -p $WORKSPACES_ROOT/blazor-console
cd $WORKSPACES_ROOT/blazor-console
dotnet new classlib
dotnet add package Newtonsoft.Json
xmlstarlet ed --inplace --insert "/Project/PropertyGroup/OutputType" --type elem -n "LangVersion" --value "7.3" blazor-console.csproj
dotnet build /bl:package_fullBuild.binlog

#humanizer.api
mkdir -p $WORKSPACES_ROOT/humanizer.api
cd $WORKSPACES_ROOT/humanizer.api
dotnet new classlib
dotnet add package Newtonsoft.Json
dotnet add package Humanizer
xmlstarlet ed --inplace --insert "/Project/PropertyGroup/OutputType" --type elem -n "LangVersion" --value "7.3" humanizer.api.csproj
dotnet build /bl:package_fullBuild.binlog

## NodaTime
mkdir -p $WORKSPACES_ROOT/blazor-nodatime.api
cd $WORKSPACES_ROOT/blazor-nodatime.api
dotnet new classlib
xmlstarlet ed --inplace --insert "/Project/PropertyGroup/OutputType" --type elem -n "LangVersion" --value "7.3" blazor-nodatime.api.csproj
dotnet add package Newtonsoft.Json
dotnet add package NodaTime -v 2.4.0
dotnet add package NodaTime.Testing -v 2.4.0
dotnet build /bl:package_fullBuild.binlog

## Logging
mkdir -p $WORKSPACES_ROOT/blazor-ms.logging
cd $WORKSPACES_ROOT/blazor-ms.logging
dotnet new classlib
xmlstarlet ed --inplace --insert "/Project/PropertyGroup/OutputType" --type elem -n "LangVersion" --value "7.3" blazor-ms.logging.csproj
dotnet add package Microsoft.Extensions.Logging
dotnet build /bl:package_fullBuild.binlog