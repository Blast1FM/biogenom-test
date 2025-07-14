FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 5269
EXPOSE 38170
EXPOSE 7002

ENV ASPNETCORE_URLS=http://+:5269

USER app
FROM --platform=$BUILDPLATFORM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG configuration=Release
WORKDIR /src
COPY ["biogenom-test/biogenom-test.csproj", "biogenom-test/"]
RUN dotnet restore "biogenom-test/biogenom-test.csproj"
COPY . .
WORKDIR "/src/biogenom-test"
RUN dotnet build "biogenom-test.csproj" -c $configuration -o /app/build

FROM build AS publish
ARG configuration=Release
RUN dotnet publish "biogenom-test.csproj" -c $configuration -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "biogenom-test.dll"]