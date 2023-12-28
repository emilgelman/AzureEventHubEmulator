FROM --platform=amd64 alpine/openssl:latest AS server-cert
    WORKDIR /home/testca
    COPY ./docker/testca .
    COPY ./docker/openssl.cnf .
    RUN mkdir certs && \
        echo 01 > /home/testca/serial && \
	    touch /home/testca/index.txt && \
        openssl genrsa -out key.pem 2048 && \
        openssl req -new -key key.pem -out req.pem -outform PEM -nodes \
          -subj /CN=ehemulator.servicebus.windows.net/O=server/ \
          -config openssl.cnf \
          -addext "subjectAltName = DNS:localhost, DNS:emulator" && \
        openssl ca -config openssl.cnf -in req.pem -out cert.pem -notext -batch -extensions server_ca_extensions && \
        openssl pkcs12 -export -out cert.pfx -inkey key.pem -in cert.pem -passout pass:password
FROM --platform=amd64 mcr.microsoft.com/dotnet/sdk:7.0 AS build
    WORKDIR /app
    COPY . ./
    WORKDIR /app/AzureEventHubEmulator.Host
    RUN dotnet publish -c Release -o /app/out

FROM mcr.microsoft.com/dotnet/aspnet:7.0
    WORKDIR /app
    COPY --from=build /app/out .
    COPY --from=server-cert /home/testca/cert.pfx .
    COPY --from=server-cert /home/testca/cacert.cer .
    ENTRYPOINT ["dotnet", "AzureEventHubEmulator.Host.dll"]