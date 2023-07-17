FROM mcr.microsoft.com/dotnet/sdk:7.0-alpine as BUILD

RUN apk add -U bash build-base clang icu-libs krb5-libs libgcc libintl libssl1.1 libstdc++ zlib-dev
WORKDIR /build
COPY . .

RUN dotnet publish src/Alma/Alma.csproj -c Release -r linux-musl-x64 -p:PublishAot=true -o /app

FROM alpine:edge as FINAL

RUN mkdir /data
WORKDIR /data

ENV ALMA_APP_DATA_FALLBACK=/appdata
ENV ALMA_CONFIG_FALLBACK=/appconfig

RUN mkdir /appdata
RUN mkdir /appconfig

RUN apk add -U icu-libs libstdc++

COPY --from=BUILD /app/Alma /alma

ENTRYPOINT ["/alma"]
CMD ["help"]