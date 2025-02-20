# Sử dụng image chính thức của .NET runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Sao chép file solution trước
COPY ["ZenGarden/ZenGarden.sln", "ZenGarden/"]

# Sao chép từng project trước khi chạy restore
COPY ["ZenGarden/ZenGarden.API/ZenGarden.API.csproj", "ZenGarden/ZenGarden.API/"]
COPY ["ZenGarden/ZenGarden.Core/ZenGarden.Core.csproj", "ZenGarden/ZenGarden.Core/"]
COPY ["ZenGarden/ZenGarden.Domain/ZenGarden.Domain.csproj", "ZenGarden/ZenGarden.Domain/"]
COPY ["ZenGarden/ZenGarden.Infrastructure/ZenGarden.Infrastructure.csproj", "ZenGarden/ZenGarden.Infrastructure/"]
COPY ["ZenGarden/ZenGarden.Shared/ZenGarden.Shared.csproj", "ZenGarden/ZenGarden.Shared/"]

# Chạy restore trước để tối ưu cache
WORKDIR /src/ZenGarden
RUN dotnet restore "ZenGarden.sln"

# Sao chép toàn bộ source code vào container
COPY . .

# Đặt thư mục làm việc thành API để build
WORKDIR "/src/ZenGarden/ZenGarden.API"

# Biên dịch ứng dụng
RUN dotnet build -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ZenGarden.API.dll"]
