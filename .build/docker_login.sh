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

    if [ $LOGIN_SUCCESS -eq 0 ]
    then
        return 0
    else
        echo "Docker login failed with exit code $LOGIN_SUCCESS"
        exit 1
    fi

    # We logged into the CR used by the devdiv collection.
    # We're also logging into the one used by msazure
    docker login -u $MSAZURE_DOCKER_USER -p $MSAZURE_DOCKER_PASSWORD $MSAZURE_DOCKER_SERVER

    LOGIN_SUCCESS=$?

    if [ $LOGIN_SUCCESS -eq 0 ]
    then
        return 0
    else
        echo "Docker login failed with exit code $LOGIN_SUCCESS"
        exit 1
    fi
}
