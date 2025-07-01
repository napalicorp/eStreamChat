FROM mcr.microsoft.com/dotnet/aspnet:9.0-preview AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:9.0-preview AS build
WORKDIR /src
COPY ["src/eStreamChat.Api/eStreamChat.Api.csproj", "src/eStreamChat.Api/"]
COPY ["src/eStreamChat.Common/eStreamChat.Common.csproj", "src/eStreamChat.Common/"]
COPY ["src/eStreamChat.Interfaces/eStreamChat.Interfaces.csproj", "src/eStreamChat.Interfaces/"]
RUN dotnet restore "src/eStreamChat.Api/eStreamChat.Api.csproj"
COPY . .
WORKDIR "/src/src/eStreamChat.Api"
RUN dotnet build "eStreamChat.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "eStreamChat.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "eStreamChat.Api.dll"]