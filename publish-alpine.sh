#!/bin/sh
dotnet publish src/Alma/Alma.csproj -c Release -r linux-musl-x64 -p:PublishAot=true