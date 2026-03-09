# -------- BUILD STAGE --------
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# copy project file first for better cache
COPY *.csproj ./
RUN dotnet restore

# copy rest of source
COPY . ./

# publish optimized build
RUN dotnet publish -c Release -o /app/publish \
    /p:UseAppHost=false

# -------- RUNTIME STAGE --------
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "DataLabelProject.dll"]