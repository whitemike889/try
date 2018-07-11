#!/bin/sh
set -e
set -x
REPO_ROOT=`dirname "$0"`; REPO_ROOT=`eval "cd \"$REPO_ROOT/..\" && pwd"`

cd $REPO_ROOT

ls .

export TAG=$(<"build.artifact.commit.sha")
export IMAGE="mls.orchestrator"
printenv

cd $REPO_ROOT/express/template-generator
npm i
npm run build
