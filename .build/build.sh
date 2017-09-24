#!/bin/bash
set -e

REPO_ROOT=`dirname "$0"`; REPO_ROOT=`eval "cd \"$REPO_ROOT/..\" && pwd"`

cd $REPO_ROOT

source $REPO_ROOT/.build/conventions.sh

if [ -z "$BUILD_ARTIFACTSTAGINGDIRECTORY" ]
then
    BUILD_ARTIFACTSTAGINGDIRECTORY=$FALLBACK_ARTIFACTS_DIRECTORY
    mkdir -p $BUILD_ARTIFACTSTAGINGDIRECTORY
fi

echo "Pre-Build Diagnostics:"
echo "----------------------"
echo "DOCKER VERSION:"
docker version
echo "----------------------"

docker-compose -f docker-compose.ci.build.yml up --build 

buildResult=$(docker-compose -f docker-compose.ci.build.yml ps -q | xargs docker inspect -f '{{ .State.ExitCode }}' | grep -v 0 | wc -l | tr -d ' ')

if [ "$buildResult" -ne 0 ]
then
    echo ""
    echo "============================================"
    echo "build failed:"
    docker-compose -f docker-compose.ci.build.yml ps 
    echo "============================================"
    exit 1
fi

docker-compose build

docker save build.artifact:latest -o $BUILD_ARTIFACTSTAGINGDIRECTORY/build.artifact.tar.gz

git rev-parse HEAD > $BUILD_ARTIFACTSTAGINGDIRECTORY/build.artifact.commit.sha

if [ -d "$BUILD_ARTIFACTSTAGINGDIRECTORY/.build" ]
then
    rm -rf "$BUILD_ARTIFACTSTAGINGDIRECTORY/.build"
fi

mkdir -p "$BUILD_ARTIFACTSTAGINGDIRECTORY/.build"

cp -r "$REPO_ROOT/.build" "$BUILD_ARTIFACTSTAGINGDIRECTORY"

mkdir -p "$BUILD_ARTIFACTSTAGINGDIRECTORY/MLS.Agent.Integration.Tests"

cp -r "$REPO_ROOT/MLS.Agent.Integration.Tests" "$BUILD_ARTIFACTSTAGINGDIRECTORY"
