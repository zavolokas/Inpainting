FROM mono
EXPOSE 4321
ADD . /Inpainting
WORKDIR /Inpainting
RUN mono inpainting.exe restore
RUN xbuild /p:Configuration=Release
CMD [ "mono", "/src/Mono-FirstNancy/bin/Release/Mono-FirstNancy.exe" ]