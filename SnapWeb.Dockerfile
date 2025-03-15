FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base

# We don't need the standalone Chromium
ENV PUPPETEER_SKIP_CHROMIUM_DOWNLOAD true

# Install Google Chrome Stable and fonts
# Note: this installs the necessary libs to make the browser work with Puppeteer.
RUN apt-get update && apt-get install gnupg wget -y && \
  wget --quiet --output-document=- https://dl-ssl.google.com/linux/linux_signing_key.pub | gpg --dearmor > /etc/apt/trusted.gpg.d/google-archive.gpg && \
  sh -c 'echo "deb [arch=amd64] http://dl.google.com/linux/chrome/deb/ stable main" >> /etc/apt/sources.list.d/google.list' && \
  apt-get update && \
  apt-get install google-chrome-stable -y --no-install-recommends && \
  rm -rf /var/lib/apt/lists/*
  
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src/SnapWebModels
COPY ["SnapWebModels/SnapWebModels.csproj", "."]
RUN dotnet restore "SnapWebModels.csproj"
COPY SnapWebModels/ .

WORKDIR /src/SnapchatLib
COPY ["SnapchatLib/SnapchatLib.csproj", "."]
RUN dotnet restore "SnapchatLib.csproj"
COPY SnapchatLib/ .

WORKDIR /src/TaskBoard
COPY ["./TaskBoard/TaskBoard.csproj", "."]
COPY ./nuget.config .
RUN dotnet restore
COPY ./TaskBoard .

FROM build AS publish
WORKDIR /src/TaskBoard
RUN dotnet publish -c Release -a x64 -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TaskBoard.dll"]