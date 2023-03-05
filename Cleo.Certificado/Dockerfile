#See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
COPY ["fonts/", "app/fonts/"]
RUN apt-get update
RUN apt-get install -y libfreetype6
RUN apt-get install -y libfontconfig1
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Cleo.Certificado/Cleo.Certificado.csproj", "Cleo.Certificado/"]
RUN dotnet restore "Cleo.Certificado/Cleo.Certificado.csproj"
COPY . .
WORKDIR "/src/Cleo.Certificado"
RUN dotnet build "Cleo.Certificado.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Cleo.Certificado.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Cleo.Certificado.dll"]