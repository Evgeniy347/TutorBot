# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
#FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/aspnet:10.0-alpine-arm64v8 AS base
USER root
WORKDIR /app 
EXPOSE 8080

# This stage is used to build the service project 
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:10.0 AS build 

ARG VERSION_PREFIX
ARG VERSION_SUFFIX

WORKDIR /build

COPY BuildInfo.Build.props ./
COPY Directory.Build.props ./
COPY Directory.Packages.props ./

COPY NuGet.Config .
COPY TutorBot.API.slnx .

COPY src/TutorBot.Abstractions/TutorBot.Abstractions.csproj ./src/TutorBot.Abstractions/
COPY src/TutorBot.App/TutorBot.App.csproj ./src/TutorBot.App/
COPY src/TutorBot.Authentication/TutorBot.Authentication.csproj ./src/TutorBot.Authentication/
COPY src/TutorBot.Core/TutorBot.Core.csproj ./src/TutorBot.Core/
COPY src/TutorBot.Frontend/TutorBot.Frontend.csproj ./src/TutorBot.Frontend/
COPY src/TutorBot.Primitives/TutorBot.Primitives.csproj ./src/TutorBot.Primitives/
COPY src/TutorBot.ServiceDefaults/TutorBot.ServiceDefaults.csproj ./src/TutorBot.ServiceDefaults/
COPY src/TutorBot.TelegramService/TutorBot.TelegramService.csproj ./src/TutorBot.TelegramService/
COPY src/TutorBot.IntegrationTest/TutorBot.IntegrationTest.csproj ./src/TutorBot.IntegrationTest/
COPY src/TutorBot.Test/TutorBot.Test.csproj ./src/TutorBot.Test/

ARG TARGETARCH

RUN \
    --mount=type=cache,id=nuget-arm,target=/root/.nuget/packages \
    dotnet restore -r linux-arm64
                                    
# Copy all the files
COPY src ./src

# Publish project
# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish 

ARG BUILD_VERSION=1.0.0
ARG VERSION_SUFFIX=alpha

WORKDIR /build/src/TutorBot.App
RUN \
    --mount=type=cache,id=nuget-arm,target=/root/.nuget/packages \
    dotnet publish "TutorBot.App.csproj" \
      -r linux-arm64 \
      -c Release \
      -o /app/publish \
      /p:VersionPrefix=$BUILD_VERSION \
      /p:VersionSuffix=$VERSION_SUFFIX \
      --no-restore \
      --no-self-contained \
      /p:InvariantGlobalization=true \
      /p:PublishTrimmed=true \
      /p:EnableConfigurationBindingGenerator=false

# Remove Roslyn compiler DLLs (~10 MB) — not needed for published Blazor Server
RUN rm -rf \
    /app/publish/Microsoft.CodeAnalysis*.dll \
    /app/publish/cs/Microsoft.CodeAnalysis* \
    /app/publish/de/Microsoft.CodeAnalysis* \
    /app/publish/es/Microsoft.CodeAnalysis* \
    /app/publish/fr/Microsoft.CodeAnalysis* \
    /app/publish/it/Microsoft.CodeAnalysis* \
    /app/publish/ja/Microsoft.CodeAnalysis* \
    /app/publish/ko/Microsoft.CodeAnalysis* \
    /app/publish/pl/Microsoft.CodeAnalysis* \
    /app/publish/pt-BR/Microsoft.CodeAnalysis* \
    /app/publish/ru/Microsoft.CodeAnalysis* \
    /app/publish/tr/Microsoft.CodeAnalysis* \
    /app/publish/zh-Hans/Microsoft.CodeAnalysis* \
    /app/publish/zh-Hant/Microsoft.CodeAnalysis*

# Remove extra Radzen CSS themes (~7 MB) — keep only material
RUN find /app/publish/wwwroot/_content/Radzen.Blazor/css \
    -type f ! -name 'material*' -delete

# Build final image with all the layers
# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration)
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["dotnet", "TutorBot.App.dll"]
