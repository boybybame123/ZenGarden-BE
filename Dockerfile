# Sử dụng image chính thức của .NET runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy file project trước để tối ưu caching
COPY ["ZenGarden/ZenGarden.API/ZenGarden.API.csproj", "ZenGarden/ZenGarden.API/"]
COPY ["ZenGarden/ZenGarden.sln", "ZenGarden/"]

# Chạy restore cho toàn bộ solution
WORKDIR /src/ZenGarden
RUN dotnet restore "ZenGarden.sln"

# Copy toàn bộ mã nguồn
COPY . .

# Build ứng dụng
WORKDIR "/src/ZenGarden/ZenGarden.API"
RUN dotnet build -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Final stage - Runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ZenGarden.API.dll"]
