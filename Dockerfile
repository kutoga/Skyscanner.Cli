FROM mcr.microsoft.com/dotnet/core/runtime:2.2-bionic AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS build
RUN apt-get update && apt-get -y install locales-all locales
RUN locale-gen en_US.UTF-8

RUN dotnet tool install --global Paket --version 5.203.0
ENV PATH $PATH:/root/.dotnet/tools

WORKDIR /src
COPY paket.* ./
RUN paket install

COPY ["Skyscanner.Cli/Skyscanner.Cli.csproj", "Skyscanner.Cli/"]
COPY ["Skyscanner.Cli/Skyscanner.Cli.csproj", "Skyscanner.Cli/"]
COPY ["Skyscanner.Model/Skyscanner.Model.csproj", "Skyscanner.Model/"]
COPY ["Skyscanner.Infrastructure/Skyscanner.Infrastructure.csproj", "Skyscanner.Infrastructure/"]
COPY ["Skyscanner.Domain/Skyscanner.Domain.csproj", "Skyscanner.Domain/"]
COPY . .
WORKDIR "/src/Skyscanner.Cli"
RUN dotnet build "Skyscanner.Cli.csproj" -c Release -o /app

FROM build AS test
LABEL test=true
WORKDIR /src
RUN dotnet test -c Release

FROM build AS publish
RUN dotnet publish "Skyscanner.Cli.csproj" -c Release -o /app

FROM base AS final

#=========
# Firefox: Based on https://github.com/SeleniumHQ/docker-selenium/blob/master/NodeFirefox/Dockerfile
#=========
ARG FIREFOX_VERSION=66.0.5
RUN FIREFOX_DOWNLOAD_URL=$(if [ $FIREFOX_VERSION = "latest" ] || [ $FIREFOX_VERSION = "nightly-latest" ] || [ $FIREFOX_VERSION = "devedition-latest" ]; then echo "https://download.mozilla.org/?product=firefox-$FIREFOX_VERSION-ssl&os=linux64&lang=en-US"; else echo "https://download-installer.cdn.mozilla.net/pub/firefox/releases/$FIREFOX_VERSION/linux-x86_64/en-US/firefox-$FIREFOX_VERSION.tar.bz2"; fi) \
  && apt-get update -qqy \
  && apt-get -qqy --no-install-recommends install wget firefox \
  && rm -rf /var/lib/apt/lists/* /var/cache/apt/* \
  && wget --no-verbose -O /tmp/firefox.tar.bz2 $FIREFOX_DOWNLOAD_URL \
  && apt-get -y purge firefox \
  && rm -rf /opt/firefox \
  && tar -C /opt -xjf /tmp/firefox.tar.bz2 \
  && rm /tmp/firefox.tar.bz2 \
  && mv /opt/firefox /opt/firefox-$FIREFOX_VERSION \
  && ln -fs /opt/firefox-$FIREFOX_VERSION/firefox /usr/bin/firefox

WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Skyscanner.Cli.dll"]

