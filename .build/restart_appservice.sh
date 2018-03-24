function restart_appservice () {
    if [ -z "$TENANT_ID" ]
    then
        echo "TENANT_ID not provided."
        FAILED=1
    fi

    if [ -z "$SUBSCRIPTION_ID" ]
    then
        echo "SUBSCRIPTION_ID not provided"
        FAILED=1
    fi

    if [ -z "$RESOURCE_GROUP_NAME" ]
    then
        echo "RESOURCE_GROUP_NAME not provided"
        FAILED=1
    fi

    if [ -z "$CLIENT_ID" ]
    then
        echo "CLIENT_ID not provided"
        FAILED=1
    fi

    if [ -z "$CLIENT_SECRET" ]
    then
        echo "CLIENT_SECRET not provided"
        FAILED=1
    fi

    if [ -z "$REPO_ROOT" ]
    then
        echo "REPO_ROOT not provided"
        FAILED=1
    fi

    if [ -z "$1" ]
    then
        echo "WebApp Name not provided as parameter [1]"
        FAILED=1
    fi

    if [ -n "$FAILED" ]
    then
        echo "Login failed. Exiting. '$FAILED'"
        exit 1
    fi

    echo "Restarting service $RESOURCE_GROUP_NAME.$1"

    dotnet run -p $REPO_ROOT/.build/MLS.ReleaseManager/src/MLS.ReleaseManager/MLS.ReleaseManager.csproj \
        $TENANT_ID \
        $SUBSCRIPTION_ID \
        $RESOURCE_GROUP_NAME \
        $1 \
        $CLIENT_ID \
        $CLIENT_SECRET

    if [ $? -eq 0 ]
    then
        return 0
    else
        echo "Restart failed with exit code $?"
        exit 1
    fi
}
