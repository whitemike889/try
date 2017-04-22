export IMAGE_NAME=dotnet-repl

echo "Pre-Build Diagnostics:"
echo "----------------------"
echo "IMAGE_NAME: $IMAGE_NAME"
echo "DOCKER_REPOSITORY_SERVER: $DOCKER_REPOSITORY_SERVER"
echo "----------------------"
echo "DOCKER VERSION:"
docker version
echo "----------------------"

docker-compose -f docker-compose.ci.build.yml up --build
docker-compose build

if [ -z "${PUSH_DOCKER_IMAGE+x}" ]
then
    echo "Not pushing docker image"
else
    if [ -z "${DOCKER_REPOSITORY_USER+x}" ]; then echo "DOCKER_REPOSITORY_USER env var must be set." && MISSING_ARGUMENTS=1; fi

    if [ -z "${DOCKER_REPOSITORY_PASSWORD+x}" ]; then echo "DOCKER_REPOSITORY_PASSWORD env var must be set." && MISSING_ARGUMENTS=1; fi

    if [ -z "${DOCKER_REPOSITORY_SERVER+x}" ]; then echo "DOCKER_REPOSITORY_SERVER env var must be set." && MISSING_ARGUMENTS=1 && DOCKER_REPOSITORY_SERVER=undefined; fi

    if [ -z "${BUILD_SOURCEVERSION+x}" ]; then echo "BUILD_SOURCEVERSION env var must be set." && MISSING_ARGUMENTS=1; fi

    if [ -n "$MISSING_ARGUMENTS" ] && [ "$MISSING_ARGUMENTS" -eq 1 ] ; then exit 1; fi

    docker tag $DOCKER_REPOSITORY_SERVER/$IMAGE_NAME:latest $DOCKER_REPOSITORY_SERVER/$IMAGE_NAME:$BUILD_SOURCEVERSION

    docker login -u $DOCKER_REPOSITORY_USER -p $DOCKER_REPOSITORY_PASSWORD $DOCKER_REPOSITORY_SERVER

    docker push $DOCKER_REPOSITORY_SERVER/$IMAGE_NAME:latest

    docker push $DOCKER_REPOSITORY_SERVER/$IMAGE_NAME:$BUILD_SOURCEVERSION
fi
