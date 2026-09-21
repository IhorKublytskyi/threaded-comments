FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY dZENcode.API/dZENcode.API.csproj dZENcode.API/
COPY dZENcode.Application/dZENcode.Application.csproj dZENcode.Application/
COPY dZENcode.Core/dZENcode.Core.csproj dZENcode.Core/
COPY dZENcode.Persistence/dZENcode.Persistence.csproj dZENcode.Persistence/
RUN dotnet restore dZENcode.API/dZENcode.API.csproj

COPY . .
RUN dotnet publish dZENcode.API/dZENcode.API.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "dZENcode.API.dll"]