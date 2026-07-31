FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["SargentNexus.sln", "./"]
COPY ["src/SargentNexus.Client/SargentNexus.Client.csproj", "src/SargentNexus.Client/"]
RUN dotnet restore "src/SargentNexus.Client/SargentNexus.Client.csproj"

COPY . .
RUN dotnet publish "src/SargentNexus.Client/SargentNexus.Client.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM nginx:alpine AS final
WORKDIR /usr/share/nginx/html
EXPOSE 80

COPY --from=build /app/publish/wwwroot/ .
