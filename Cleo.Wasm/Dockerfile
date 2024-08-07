#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
COPY ["fonts/", "app/fonts/"]
RUN mkdir -p /usr/share/fonts/truetype/
RUN install -m644 /app/fonts/*.ttf /usr/share/fonts/truetype/
RUN apt-get update
RUN apt-get install -y libfreetype6 libfontconfig1
USER app
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Cleo.Wasm/Cleo.Wasm.csproj", "Cleo.Wasm/"]
RUN dotnet restore "./Cleo.Wasm/Cleo.Wasm.csproj"
COPY . .
WORKDIR "/src/Cleo.Wasm"
RUN dotnet build "./Cleo.Wasm.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Cleo.Wasm.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENV ASPNETCORE_HTTP_PORTS=80
ENTRYPOINT ["dotnet", "Cleo.Wasm.dll"]