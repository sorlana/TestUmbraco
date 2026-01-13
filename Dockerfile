
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY *.sln .
COPY ["TestUmbraco/TestUmbraco.csproj", "TestUmbraco/"]
COPY ["TestUmbraco.Application/TestUmbraco.Application.csproj", "TestUmbraco.Application/"]
COPY ["TestUmbraco.Domain/TestUmbraco.Domain.csproj", "TestUmbraco.Domain/"]
RUN dotnet restore 

COPY ["TestUmbraco/.", "./TestUmbraco/"]
COPY ["TestUmbraco.Application/.", "./TestUmbraco.Application/"]
COPY ["TestUmbraco.Domain/.", "./TestUmbraco.Domain/"]

WORKDIR "/src/TestUmbraco"

RUN dotnet publish "TestUmbraco.csproj" -c Release -o /app/publish

# Copy the wwwroot/media folder after publishing
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80
COPY --from=build /app/publish .

# Copy the wwwroot/media folder
COPY --from=build /src/TestUmbraco/wwwroot/media /app/wwwroot/media
ENTRYPOINT ["dotnet", "TestUmbraco.dll"]