FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY ["SessionService/SessionService.csproj", "SessionService/"]
COPY ["Contracts/Contracts.csproj", "Contracts/"]
RUN dotnet restore "SessionService/SessionService.csproj"
COPY . .
RUN dotnet publish "SessionService/SessionService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "SessionService.dll"]