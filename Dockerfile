# Build aşaması
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Solution dosyasını ve tüm proje dosyalarını kopyala
COPY ["E-Ticaret.sln", "./"]
COPY ["E-Ticaret.WEBUI/E-Ticaret.WEBUI.csproj", "E-Ticaret.WEBUI/"]
COPY ["E-Ticaret.Service/E-Ticaret.Service.csproj", "E-Ticaret.Service/"]
COPY ["E-Ticaret.Data/E-Ticaret.Data.csproj", "E-Ticaret.Data/"]
COPY ["E-Ticaret.Core/E-Ticaret.Core.csproj", "E-Ticaret.Core/"]

# Restore işlemini yap
RUN dotnet restore

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