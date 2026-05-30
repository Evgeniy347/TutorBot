# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Minimal base image — only native deps, no shared .NET runtime
FROM alpine:3.21 AS base
RUN apk add --no-cache \
    libstdc++ \
    libgcc \
    openssl \
    zlib \
    zstd-libs \
    krb5-libs
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
    dotnet restore -r linux-musl-arm64
                                    
# Copy all the files
COPY src ./src

# Publish project
FROM build AS publish 

ARG BUILD_VERSION=1.0.0
ARG VERSION_SUFFIX=alpha

WORKDIR /build/src/TutorBot.App
RUN \
    --mount=type=cache,id=nuget-arm,target=/root/.nuget/packages \
    dotnet publish "TutorBot.App.csproj" \
      -r linux-musl-arm64 \
      -c Release \
      -o /app/publish \
      /p:VersionPrefix=$BUILD_VERSION \
      /p:VersionSuffix=$VERSION_SUFFIX \
      --self-contained \
      /p:PublishTrimmed=true \
      /p:SuppressTrimAnalysisWarnings=true \
      /p:DebuggerSupport=true \
      /p:EventSourceSupport=true \
      /p:InvariantGlobalization=true \
      /p:EnableConfigurationBindingGenerator=false

# Remove Roslyn compiler DLLs (~8 MB) — not needed, BlazorRuntimeCompilation=false
RUN find /app/publish -name 'Microsoft.CodeAnalysis*' -delete

# Remove extra Radzen CSS themes (~7 MB) — keep only material
RUN find /app/publish/wwwroot/_content/Radzen.Blazor/css \
    -type f ! -name 'material*' -delete

# Remove localization resource DLLs (~5 MB) — keep only ru
RUN find /app/publish -type d -name 'cs' -o -name 'de' -o -name 'es' -o -name 'fr' \
    -o -name 'it' -o -name 'ja' -o -name 'ko' -o -name 'pl' -o -name 'pt-BR' \
    -o -name 'tr' -o -name 'zh-Hans' -o -name 'zh-Hant' | xargs -r rm -rf

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

ENTRYPOINT ["./TutorBot.App"]
