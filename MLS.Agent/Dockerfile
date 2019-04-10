FROM microsoft/dotnet:2.1.503-sdk-alpine

# configure environment
ENV NUGET_PACKAGES=/dependencies/.nuget/packages
ENV TRYDOTNET_PACKAGES_PATH=/workspaces

# install command line tools
RUN apk update
RUN apk add xmlstarlet
RUN apk add shadow

# set up the working directory where the Agent ASP.NET app runs
RUN mkdir -p /app
RUN groupadd app_group 
RUN useradd -r --gid app_group --home /app --shell /sbin/nologin --comment "app user" app_user 
COPY . ./

RUN chown -R app_user:app_group /app
RUN chown -R app_user:app_group /dependencies/.nuget/packages
RUN chown -R app_user:app_group /workspaces

RUN chmod -R 777 /root/

ENV ASPNETCORE_URLS=http://+:4242
EXPOSE 4242

WORKDIR /app
USER app_user

ENV NUGET_XMLDOC_MODE=none
RUN dotnet tool install -g --version 1.0.87 t-rex
RUN dotnet new -i Microsoft.AspNetCore.Blazor.Templates::0.7.0

RUN dotnet MLS.Agent.dll pack /workspaces/blazor-ms.logging
RUN ls /workspaces/blazor-ms.logging
RUN dotnet MLS.Agent.dll install  blazor-ms.logging --add-source /workspaces/blazor-ms.logging

ENTRYPOINT ["dotnet", "MLS.Agent.dll", "hosted"]