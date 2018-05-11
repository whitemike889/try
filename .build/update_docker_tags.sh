function update_docker_tags() {
    if [ -z "$DOCKER_REPOSITORY_SERVER" ]
    then
        echo "DOCKER_REPOSITORY_SERVER not provided"
        FAILED=1
    fi

    if [ -z "$COMMIT_HASH" ]
    then
        echo "COMMIT_HASH not provided"
        FAILED=1
    fi

    if [ -n "$FAILED" ]
    then
        echo "Tagging failed. Exiting."
        exit 1
    fi

    docker tag build.artifact:latest $DOCKER_REPOSITORY_SERVER/$IMAGE_NAME:$COMMIT_HASH

    # Tag images for devdiv CR
    docker tag $DOCKER_REPOSITORY_SERVER/$IMAGE_NAME:$COMMIT_HASH $DOCKER_REPOSITORY_SERVER/$IMAGE_NAME:staging
    docker tag $DOCKER_REPOSITORY_SERVER/$IMAGE_NAME:$COMMIT_HASH $DOCKER_REPOSITORY_SERVER/$IMAGE_NAME:latest

    # Tag images for msazure CR
    docker tag build.artifact:latest $MSAZURE_DOCKER_SERVER/$IMAGE_NAME:$COMMIT_HASH
    docker tag $MSAZURE_DOCKER_SERVER/$IMAGE_NAME:$COMMIT_HASH $MSAZURE_DOCKER_SERVER/$IMAGE_NAME:latest

    docker rmi build.artifact:latest

    return 0
}
