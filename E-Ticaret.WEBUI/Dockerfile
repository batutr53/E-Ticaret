FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["E-Ticaret.WEBUI/E-Ticaret.WEBUI.csproj", "E-Ticaret.WEBUI/"]
WORKDIR /src/E-Ticaret.WEBUI
RUN dotnet restore

COPY . .
RUN dotnet publish -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "E-Ticaret.WEBUI.dll"]
