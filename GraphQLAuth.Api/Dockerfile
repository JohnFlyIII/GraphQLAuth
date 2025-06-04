FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["GraphQLAuth.Api/GraphQLAuth.Api.csproj", "GraphQLAuth.Api/"]
RUN dotnet restore "GraphQLAuth.Api/GraphQLAuth.Api.csproj"
COPY . .
WORKDIR "/src/GraphQLAuth.Api"
RUN dotnet build "GraphQLAuth.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "GraphQLAuth.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "GraphQLAuth.Api.dll"]