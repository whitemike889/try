function do_docker_login () {
    if [ -z "$DOCKER_REPOSITORY_USER" ]
    then
        echo "DOCKER_REPOSITORY_USER not provided."
        FAILED=1
    fi

    if [ -z "$DOCKER_REPOSITORY_PASSWORD" ]
    then
        echo "DOCKER_REPOSITORY_PASSWORD not provided"
        FAILED=1
    fi

    if [ -z "$DOCKER_REPOSITORY_SERVER" ]
    then
        echo "DOCKER_REPOSITORY_SERVER not provided"
        FAILED=1
    fi

    if [ -n "$FAILED" ]
    then
        echo "Login failed. Exiting. '$FAILED'"
        exit 1
    fi

    echo "Logging into Docker Repository $DOCKER_REPOSITORY_SERVER"

    docker login -u $DOCKER_REPOSITORY_USER -p $DOCKER_REPOSITORY_PASSWORD $DOCKER_REPOSITORY_SERVER

    LOGIN_SUCCESS=$?

    if [ $LOGIN_SUCCESS -ne 0 ]
    then
        echo "Docker login failed with exit code $LOGIN_SUCCESS"
        exit 1
    fi
}
