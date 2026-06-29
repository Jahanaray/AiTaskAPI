FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# copy project files FIRST (for caching + proper restore)
COPY ["AiTaskApi/AiTaskApi.csproj", "AiTaskApi/"]
COPY ["AiTaskApi.Shared/AiTaskApi.Shared.csproj", "AiTaskApi.Shared/"]

# restore
RUN dotnet restore "AiTaskApi/AiTaskApi.csproj"

# copy everything
COPY . .

# build
WORKDIR /src/AiTaskApi
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "AiTaskApi.dll"]
