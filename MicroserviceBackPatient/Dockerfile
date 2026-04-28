FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["MicroserviceBackPatient.csproj", "."]
RUN dotnet restore "MicroserviceBackPatient.csproj"

COPY . .
RUN dotnet publish "MicroserviceBackPatient.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "MicroserviceBackPatient.dll"]