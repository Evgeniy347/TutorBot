# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# This stage is used when running from VS in fast mode (Default for Debug configuration)
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER root
WORKDIR /app 
EXPOSE 8080
 
FROM base AS setup

#######
#RUN apt update 
#RUN apt install -y mc
#RUN apt install -y nginx
#RUN apt install -y curl 
  
# This stage is used to build the service project
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build 
WORKDIR /src
 
ARG VERSION_PREFIX
ARG VERSION_SUFFIX

COPY ["Directory.Build.props","NuGet.Config", "./"]

# Copy all the project files keeping their hierarchy
#COPY ["**/*.csproj", "./"] 
#RUN for file in $(find . -type f -name "*.csproj"); do \
    #mkdir -p ${file%.*}/ && mv $file ${file%.*}/; \
    #done 
#
# Restore project dependencies
#RUN dotnet restore "TutorBot.App/TutorBot.App.csproj" -r linux-arm64

COPY *.sln .
COPY TutorBot.Abstractions/*.csproj ./TutorBot.Abstractions/
COPY TutorBot.API/*.csproj ./TutorBot.API/
COPY TutorBot.App/*.csproj ./TutorBot.App/
COPY TutorBot.Authentication/*.csproj ./TutorBot.Authentication/
COPY TutorBot.Core/*.csproj ./TutorBot.Core/
COPY TutorBot.Frontend/*.csproj ./TutorBot.Frontend/
COPY TutorBot.Primitives/*.csproj ./TutorBot.Primitives/
COPY TutorBot.ServiceDefaults/*.csproj ./TutorBot.ServiceDefaults/
COPY TutorBot.TelegrammService/*.csproj ./TutorBot.TelegrammService/
COPY TutorBot.Test/*.csproj ./TutorBot.Test/

RUN dotnet restore

#WORKDIR /src

# Copy all the files
COPY . .
 
# Publish project
# This stage is used to publish the service project to be copied to the final stage
FROM build AS publish 
  
ARG BUILD_VERSION=1.0.0
ARG VERSION_SUFFIX=alpha 

WORKDIR "/src/TutorBot.App"
RUN \
    dotnet publish "TutorBot.App.csproj" \
      -c Release \
      -o /app/publish \
      /p:VersionPrefix=$BUILD_VERSION \ 
      /p:VersionSuffix=$VERSION_SUFFIX \
      /p:UseAppHost=false \
      /p:DebugType=None \
      /p:DebugSymbols=false \ 
      --no-restore \ 
      --no-self-contained
       
# Build final image with all the layers 
# This stage is used in production or when running from VS in regular mode (Default when not using the Debug configuration) 
FROM setup AS final
WORKDIR /app
COPY --from=publish /app/publish . 
 
#######
ENTRYPOINT ["dotnet", "TutorBot.App.dll"]