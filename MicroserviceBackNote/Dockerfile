FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["MicroserviceBackNote.csproj", "."]
RUN dotnet restore "MicroserviceBackNote.csproj"

COPY . .
RUN dotnet publish "MicroserviceBackNote.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "MicroserviceBackNote.dll"]FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["MicroserviceBackNote.csproj", "."]
RUN dotnet restore "MicroserviceBackNote.csproj"

COPY . .
RUN dotnet publish "MicroserviceBackNote.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "MicroserviceBackNote.dll"]