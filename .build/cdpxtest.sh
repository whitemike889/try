#!/bin/sh
set -e

REPO_ROOT=`dirname "$0"`; REPO_ROOT=`eval "cd \"$REPO_ROOT/..\" && pwd"`

cd $REPO_ROOT

dotnet test MLS.Agent.Tests/MLS.Agent.Tests.csproj --logger trx --diag:TestResults/MLS.Agent.Tests.log.txt

