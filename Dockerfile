FROM  microsoft/dotnet:2.1-aspnetcore-runtime

COPY ./Hangfire.Job/Hangfire.Job .

RUN dotnet publish -c Release -o .

RUN chmod 777 Hangfire.Job.dll

ENTRYPOINT ["dotnet", "Hangfire.Job.dll"]