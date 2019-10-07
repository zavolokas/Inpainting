FROM mcr.microsoft.com/dotnet/framework/sdk:4.7.2-20190312-windowsservercore-ltsc2019 AS build
WORKDIR /app

# copy csproj and restore as distinct layers
COPY . .
RUN nuget restore Inpainting.sln
RUN msbuild Inpainting.sln /p:Configuration=Release


FROM mcr.microsoft.com/dotnet/framework/sdk:4.7.2-20190312-windowsservercore-ltsc2019 AS runtime
EXPOSE 8069
WORKDIR /app
COPY --from=build /app/Samples/InpaintHTTP/bin/Release/ ./
ENTRYPOINT ["InpaintHTTP.exe"]