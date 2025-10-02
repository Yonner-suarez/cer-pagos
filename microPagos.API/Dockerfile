# Etapa de build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copiamos todos los csproj (para cachear restores correctamente)
COPY . .

# Restauramos dependencias de todos los proyectos
RUN find . -name "*.csproj" -print0 | xargs -0 -n1 dotnet restore

# ARG para elegir el microservicio
ARG PROJECT_PATH
WORKDIR "/src/${PROJECT_PATH}"

# Publicamos el proyecto seleccionado
RUN dotnet publish -c Release -o /app/publish --no-restore

# Etapa final (runtime)
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 8080

# Render espera que la app escuche en 8080
ENV ASPNETCORE_URLS=http://+:8080

COPY --from=build /app/publish .

# Entrypoint dinámico según el proyecto
ARG PROJECT_DLL
ENTRYPOINT ["dotnet", "${PROJECT_DLL}"]

