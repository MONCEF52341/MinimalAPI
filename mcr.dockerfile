# syntax=docker/dockerfile:1

FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
COPY . /source
WORKDIR /source

RUN dotnet publish \
  --self-contained true \
  -p:PublishSingleFile=true \
  -p:RuntimeIdentifier=linux-musl-x64 \
  -p:PublishTrimmed=true \
  -o /app

FROM mcr.microsoft.com/dotnet/runtime-deps:9.0-alpine AS final
WORKDIR /app
COPY --from=build /app/MinimalAPI /app/MinimalAPI

EXPOSE 8080
ENTRYPOINT ["./MinimalAPI"]