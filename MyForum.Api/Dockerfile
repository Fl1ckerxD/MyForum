FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY ["MyForum.Api/MyForum.Api.csproj", "MyForum.Api/"]
RUN dotnet restore "MyForum.Api/MyForum.Api.csproj"

COPY ./MyForum.Api ./MyForum.Api
WORKDIR /app/MyForum.Api
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
EXPOSE 80
COPY --from=build /app/MyForum.Api/out .

ENTRYPOINT ["dotnet", "MyForum.Api.dll"]