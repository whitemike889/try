FROM microsoft/dotnet:2.1.301-sdk-alpine

# configure environment
ENV DOTNET_TOOLS=/dependencies/.dotnet/tools
ENV NUGET_PACKAGES=/dependencies/.nuget/packages
ENV TRYDOTNET_WORKSPACES_PATH=/workspaces


# install command line tools
RUN apk update
RUN apk add xmlstarlet
RUN apk add shadow
RUN dotnet tool install -g t-rex

# set up the working directory where the Agent ASP.NET app runs
RUN mkdir -p /app
RUN groupadd app_group 
RUN useradd -r --gid app_group --home /app --shell /sbin/nologin --comment "app user" app_user 
COPY . ./

RUN chown -R app_user:app_group /app
RUN chown -R app_user:app_group /dependencies/.dotnet/tools
RUN chown -R app_user:app_group /dependencies/.nuget/packages
RUN chown -R app_user:app_group /workspaces

RUN chmod -R 777 /root/

ENV ASPNETCORE_URLS=http://+:4242
EXPOSE 4242

WORKDIR /app
USER app_user

ENTRYPOINT ["dotnet", "MLS.Agent.dll"]