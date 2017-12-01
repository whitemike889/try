FROM microsoft/aspnetcore-build:2.0
ARG source

RUN mkdir -p /app
RUN groupadd app_group 
RUN useradd -r --gid app_group --home /app --shell /sbin/nologin --comment "app user" app_user 
RUN mkdir -p /dependencies/omnisharp 
RUN curl -SL https://github.com/OmniSharp/omnisharp-roslyn/releases/download/v1.27.2/omnisharp-linux-x64.tar.gz | tar xvz -C /dependencies/omnisharp

WORKDIR /app
ENV ASPNETCORE_URLS=http://+:4242
EXPOSE 4242
COPY ${source:-obj/Docker/publish} .
RUN chown --recursive app_user:app_group /app
USER app_user
ENTRYPOINT ["dotnet", "MLS.Agent.dll"]