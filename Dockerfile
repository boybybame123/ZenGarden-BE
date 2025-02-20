# Sử dụng image chính thức của .NET runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Sao chép file solution và project để thực hiện restore
COPY ["ZenGarden.sln", "."]
COPY ["ZenGarden.API/ZenGarden.API.csproj", "ZenGarden.API/"]
COPY ["ZenGarden.Core/ZenGarden.Core.csproj", "ZenGarden.Core/"]
COPY ["ZenGarden.Domain/ZenGarden.Domain.csproj", "ZenGarden.Domain/"]
COPY ["ZenGarden.Infrastructure/ZenGarden.Infrastructure.csproj", "ZenGarden.Infrastructure/"]
COPY ["ZenGarden.Shared/ZenGarden.Shared.csproj", "ZenGarden.Shared/"]

# Chạy restore trước để tận dụng Docker caching
RUN dotnet restore "ZenGarden.sln"

# Sao chép toàn bộ source code vào container
COPY . .

# Đặt thư mục làm việc thành API để build
WORKDIR "/src/ZenGarden.API"

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
