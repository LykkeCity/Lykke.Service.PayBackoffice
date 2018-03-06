FROM microsoft/aspnetcore:2.0
ENTRYPOINT ["dotnet", "Lykke.Service.PayBackoffice.dll"]
ARG source=.
WORKDIR /app
COPY $source .
COPY /runtimes/linux-x64/native/libuv.so /app