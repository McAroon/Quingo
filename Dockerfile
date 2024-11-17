FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Quingo/Quingo.csproj", "Quingo/"]
RUN dotnet restore "Quingo/Quingo.csproj"

COPY . .
WORKDIR "/src/Quingo"
RUN dotnet build "Quingo.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Quingo.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Quingo.dll"]