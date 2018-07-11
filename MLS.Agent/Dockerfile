FROM microsoft/dotnet:2.1.301-sdk-alpine

# configure environment
ENV DOTNET_TOOLS=/dependencies/.dotnet/tools
ENV NUGET_PACKAGES=/dependencies/.nuget/packages
ENV TRYDOTNET_WORKSPACES_PATH=/workspaces


# install command line tools
RUN apk update
RUN apk add xmlstarlet
RUN apk add shadow
RUN dotnet tool install t-rex --version 1.0.0-preview1-004 --add-source https://www.myget.org/F/wultest/api/v3/index.json --tool-path $DOTNET_TOOLS

ARG source

# set up the working directory where the Agent ASP.NET app runs
RUN mkdir -p /app
RUN groupadd app_group 
RUN useradd -r --gid app_group --home /app --shell /sbin/nologin --comment "app user" app_user 
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:4242
EXPOSE 4242
COPY . ./

RUN chown --recursive app_user:app_group /app
RUN chown --recursive app_user:app_group /dependencies
RUN chown --recursive app_user:app_group /workspaces

# make sure that the root folder is available to app_user since this is where the nuget package cache is found
RUN chmod -R 777 /root/

USER app_user

ENTRYPOINT ["dotnet", "MLS.Agent.dll"]