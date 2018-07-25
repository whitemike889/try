#!/bin/sh
set -e

# Get absolute path to source root
source conventions.sh
cd $REPO_ROOT

# prepare artifacts for release stage
mkdir $RELEASE_ROOT
echo $BUILD_SOURCEVERSION > $RELEASE_ROOT/build.artifact.commit.sha
cp $REPO_ROOT/.cdpx/release.sh $RELEASE_ROOT/release.sh

dotnet publish $REPO_ROOT/MLS.Agent/MLS.Agent.csproj -c Release -o $APP_ROOT

cp $REPO_ROOT/MLS.Agent/Dockerfile $DOCKER_CONTEXT_ROOT/Dockerfile

mkdir -p $DOCKER_NUGET_PACKAGES
cp -r $NUGET_PACKAGES/. $DOCKER_NUGET_PACKAGES
