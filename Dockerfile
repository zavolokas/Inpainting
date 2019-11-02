#
# This file can be used to create a docker container that will automatically execute
# the InpaintHTTP executeable on running it. 
#
# Building: docker build -t <your tag> Dockerfile
# Example:  docker build -t inpainter/inpainter Dockerfile
#
# Running: docker run -p 8069:8069 <your tag>
# Example: docker run -p 8069:8069 inpainter/inpainter
#

# create a build container with the .NET Core
FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build-env
WORKDIR /app

# copy some projects and build and publish the app
COPY Inpainting/ ./
COPY Samples/ ./
RUN dotnet restore Inpainting
RUN dotnet build Inpainting
RUN dotnet restore Samples/InpaintHTTP
RUN dotnet publish Samples/InpaintHTTP -c Release -o output

# Runtime image
FROM mcr.microsoft.com/dotnet/core/aspnet:2.2
WORKDIR /app
COPY --from=build-env /app/output .
ENTRYPOINT ["dotnet", "InpaintHTTP.exe"]