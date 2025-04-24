# Build aşaması
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Proje dosyasını kopyala ve restore yap
COPY ["E-Ticaret.WEBUI/E-Ticaret.WEBUI.csproj", "E-Ticaret.WEBUI/"]
RUN dotnet restore "E-Ticaret.WEBUI/E-Ticaret.WEBUI.csproj"

# Tüm dosyaları kopyala ve publish et
COPY . .
WORKDIR /src/E-Ticaret.WEBUI
RUN dotnet publish -c Release -o /app/publish --no-restore

# Runtime aşaması
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app

# Publish edilen dosyaları kopyala
COPY --from=build /app/publish .

# Uygulamayı başlat
ENTRYPOINT ["dotnet", "E-Ticaret.WEBUI.dll"]
