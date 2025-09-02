# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
#FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/aspnet:9.0-alpine-arm64v8 AS base
USER root
WORKDIR /app 
EXPOSE 8080
 
FROM base AS setup

# This stage is used to build the service project
#FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build 
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build 
WORKDIR /src
 
ARG VERSION_PREFIX
ARG VERSION_SUFFIX
 
COPY ["src/Directory.Build.props","src/NuGet.Config", "./"]

COPY src/TutorBot.API.sln .
COPY src/TutorBot.Abstractions/*.csproj ./TutorBot.Abstractions/
COPY src/TutorBot.API/*.csproj ./TutorBot.API/
COPY src/TutorBot.App/*.csproj ./TutorBot.App/
COPY src/TutorBot.Authentication/*.csproj ./TutorBot.Authentication/
COPY src/TutorBot.Core/*.csproj ./TutorBot.Core/
COPY src/TutorBot.Frontend/*.csproj ./TutorBot.Frontend/
COPY src/TutorBot.Primitives/*.csproj ./TutorBot.Primitives/
COPY src/TutorBot.ServiceDefaults/*.csproj ./TutorBot.ServiceDefaults/
COPY src/TutorBot.TelegrammService/*.csproj ./TutorBot.TelegrammService/
COPY src/TutorBot.Test/*.csproj ./TutorBot.Test/
ARG TARGETARCH

#RUN dotnet restore
RUN \
    --mount=type=cache,id=nuget-arm,target=/root/.nuget/packages \
    dotnet restore -r linux-arm64

#WORKDIR /src

# Copy all the files
COPY src .
 
# Publish project
# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish 
  
ARG BUILD_VERSION=1.0.0
ARG VERSION_SUFFIX=alpha 

WORKDIR "/src/TutorBot.App"
RUN \
    --mount=type=cache,id=nuget-arm,target=/root/.nuget/packages \
    dotnet publish "TutorBot.App.csproj" \
      -r linux-arm64 \ 
      -c Release \
      -o /app/publish \
      /p:VersionPrefix=$BUILD_VERSION \ 
      /p:VersionSuffix=$VERSION_SUFFIX \
      --no-restore \ 
      --no-self-contained
       
# Build final image with all the layers 
# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration) 
FROM setup AS final
WORKDIR /app
COPY --from=publish /app/publish . 
 
#######
ENTRYPOINT ["dotnet", "TutorBot.App.dll"]