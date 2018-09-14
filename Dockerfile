FROM  microsoft/dotnet:2.1-aspnetcore-runtime

COPY ./Hangfire.Job/bin/Debug/netcoreapp2.1 .

RUN chmod 777 Hangfire.Job.dll

ENTRYPOINT ["dotnet", "Hangfire.Job.dll"]