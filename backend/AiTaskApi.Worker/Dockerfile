# BUILD
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["AiTaskApi.Worker/AiTaskApi.Worker.csproj", "AiTaskApi.Worker/"]
COPY ["AiTaskApi/AiTaskApi.csproj", "AiTaskApi/"]
COPY ["AiTaskApi.Shared/AiTaskApi.Shared.csproj", "AiTaskApi.Shared/"]

RUN dotnet restore "AiTaskApi.Worker/AiTaskApi.Worker.csproj"

COPY . .
WORKDIR "/src/AiTaskApi.Worker"

RUN dotnet publish -c Release -o /app/publish

# RUNTIME
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

RUN apt-get update \
    && apt-get install -y --no-install-recommends libgssapi-krb5-2 \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "AiTaskApi.Worker.dll"]


