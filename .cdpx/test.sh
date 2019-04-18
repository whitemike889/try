#!/bin/sh
set -e

REPO_ROOT=`dirname "$0"`; REPO_ROOT=`eval "cd \"$REPO_ROOT/..\" && pwd"`

cd $REPO_ROOT

EXIT_STATUS=0

dotnet test Microsoft.DotNet.Try.Project.Tests/Microsoft.DotNet.Try.Project.Tests.csproj --configuration Release --logger trx --diag:TestResults/Microsoft.DotNet.Try.Project.Tests.log.txt || EXIT_STATUS=$?
dotnet test Microsoft.DotNet.Try.Protocol.Tests/Microsoft.DotNet.Try.Protocol.Tests.csproj --configuration Release --logger trx --diag:TestResults/Microsoft.DotNet.Try.Protocol.Tests.log.txt || EXIT_STATUS=$?
dotnet test Microsoft.DotNet.Try.Client.Configuration.Tests/Microsoft.DotNet.Try.Client.Configuration.Tests.csproj --configuration Release --logger trx --diag:TestResults/Microsoft.DotNet.Try.Client.Configuration.Tests.log.txt || EXIT_STATUS=$?
dotnet test WorkspaceServer.Tests/WorkspaceServer.Tests.csproj --configuration Release --logger trx --diag:TestResults/WorkspaceServer.Tests.log.txt || EXIT_STATUS=$?
dotnet test Microsoft.DotNet.Try.Jupyter.Tests/Microsoft.DotNet.Try.Jupyter.Tests.csproj --configuration Release --logger trx --diag:TestResults/Microsoft.DotNet.Try.Jupyter.Tests.log.txt || EXIT_STATUS=$?
dotnet test Microsoft.DotNet.Try.Markdown.Tests/Microsoft.DotNet.Try.Markdown.Tests.csproj --configuration Release --logger trx --diag:TestResults/Microsoft.DotNet.Try.Markdown.Tests.log.txt || EXIT_STATUS=$?
dotnet test MLS.Agent.Tests/MLS.Agent.Tests.csproj --configuration Release --logger trx --diag:TestResults/MLS.Agent.Tests.log.txt || EXIT_STATUS=$?

exit $EXIT_STATUS
