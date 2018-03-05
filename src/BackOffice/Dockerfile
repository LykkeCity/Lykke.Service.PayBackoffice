FROM microsoft/aspnetcore:2.0
ENTRYPOINT ["dotnet", "BackOffice.dll"]
ARG source=.
WORKDIR /app
COPY $source .
COPY /runtimes/linux-x64/native/libuv.so /app