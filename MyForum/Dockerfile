FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /app

COPY ["MyForum/MyForum.csproj", "MyForum/"]
RUN dotnet restore "MyForum/MyForum.csproj"

COPY ./MyForum ./MyForum
WORKDIR /app/MyForum
RUN dotnet publish -c Release -o out

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
EXPOSE 80
COPY --from=build /app/MyForum/out .

ENTRYPOINT ["dotnet", "MyForum.dll"]