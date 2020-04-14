FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["DemoServer/DemoServer.csproj", "DemoServer/"]
RUN dotnet restore "DemoServer/DemoServer.csproj"
COPY . .
WORKDIR "/src/DemoServer"
RUN dotnet build "DemoServer.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "DemoServer.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "DemoServer.dll", "--urls", "https://+:8443;http://+:8080"]