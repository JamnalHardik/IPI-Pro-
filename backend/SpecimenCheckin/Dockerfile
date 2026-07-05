FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/SpecimenCheckin.Api/SpecimenCheckin.Api.csproj", "src/SpecimenCheckin.Api/"]
RUN dotnet restore "src/SpecimenCheckin.Api/SpecimenCheckin.Api.csproj"
COPY . .
WORKDIR "/src/src/SpecimenCheckin.Api"
RUN dotnet publish "SpecimenCheckin.Api.csproj" -c Release -o /app/publish

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
ENTRYPOINT ["dotnet", "SpecimenCheckin.Api.dll"]
