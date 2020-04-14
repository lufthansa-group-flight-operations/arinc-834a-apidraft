FROM mcr.microsoft.com/dotnet/core/aspnet:3.1-buster-slim AS base
WORKDIR /app
EXPOSE 5658

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-buster AS build
WORKDIR /src
COPY ["StapServer/StapServer.csproj", "StapServer/"]
RUN dotnet restore "StapServer/StapServer.csproj"
COPY . .
WORKDIR "/src/StapServer"
RUN dotnet build "StapServer.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "StapServer.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "StapServer.dll"]