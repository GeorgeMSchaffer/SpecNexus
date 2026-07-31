FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["SargentNexus.sln", "./"]
COPY ["src/SargentNexus.API/SargentNexus.API.csproj", "src/SargentNexus.API/"]
COPY ["src/SargentNexus.Application/SargentNexus.Application.csproj", "src/SargentNexus.Application/"]
COPY ["src/SargentNexus.Domain/SargentNexus.Domain.csproj", "src/SargentNexus.Domain/"]
COPY ["src/SargentNexus.Infrastructure/SargentNexus.Infrastructure.csproj", "src/SargentNexus.Infrastructure/"]
RUN dotnet restore "src/SargentNexus.API/SargentNexus.API.csproj"

COPY . .
RUN dotnet publish "src/SargentNexus.API/SargentNexus.API.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SargentNexus.API.dll"]
