FROM microsoft/aspnetcore-build:2.0

ARG source

# set up OmniSharp
RUN mkdir -p /dependencies/omnisharp 
COPY omnisharp.tar.gz ./omnisharp.tar.gz
RUN tar -xvzf ./omnisharp.tar.gz -C /dependencies/omnisharp/
RUN rm ./omnisharp.tar.gz
ENV TRYDOTNET_OMNISHARP_PATH=/dependencies/omnisharp/
ENV NUGET_PACKAGES=/root/.nuget/packages/

# set up workspaces directory
ENV TRYDOTNET_WORKSPACES_PATH=/workspaces

WORKDIR /workspaces/console
RUN dotnet new console
RUN dotnet add package Newtonsoft.Json
RUN dotnet build

WORKDIR /workspaces/nodatime.api
RUN dotnet new console
RUN dotnet add package Newtonsoft.Json
RUN dotnet add package NodaTime
RUN dotnet add package NodaTime.Testing
RUN dotnet build

# hack to get the emit plugin into the package cache
WORKDIR /workspaces/emit
RUN dotnet new library
# dotnet objects to trying to add this package, so we use some coercion and return true in order to prevent the resulting error from stopping the Docker build
RUN sed -i 's/netstandard2.0/net46/' emit.csproj
RUN dotnet add package -v 1.29.0-beta2 trydotnet.omnisharp.emit ; true

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