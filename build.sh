#!/bin/sh
cd ElectronFlex
dotnet publish -c Release
cd ../nodejs
npm run build
cd ..
mkdir -p build
mv nodejs/dist build/"$(date +"%Y-%m-%d")"