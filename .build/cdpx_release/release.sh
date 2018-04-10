#!/bin/bash
set -e

ls .

echo "Starting Deployment"
echo $CDPXUSER $CDPXPASSWORD
docker login  -u $CDPXUSER -p $CDPXPASSWORD cdpxlinuxtest.azurecr.io

docker pull cdpxlinuxtest.azurecr.io/artifact/150d87b4-235b-4cbb-a47e-b3d8eb541563/pullrequest/mls.agent:1.0.0alpha

echo "PWD:"
echo $PWD

SCRIPT_ROOT=`dirname "$0"`; SCRIPT_ROOT=`eval "cd \"$SCRIPT_ROOT\" && pwd"`
REPO_ROOT=`eval "cd \"$SCRIPT_ROOT/..\" && pwd"`

echo $REPO_ROOT

COMMIT_HASH=$(<"$REPO_ROOT/../build.artifact.commit.sha")


docker tag cdpxlinuxtest.azurecr.io/artifact/150d87b4-235b-4cbb-a47e-b3d8eb541563/pullrequest/mls.agent:1.0.0alpha trydotnetcdpx.azurecr.io/mls.agent:$COMMIT_HASH

docker login -u $TDNUSER -p $TDNPASSWORD  trydotnetcdpx.azurecr.io
docker push trydotnetcdpx.azurecr.io/mls.agent:$COMMIT_HASH

docker tag cdpxlinuxtest.azurecr.io/artifact/150d87b4-235b-4cbb-a47e-b3d8eb541563/pullrequest/mls.agent:1.0.0alpha trydotnetcdpx.azurecr.io/mls.agent:latest
docker push trydotnetcdpx.azurecr.io/mls.agent:latest