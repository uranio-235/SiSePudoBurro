FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["View/View.csproj", "View/"]
RUN dotnet restore "View/View.csproj"
COPY . .
WORKDIR "/src/View"
RUN dotnet build "View.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "View.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "View.dll"]