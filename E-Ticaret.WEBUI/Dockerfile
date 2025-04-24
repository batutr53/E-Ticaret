
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["E-Ticaret.WEBUI.csproj", "./"]
RUN dotnet restore "./E-Ticaret.WEBUI.csproj"
COPY . .
RUN dotnet publish "E-Ticaret.WEBUI.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "E-Ticaret.WEBUI.dll"]
