FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["MicroserviceFront.csproj", "."]
RUN dotnet restore "MicroserviceFront.csproj"

COPY . .
RUN dotnet publish "MicroserviceFront.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080

ENTRYPOINT ["dotnet", "MicroserviceFront.dll"]