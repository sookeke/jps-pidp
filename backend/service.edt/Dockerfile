FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine as build-env
WORKDIR /source

ENV DOTNET_CLI_HOME="/opt"
ENV PATH="$PATH:/opt/.dotnet/tools"

RUN dotnet tool install --global dotnet-ef

COPY *.csproj .
RUN dotnet restore -r linux-musl-x64 /p:PublishReadyToRun=true

COPY . .
RUN dotnet publish -c release -o /app -r linux-musl-x64 --self-contained true --no-restore

RUN dotnet ef migrations bundle

#----------------------------------------------------------------#
FROM mcr.microsoft.com/dotnet/sdk:6.0-alpine
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT="Production"
ENV ASPNETCORE_URLS="http://+:8080"


RUN dotnet tool install dotnet-trace --tool-path=/app


#ENV \
    #DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false \
    #LC_ALL=en_US.UTF-8 \
    #LANG=en_US.UTF-8
#RUN apk add --no-cache icu-libs


COPY logs/pidp /app/certs

COPY --from=build-env /app .
COPY --from=build-env /source/efbundle .

EXPOSE 8080

CMD ["./edt.service"]
