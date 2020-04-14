# ARINC 834A DemoServer

> **This material is derived from ARINC Standards prepared by the Airlines Electronic Engineering Committee (AEEC).  
> Lufthansa Group is a long-standing member of the AEEC.**

This project contains a simple demo server for performance measurements.

Server written in C# with ASP.NET Core.

curl commands contain -k for accepting self-signed certificate!

## Build Docker image and start server

```shell
cd DemoServer
docker build -f DemoServer/DockerfileRpi -t lhgflightops/arinc-834-demoserver .
docker run --rm -p 8080:8080 -p 8443:8443 lhgflightops/arinc-834-demoserver:latest
```

## Start server stand-alone

```shell
dotnet run --urls "https://+:8443;http://+:8080"
```

## To avoid issues with TLS security on SPDY disable SPDY

This option is only necessary when working with a self-signed certificate.

```json
{
  "Kestrel": {
    "EndpointDefaults": {
      "Protocols": "Http1"
    }
  }
}
```

## Create new test certificate

```shell
dotnet dev-certs https -ep demo_cert.pxf -p "arinc834democert"
```

## Loading Certificate

### From File

```json
{  
  "Kestrel": {
    "Certificates": {
      "Default": {
        "Password": "the_password",
        "Path": "path/to/file/cert.pxf"
      }
    }
  }
}
```

### From CertificateStore

```json
{  
  "Kestrel": {
    "Certificates": {
      "Default": {
        "Subject": "<subject; required>",
        "Store": "<cert store; required>",
        "Location": "<location; defaults to CurrentUser>",
        "AllowInvalid": "<true or false; defaults to false>"
      }
    }
  }
}
```

## Communication examples

### Read available parameters and format output

```shell
curl -k -X OPTIONS https://localhost:8443/api/v1/parameters | python -m json.tool
```

### Read all parameters as json

```shell
curl -k -X GET https://localhost:8443/api/v1/parameters
```

### Read a subset of parameters as xml

```shell
curl -k -X GET -H "Accept: application/xml" https://localhost:8443/api/v1/parameters?keys=dkey,hkey
```

### Read all parameters as text

```shell
curl -k -X GET -H "Accept: text/avionic" https://localhost:8443/api/v1/parameters
```

## For testing with Firefox configuration changes may be necessary to allow self-signed certificates

Before changes configuration test if the connection is refused.

```config
network.websocket.allowInsecureFromHTTPS
```

## FileStorage

### Getting file list from storage

Getting list of files in storage as JSON/XML with size and last change date.

```shell
curl -k 'https://localhost:8443/api/v1/files/'
```

### Getting files from storage

Getting a file from storage.

```shell
curl -k 'https://localhost:8443/api/v1/files/%FILENAME%'
```

### Getting files from storage with ranges

Getting a part of a file from storage. Usecase can be filetransfer in smaller packets for more stable downloads or the possiblity to resume downloads.

```shell
curl -k -r [0-... range in bytes] 'https://localhost:8443/api/v1/files/%FILENAME%'
```

### Upload a file to storage

Save file into storage with given filename.

```shell
curl -k -X POST -T "/path/to/file" "https://localhost:8443/api/v1/files/%FILENAME%"
```
