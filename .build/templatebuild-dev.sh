#!/bin/bash
set -e
REPO_ROOT=`dirname "$0"`; REPO_ROOT=`eval "cd \"$REPO_ROOT/..\" && pwd"`

cd $REPO_ROOT/express/template-generator
npm i
npm run build
