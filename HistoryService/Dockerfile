FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["HistoryService/HistoryService.csproj", "HistoryService/"]
COPY ["Contracts/Contracts.csproj", "Contracts/"]
RUN dotnet restore "HistoryService/HistoryService.csproj"

COPY . .
RUN dotnet publish "HistoryService/HistoryService.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "HistoryService.dll"]