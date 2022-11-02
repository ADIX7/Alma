#!/bin/sh
dotnet publish src/Alma/Alma.csproj -c Release -r linux-x64 -p:PublishAot=true