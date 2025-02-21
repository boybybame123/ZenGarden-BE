# 1️⃣ Sử dụng hình ảnh chính thức của .NET SDK
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# 2️⃣ Copy tất cả các file vào container
COPY . .

# 3️⃣ Khôi phục các package
RUN dotnet restore ZenGarden/ZenGarden.sln

# 4️⃣ Biên dịch ứng dụng
WORKDIR /src/ZenGarden/ZenGarden.API
RUN dotnet build -c Release -o /app/build

# 5️⃣ Publish ứng dụng
FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

# 6️⃣ Tạo image runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Thêm các dòng này
ENV PORT=8080
ENV ASPNETCORE_URLS="http://+:${PORT};https://+:${PORT}"
EXPOSE ${PORT}

# 7️⃣ Chạy ứng dụng
ENTRYPOINT ["dotnet", "ZenGarden.API.dll"]

VOLUME /app/DataProtection-Keys