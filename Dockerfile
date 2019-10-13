#
# This file can be used to create a docker container that will automatically execute
# the InpaintHTTP executeable on running it. This container is based on the Mono build
# system and should run under Windows and Linux.
#
# Building: docker build -t <your tag> Dockerfile
# Example:  docker build -t inpainter/inpainter Dockerfile
#
# Running: docker run -p 8069:8069 <your tag>
# Example: docker run -p 8069:8069 inpainter/inpainter
#

# create a build container with the latest Mono version
FROM mono AS build
WORKDIR /app

# copy the project directory to the build container and build the whole solution
COPY . .
RUN nuget restore Inpainting.sln
RUN xbuild Inpainting.sln /p:Configuration=Release

# finally create the runtime container
FROM mono AS runtime

# expose port 8069 of the webserver
EXPOSE 8069

# copy the compiled release version of the InpaintHTTP sample to the app directory...
WORKDIR /app
COPY --from=build /app/Samples/InpaintHTTP/bin/Release/ ./

# ...and set the entry point of the sample
CMD [ "mono",  "./InpaintHTTP.exe" ]