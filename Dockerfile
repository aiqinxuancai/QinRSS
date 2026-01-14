FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY QinRSS/QinRSS.csproj QinRSS/
RUN dotnet restore QinRSS/QinRSS.csproj
COPY . .
WORKDIR /src/QinRSS
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/runtime:9.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "QinRSS.dll"]
