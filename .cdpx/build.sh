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

dotnet publish $REPO_ROOT/MLS.Agent/MLS.Agent.csproj -c Release -o $APP_ROOT

cp $REPO_ROOT/MLS.Agent/Dockerfile $DOCKER_CONTEXT_ROOT/Dockerfile

mkdir -p $DOCKER_NUGET_PACKAGES
cp -r $NUGET_PACKAGES/. $DOCKER_NUGET_PACKAGES

# create packages
mkdir $PACKAGE_ROOT
dotnet pack $REPO_ROOT/MLS-LS.sln  -c Release --no-build -o $PACKAGE_ROOT /p:NoPackageAnalysis=true

noLeadingZeros=$(echo $CDP_BUILD_NUMBER | sed 's/^0*//')
finalVersion="$CDP_MAJOR_NUMBER_ONLY.$CDP_MINOR_NUMBER_ONLY.$noLeadingZeros"
echo "Creating npm package mls-agent-results with version" $finalVersion
cd $REPO_ROOT/MLS.Agent.Tests
npm install
npm install typescript -g
npm run build
npm version $finalVersion
npm pack
cp -r $REPO_ROOT/MLS.Agent.Tests/mls-agent-results-*.tgz $REPO_ROOT/.package/mls-agent-results