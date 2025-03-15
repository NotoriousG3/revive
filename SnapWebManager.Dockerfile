FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src/SnapWebModels
COPY ["SnapWebModels/SnapWebModels.csproj", "."]
RUN dotnet restore "SnapWebModels.csproj"
COPY SnapWebModels/ .
WORKDIR /src/SnapWebManager
COPY ["SnapWebManager/SnapWebManager.csproj", "."]
RUN dotnet restore "SnapWebManager.csproj"
COPY SnapWebManager/ .
RUN dotnet build "SnapWebManager.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SnapWebManager.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SnapWebManager.dll"]
