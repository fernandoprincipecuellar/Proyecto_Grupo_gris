# ETAPA 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build-env
WORKDIR /app

COPY *.csproj ./
RUN dotnet restore

COPY . ./
RUN dotnet publish -c Release -o out

# ETAPA 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build-env /app/out .

ENV APP_NET_CORE=Proyecto_Grupo_gris.dll

CMD ["sh", "-c", "ASPNETCORE_URLS=http://*:$PORT dotnet $APP_NET_CORE"]