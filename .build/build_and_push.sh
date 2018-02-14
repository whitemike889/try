#!/bin/bash

sudo docker-compose -f ./docker-compose.ci.build.yml up --build ci-publish-mls.agent
sudo docker-compose build
echo "trydotnetcr3\nx9wujtd6GHSn8yJs14tSy6ngahuBM=dJ" | sudo docker login trydotnetcr3.azurecr.io
sudo docker tag build.artifact:latest trydotnetcr3.azurecr.io/mls.agent
sudo docker push trydotnetcr3.azurecr.io/mls.agent
