# Use the official .NET 8.0 SDK image for building
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj file and restore dependencies
COPY ["WinInventory.csproj", "./"]
RUN dotnet restore "WinInventory.csproj" -r linux-x64

# Copy everything else and build
COPY . .
WORKDIR "/src"
RUN dotnet build "WinInventory.csproj" -c Release -o /app/build -r linux-x64

# Publish the app
FROM build AS publish
RUN dotnet publish "WinInventory.csproj" -c Release -o /app/publish -r linux-x64 --self-contained false

# Use the .NET 8.0 runtime image for running
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

# Copy published app from build stage
COPY --from=publish /app/publish .

# Set the entry point
ENTRYPOINT ["dotnet", "WinInventory.dll"]

