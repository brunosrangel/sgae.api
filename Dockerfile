# Estágio 1: Imagem de Execução (Runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 3000
ENV ASPNETCORE_URLS=http://+:3000

# Estágio 2: Imagem de Compilação (SDK)
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copia o arquivo da solução e os arquivos de projeto individualmente para otimização do cache de camadas do Docker
COPY ["SgaeSolution.sln", "./"]
COPY ["Sgae.API/Sgae.API.csproj", "Sgae.API/"]
COPY ["Sgae.Application/Sgae.Application.csproj", "Sgae.Application/"]
COPY ["Sgae.Domain/Sgae.Domain.csproj", "Sgae.Domain/"]
COPY ["Sgae.Infrastructure/Sgae.Infrastructure.csproj", "Sgae.Infrastructure/"]
COPY ["Sgae.Domain.Tests/Sgae.Domain.Tests.csproj", "Sgae.Domain.Tests/"]
COPY ["Sgae.Application.Tests/Sgae.Application.Tests.csproj", "Sgae.Application.Tests/"]

# Restaura as dependências NuGet de toda a solução
RUN dotnet restore SgaeSolution.sln

# Copia todo o código fonte restante do repositório
COPY . .

# Compila o projeto da API em modo Release
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
