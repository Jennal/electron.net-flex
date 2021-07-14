#!/bin/sh
dotnet build
cd nodejs
npm start
cd -