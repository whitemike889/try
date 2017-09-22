FROM microsoft/aspnetcore-build:2.0
ARG source
RUN mkdir -p /app \
   && groupadd app_group \
   && useradd -r --gid app_group --home /app --shell /sbin/nologin --comment "app user" app_user 

WORKDIR /app
ENV ASPNETCORE_URLS=http://+:4242
EXPOSE 4243
COPY ${source:-obj/Docker/publish} .
# Chown all the files to the app user.
RUN chown --recursive app_user:app_group /app
# Change to the app user.
USER app_user

# Change to the app user.
USER app_user
ENTRYPOINT ["dotnet", "MLS.Agent.dll"]

