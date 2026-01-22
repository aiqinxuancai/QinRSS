FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY QinRSS/QinRSS.csproj QinRSS/
RUN dotnet restore QinRSS/QinRSS.csproj
COPY . .
WORKDIR /src/QinRSS
RUN dotnet publish -c Release -o /app/publish -p:PublishAot=false

FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "QinRSS.dll"]
