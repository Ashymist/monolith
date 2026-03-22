FROM dhi.io/dotnet:10-sdk AS build

WORKDIR /app

COPY . ./

RUN dotnet restore backend.csproj

RUN dotnet publish backend.csproj -c Release -o /app/out



FROM dhi.io/aspnetcore:10

WORKDIR /app

COPY --from=build /app/out .

ENTRYPOINT ["dotnet", "backend.dll"]