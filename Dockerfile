FROM mcr.microsoft.com/dotnet/sdk:6.0 AS builder
WORKDIR /src
COPY . /src/
RUN dotnet restore ./TTTWeb/TTTWeb.csproj
RUN dotnet build ./TTTWeb/TTTWeb.csproj --no-restore
RUN dotnet publish ./TTTWeb/TTTWeb.csproj --no-restore -o /bin/ -c Release

FROM mcr.microsoft.com/dotnet/aspnet:6.0
WORKDIR /app
COPY --from=builder /bin/ ./
ENTRYPOINT [ "dotnet", "TTTWeb.dll" ]
EXPOSE 80 443