#!/bin/sh
set -e

REPO_ROOT=`dirname "$0"`; REPO_ROOT=`eval "cd \"$REPO_ROOT/..\" && pwd"`

cd $REPO_ROOT

EXIT_STATUS=0

dotnet test MLS.Agent.Tests/MLS.Agent.Tests.csproj --logger trx --diag:TestResults/MLS.Agent.Tests.log.txt || EXIT_STATUS=$?
dotnet test WorkspaceServer.Tests/WorkspaceServer.Tests.csproj --logger trx --diag:TestResults/WorkspaceServer.Tests.log.txt || EXIT_STATUS=$?

exit $EXIT_STATUS
