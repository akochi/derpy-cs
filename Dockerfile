FROM mcr.microsoft.com/dotnet/core/sdk:3.0 AS build
WORKDIR /build
COPY . /build
RUN dotnet build -c Release

FROM mcr.microsoft.com/dotnet/core/runtime:3.0-alpine
WORKDIR /dist
COPY --from=build /build/bin/Release/netcoreapp3.0 /dist
CMD [ "/dist/derpy" ]
