FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["Directory.Build.props", "."]
COPY ["Directory.Packages.props", "."]
COPY ["src/Puyaq.Api/Puyaq.Api.csproj", "src/Puyaq.Api/"]
COPY ["src/Puyaq.Application/Puyaq.Application.csproj", "src/Puyaq.Application/"]
COPY ["src/Puyaq.Domain/Puyaq.Domain.csproj", "src/Puyaq.Domain/"]
COPY ["src/Puyaq.Infrastructure/Puyaq.Infrastructure.csproj", "src/Puyaq.Infrastructure/"]
COPY ["src/Puyaq.CrossCutting/Puyaq.CrossCutting.csproj", "src/Puyaq.CrossCutting/"]

RUN dotnet restore "src/Puyaq.Api/Puyaq.Api.csproj"

COPY . .
WORKDIR "/src/src/Puyaq.Api"
RUN dotnet publish "Puyaq.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Puyaq.Api.dll"]
