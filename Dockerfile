FROM  microsoft/dotnet:2.1-sdk

COPY ./Hangfire.Job .

RUN dotnet publish -c Release -o ./runtime

WORKDIR /runtime

RUN chmod 777 Hangfire.Job.dll

ENTRYPOINT ["dotnet", "Hangfire.Job.dll"]