FROM microsoft/dotnet:2.1-aspnetcore-runtime AS base
WORKDIR /app
EXPOSE 88

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY ["Hangfire.Job/Hangfire.Job.csproj", "Hangfire.Job/"]
RUN dotnet restore "Hangfire.Job/Hangfire.Job.csproj"
COPY . .
WORKDIR "/src/Hangfire.Job"
RUN dotnet build "Hangfire.Job.csproj" -c Release -o /app

FROM build AS publish
RUN dotnet publish "Hangfire.Job.csproj" -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Hangfire.Job.dll"]