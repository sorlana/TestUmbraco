
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY *.sln .
COPY ["Emax/Emax.csproj", "Emax/"]
COPY ["Emax.Application/Emax.Application.csproj", "Emax.Application/"]
COPY ["Emax.Domain/Emax.Domain.csproj", "Emax.Domain/"]
RUN dotnet restore 

COPY ["Emax/.", "./Emax/"]
COPY ["Emax.Application/.", "./Emax.Application/"]
COPY ["Emax.Domain/.", "./Emax.Domain/"]

WORKDIR "/src/Emax"

RUN dotnet publish "Emax.csproj" -c Release -o /app/publish

# Copy the wwwroot/media folder after publishing
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80
COPY --from=build /app/publish .

# Copy the wwwroot/media folder
COPY --from=build /src/Emax/wwwroot/media /app/wwwroot/media
ENTRYPOINT ["dotnet", "Emax.dll"]