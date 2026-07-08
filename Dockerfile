# Estágio 1: Imagem de Execução (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 3000
ENV ASPNETCORE_URLS=http://+:3000

# Estágio 2: Imagem de Compilação (SDK)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia os arquivos de projeto individualmente para otimização do cache de camadas do Docker
COPY ["Sgae.API/Sgae.API.csproj", "Sgae.API/"]
COPY ["Sgae.Application/Sgae.Application.csproj", "Sgae.Application/"]
COPY ["Sgae.Domain/Sgae.Domain.csproj", "Sgae.Domain/"]
COPY ["Sgae.Infrastructure/Sgae.Infrastructure.csproj", "Sgae.Infrastructure/"]

# Restaura as dependências NuGet apenas para os projetos de produção
RUN dotnet restore "Sgae.API/Sgae.API.csproj"

# Copia o restante do código fonte de produção
COPY ["Sgae.Domain/", "Sgae.Domain/"]
COPY ["Sgae.Application/", "Sgae.Application/"]
COPY ["Sgae.Infrastructure/", "Sgae.Infrastructure/"]
COPY ["Sgae.API/", "Sgae.API/"]

WORKDIR "/src/Sgae.API"
RUN dotnet build "Sgae.API.csproj" -c Release -o /app/build --no-restore

# Estágio 3: Publicação dos artefatos compilados
FROM build AS publish
RUN dotnet publish "Sgae.API.csproj" -c Release -o /app/publish /p:UseAppHost=false --no-restore

# Estágio 4: Imagem Final de Produção
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Sgae.API.dll"]
