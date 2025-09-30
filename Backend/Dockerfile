FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443
# Add debug port
EXPOSE 5005

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["TrapCam.Backend.csproj", "./"]
RUN dotnet restore "TrapCam.Backend.csproj"
COPY . .
RUN dotnet build "TrapCam.Backend.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TrapCam.Backend.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# Explicitly copy the Data directory with animals.txt
COPY Data/ /app/Data/

ENTRYPOINT ["dotnet", "TrapCam.Backend.dll"]
