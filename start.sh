#!/bin/sh
cd ElectronFlex
dotnet build
cd ../nodejs
npm start
cd -