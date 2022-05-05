#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["DemoServer (ARINC 834A Demo Server)/DemoServer (ARINC 834A Demo Server).csproj", "DemoServer (ARINC 834A Demo Server)/"]
RUN dotnet restore "DemoServer (ARINC 834A Demo Server)/DemoServer (ARINC 834A Demo Server).csproj"
COPY . .
WORKDIR "/src/DemoServer (ARINC 834A Demo Server)"
RUN dotnet build "DemoServer (ARINC 834A Demo Server).csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "DemoServer (ARINC 834A Demo Server).csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "DemoServer (ARINC 834A Demo Server).dll"]