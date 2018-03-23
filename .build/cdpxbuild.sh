#!/bin/bash
set -e

cd /source
pwd
ls .
dotnet restore ./MLS-LS.sln
dotnet publish ./MLS-LS.sln -c Release -o ./obj/Docker/publish
cp ./MLS.Agent/Dockerfile ./MLS.Agent/obj/Docker/publish/Dockerfile
mkdir -p ./MLS.Agent/obj/Docker/publish/dependencies/omnisharp
curl -L https://github.com/OmniSharp/omnisharp-roslyn/releases/download/v1.29.0-beta1/omnisharp-linux-x64.tar.gz -o ./MLS.Agent/obj/Docker/publish/omnisharp.tar.gz
