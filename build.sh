#!/bin/sh
cd ElectronFlex
dotnet publish -c Release
cd -
cp -rf wwwroot ElectronFlex/bin/Release/net5.0/publish/
cd nodejs
electron-builder
cd ..
mkdir -p build
mv nodejs/dist build/"$(date +"%Y-%m-%d")"