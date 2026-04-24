FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["TaskManagementService.slnx", "./"]
COPY ["src/TaskManagement.Observer/TaskManagement.Observer.csproj", "src/TaskManagement.Observer/"]
COPY ["src/TaskManagement.Domain/TaskManagement.Domain.csproj", "src/TaskManagement.Domain/"]
COPY ["src/TaskManagement.API/TaskManagement.API.csproj", "src/TaskManagement.API/"]
COPY ["src/TaskManagement.Infrastructure/TaskManagement.Infrastructure.csproj", "src/TaskManagement.Infrastructure/"]
COPY ["tests/TaskManagement.Tests/TaskManagement.Tests.csproj", "tests/TaskManagement.Tests/"]

RUN dotnet restore "src/TaskManagement.Observer/TaskManagement.Observer.csproj"

COPY . .
RUN dotnet publish "src/TaskManagement.Observer/TaskManagement.Observer.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .

EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "TaskManagement.Observer.dll"]
