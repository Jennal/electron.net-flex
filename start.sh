#!/bin/sh
cd ElectronFlex
dotnet build
cd -
cp -rf wwwroot ElectronFlex/bin/Debug/net5.0/
cd nodejs
npm start
cd -