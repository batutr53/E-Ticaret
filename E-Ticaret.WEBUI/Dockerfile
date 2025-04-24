FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Proje dosyasını kopyala
COPY ["E-Ticaret.WEBUI/E-Ticaret.WEBUI.csproj", "E-Ticaret.WEBUI/"]
WORKDIR /src/E-Ticaret.WEBUI
RUN dotnet restore

# Tüm dosyaları kopyala ve publish et
COPY . .
RUN dotnet publish "E-Ticaret.WEBUI.csproj" -c Release -o /app/publish

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "E-Ticaret.WEBUI.dll"]
