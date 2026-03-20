# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy csproj and restore
COPY Asambleas/Asambleas.csproj Asambleas/
RUN dotnet restore Asambleas/Asambleas.csproj

# Copy everything and build
COPY . .
WORKDIR /src/Asambleas
RUN dotnet publish -c Release -o /app/publish

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:5263
ENV ASPNETCORE_ENVIRONMENT=Production

EXPOSE 5263

ENTRYPOINT ["dotnet", "Asambleas.dll"]
