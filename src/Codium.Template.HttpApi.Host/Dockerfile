FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/Codium.Template.HttpApi.Host/Codium.Template.HttpApi.Host.csproj", "src/Codium.Template.HttpApi.Host/"]
COPY ["src/Codium.Template.Application/Codium.Template.Application.csproj", "src/Codium.Template.Application/"]
COPY ["src/Codium.Template.Application.Contracts/Codium.Template.Application.Contracts.csproj", "src/Codium.Template.Application.Contracts/"]
COPY ["src/Codium.Template.Domain.Shared/Codium.Template.Domain.Shared.csproj", "src/Codium.Template.Domain.Shared/"]
COPY ["src/Codium.Template.Domain/Codium.Template.Domain.csproj", "src/Codium.Template.Domain/"]
COPY ["src/Codium.Template.EntityFrameworkCore/Codium.Template.EntityFrameworkCore.csproj", "src/Codium.Template.EntityFrameworkCore/"]
COPY ["src/Codium.Template.HttpApi.Client/Codium.Template.HttpApi.Client.csproj", "src/Codium.Template.HttpApi.Client/"]
COPY ["src/Codium.Template.HttpApi/Codium.Template.HttpApi.csproj", "src/Codium.Template.HttpApi/"]
RUN dotnet restore "src/Codium.Template.HttpApi.Host/Codium.Template.HttpApi.Host.csproj"
COPY . .
WORKDIR "/src/src/Codium.Template.HttpApi.Host"
RUN dotnet build "Codium.Template.HttpApi.Host.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "Codium.Template.HttpApi.Host.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Codium.Template.HttpApi.Host.dll"]
