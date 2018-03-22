FROM mls-ls/dependencies
ARG source

# set up the working directory where the Agent ASP.NET app runs
RUN mkdir -p /app
RUN groupadd app_group 
RUN useradd -r --gid app_group --home /app --shell /sbin/nologin --comment "app user" app_user 
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:4242
EXPOSE 4242
COPY ${source:-obj/Docker/publish} .

RUN chown --recursive app_user:app_group /app
RUN chown --recursive app_user:app_group /dependencies
RUN chown --recursive app_user:app_group /workspaces

# make sure that the root folder is available to app_user since this is where the nuget package cache is found
RUN chmod -R 777 /root/

USER app_user

ENTRYPOINT ["dotnet", "MLS.Agent.dll"]