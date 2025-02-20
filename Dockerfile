# Sử dụng image chính thức của .NET runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["ZenGarden/ZenGarden.csproj", "ZenGarden/"]
RUN dotnet restore "ZenGarden/ZenGarden.csproj"

COPY . .
WORKDIR "/src/ZenGarden"
RUN dotnet build -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ZenGarden.dll"]
