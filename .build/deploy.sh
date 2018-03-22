#!/bin/bash
set -e

echo "Starting Deployment"

SCRIPT_ROOT=`dirname "$0"`; SCRIPT_ROOT=`eval "cd \"$SCRIPT_ROOT\" && pwd"`
REPO_ROOT=`eval "cd \"$SCRIPT_ROOT/..\" && pwd"`

CONTAINER_NAME=

if [ -z "$BUILD_DROP" ]
then
    BUILD_DROP=$REPO_ROOT/artifacts
fi

cd $REPO_ROOT

echo "Sourcing conventions from '$SCRIPT_ROOT/conventions.sh'"
source $SCRIPT_ROOT/conventions.sh

echo "Sourcing Dependencies from $SCRIPT_ROOT"

source $SCRIPT_ROOT/docker_login.sh
source $SCRIPT_ROOT/update_docker_tags.sh
source $SCRIPT_ROOT/restart_appservice.sh

echo "Loading Commit Hash from $BUILD_DROP/build.artifact.commit.sha"

COMMIT_HASH=$(<"$BUILD_DROP/build.artifact.commit.sha")

echo "Pre-Deploy Diagnostics:"
echo "----------------------"
echo "IMAGE_NAME: $IMAGE_NAME"
echo "COMMIT_HASH: $COMMIT_HASH"
echo "DOCKER VERSION:"
docker version
echo "----------------------"
echo "DOCKER IMAGES"
docker images
echo "----------------------"

do_docker_login 

docker load -i=$BUILD_DROP/build.artifact.tar.gz

update_docker_tags

docker push $DOCKER_REPOSITORY_SERVER/$IMAGE_NAME:$COMMIT_HASH

docker push $DOCKER_REPOSITORY_SERVER/$IMAGE_NAME:staging

dotnet restore $REPO_ROOT/.build/MLS.ReleaseManager/src/MLS.ReleaseManager/MLS.ReleaseManager.csproj
    
restart_appservice "trydotnetagent-staging"

dotnet restore $REPO_ROOT/MLS.Agent.Integration.Tests/MLS.Agent.Integration.Tests.csproj

dotnet test $REPO_ROOT/MLS.Agent.Integration.Tests/MLS.Agent.Integration.Tests.csproj --logger trx

docker push $DOCKER_REPOSITORY_SERVER/$IMAGE_NAME:latest

restart_appservice "trydotnetagent"
