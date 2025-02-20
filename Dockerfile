# Sử dụng image chính thức của .NET runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /ZenGarden  # Đặt đúng theo cấu trúc của bạn

# Copy solution file và các project files để tối ưu cache
COPY ZenGarden/ZenGarden.sln ZenGarden/
COPY ZenGarden/ZenGarden.API/ZenGarden.API.csproj ZenGarden/ZenGarden.API/
COPY ZenGarden/ZenGarden.Core/ZenGarden.Core.csproj ZenGarden/ZenGarden.Core/
COPY ZenGarden/ZenGarden.Infrastructure/ZenGarden.Infrastructure.csproj ZenGarden/ZenGarden.Infrastructure/
COPY ZenGarden/ZenGarden.Domain/ZenGarden.Domain.csproj ZenGarden/ZenGarden.Domain/
COPY ZenGarden/ZenGarden.Shared/ZenGarden.Shared.csproj ZenGarden/ZenGarden.Shared/

# Chạy restore trước để tận dụng Docker caching
WORKDIR /ZenGarden
RUN dotnet restore "ZenGarden.sln"

# Copy toàn bộ source code sau khi restore
COPY . .

# Build ứng dụng
WORKDIR "/ZenGarden/ZenGarden.API"
RUN dotnet build -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Final stage - Runtime image
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ZenGarden.API.dll"]
