FROM microsoft/aspnetcore-build AS builder
WORKDIR /source

# caches restore result by copying csproj file separately
COPY ./Hangfire.Job/Hangfire.Job.csproj .
RUN dotnet restore

# copies the rest of your code
COPY ./Hangfire.Job .
RUN dotnet publish --output /app/ --configuration Release

# Stage 2
FROM microsoft/aspnetcore
WORKDIR /app
COPY --from=builder /app .
EXPOSE 88
ENTRYPOINT ["dotnet", "Hangfire.Job.dll"]