#!/bin/bash
set -e

cd /source
pwd
ls .
dotnet restore ./MLS-LS.sln
dotnet publish ./MLS-LS.sln -c Release -o ./obj/Docker/publish
cp ./MLS.Agent/Dockerfile ./MLS.Agent/obj/Docker/publish/Dockerfile