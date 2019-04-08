#!/bin/sh
set -e

# Get absolute path to source root
REPO_ROOT=`dirname "$0"`; REPO_ROOT=`eval "cd \"$REPO_ROOT/..\" && pwd"`

source $REPO_ROOT/.cdpx/conventions.sh
cd $REPO_ROOT

# prepare artifacts for release stage
mkdir $RELEASE_ROOT
echo $BUILD_SOURCEVERSION > $RELEASE_ROOT/build.artifact.commit.sha
cp $REPO_ROOT/.cdpx/release.sh $RELEASE_ROOT/release.sh

echo "publish"
dotnet publish $REPO_ROOT/MLS.Agent/MLS.Agent.csproj -c Release -o $APP_ROOT

cp $REPO_ROOT/MLS.Agent/Dockerfile $DOCKER_CONTEXT_ROOT/Dockerfile

mkdir -p $DOCKER_NUGET_PACKAGES
cp -r $NUGET_PACKAGES/. $DOCKER_NUGET_PACKAGES

# create packages
mkdir $PACKAGE_ROOT
echo "package"
dotnet pack $REPO_ROOT/Microsoft.DotNet.Try.Markdown/Microsoft.DotNet.Try.Markdown.csproj -c Release -o $PACKAGE_ROOT /p:NoPackageAnalysis=true
dotnet pack $REPO_ROOT/MLS.Agent/MLS.Agent.csproj -c Release -o $PACKAGE_ROOT /p:NoPackageAnalysis=true
dotnet pack $REPO_ROOT/MLS.Agent.Tools/MLS.Agent.Tools.csproj -c Release -o $PACKAGE_ROOT /p:NoPackageAnalysis=true
dotnet pack $REPO_ROOT/MLS.Protocol/MLS.Protocol.csproj -c Release -o $PACKAGE_ROOT /p:NoPackageAnalysis=true
dotnet pack $REPO_ROOT/Microsoft.DotNetTry.Project./Microsoft.DotNetTry.Project..csproj -c Release -o $PACKAGE_ROOT /p:NoPackageAnalysis=true
dotnet pack $REPO_ROOT/WasmCodeRunner/MLS.WasmCodeRunner.csproj -c Release -o $PACKAGE_ROOT /p:NoPackageAnalysis=true