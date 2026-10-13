FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["TeacherService/TeacherService.csproj", "TeacherService/"]
COPY ["Contracts/Contracts.csproj", "Contracts/"]
RUN dotnet restore "TeacherService/TeacherService.csproj" -r linux-x64

COPY . .
RUN dotnet publish "TeacherService/TeacherService.csproj" \
    -c Release \
    -r linux-x64 \
    --self-contained false \
    -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "TeacherService.dll"]