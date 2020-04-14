# ARINC 834A DemoServer

> **This material is derived from ARINC Standards prepared by the Airlines Electronic Engineering Committee (AEEC).  
> Lufthansa Group is a long-standing member of the AEEC.**

This image provides a sample implementation of a Demo Server for a technical Proof of Concept (PoC) for the ARINC 834A specification. Purpose of this software is to verify proper usage of protocols specified in A834A.

> **NOTE:** Software should not be used for production.

The container can be used on either the Raspberry Pi (tested with the PI4) and any x86/64 platform supporting docker.

## How to start

To Start the draft sample server simply type:

```shell
docker run \
--name a834a_ds \
-d -i \
-p 0.0.0.0:8080:8080 \
-p 0.0.0.0:8443:8443 \
lhgflightops/arinc-834-demoserver:latest
```

> **NOTE:** To connect additionally a STAP Server see below.

Required to run the container detached and continuously:

```shell
-d -i
```

Map the port 8080 of the host machine (first entry) to the port 8080 (second entry) to the container, which is used for http. 0.0.0.0 allows the container to be accessed from the hosts network.

```shell
- p 0.0.0.0:8080:8080
```

>**NOTE:**

Map the port 8443 of the host machine (first entry) to the port 8443 (second entry) to the container, which is used for https. 0.0.0.0 allows the container to be accessed from the hosts network.

```shell
- p 0.0.0.0:8443:8443
```

Name of the docker image. The tag "latest" support amd64 and arm architecture on host system. The pulling client will automatically receive its appropriate image.

```shell
lhgflightops/arinc-834-demoserver:latest
```

If an explicit version is required, the latest amd64 can be retrieved with

```shell
docker pull lhgflightops/arinc-834-demoserver:x64
```

and the arm version used on rapsberry pi can be retrieved via:

```shell
docker pull lhgflightops/arinc-834-demoserver:rpi
```

### How to connect to STAP Server

As described below, the A834A DemoServer can be used to access a A834 STAP Server using WebSockets. Currently the A834A Demo Server connects to a STAP Server that is accessible on port 5658 on a host machine with hostname "stapserver".

For Demonstration also a STAP Server in another container can be used i.e. from
<https://hub.docker.com/r/lhgflightops/arinc-834-stap-demoserver>

In order to use this one, start the STAP Server:

```shell
docker run --name stapserver -di -p 5668:5658 lhgflightops/arinc-834-stap-demoserver
```

Create a docker network for communication between A834A Demo Server and A834 STAP Server:

```shell
docker network create aidnet
```

Connect the A834A DemoServer with the network:

```shell
 docker network connect aidnet a834a_ds
```

Connect the A834 STAP Server with the network:

```shell
 docker network connect aidnet stapserver
```

## API Functions

Below you wil find some examples of supported API functions.
For a current list of supported functions refer to github: <https://github.com/tbernwald/Arinc834AApiDraft/tree/2019FE-A834A-Comitee-FD/DemoServer>

### Communication examples

#### Swagger UI

An UI-Tool for exploring the regular REST interfaces can be found at:

```shell
https://localhost:8443/swagger/index.html
```

#### Read available parameters and format output

```shell
 curl -k -X OPTIONS https://localhost:8443/api/v1/parameters | python -m json.tool
```

#### Read all parameters as json

```shell
 curl -k -X GET https://localhost:8443/api/v1/parameters
```

#### Read a subset of parameters as xml

```shell
 curl -k -X GET -H "Accept: application/xml" https://localhost:8443/api/v1/parameters?keys=dkey,hkey
```

#### Read all parameters as text

```shell
 curl -k -X GET -H "Accept: text/avionic" https://localhost:8443/api/v1/parameters
```

#### Read parameters via WebSockets

Receiving parameters via WebSocket the same URIs can be used as above:

```shell
 wss://localhost:8443/api/v1/parameters?keys=dkey,hkey
```

```shell
 wss://localhost:8443/api/v1/parameters
```

After connection is established all last known values of subscribed parameters are initially send to the client. Then the parameters are updated as they are received/created at the server.

### For testing with Firefox configuration changes may be necessary to allow self-signed certificates

Before changes configuration test if the connection is refused.

```config
 network.websocket.allowInsecureFromHTTPS
```

### FileStorage

#### Getting file list from storage

Getting list of files in storage as JSON/XML with size and last change date.

```shell
 curl -k 'https://localhost:8443/api/v1/files/'
```

#### Getting files from storage

Getting a file from storage.

```shell
 curl -k 'https://localhost:8443/api/v1/files/%FILENAME%'
```

#### Getting files from storage with ranges

Getting a part of a file from storage. Use case can be file transfer in smaller packets for more stable downloads or the possibility to resume downloads.

```shell
 curl -k -r [0-... range in bytes] 'https://localhost:8443/api/v1/files/%FILENAME%'
```

#### Upload a file to storage

Save file into storage with given filename.

```shell
 curl -k -X POST -T "/path/to/file" "https://localhost:8443/api/v1/files/%FILENAME%"
```

### Other Services through WebSockets

Other TCP based services can be accessed through WebSockets i.e. a STAP Server.

#### STAP Server

To access a STAP server use a WebSocket with address:

```shell
wss://localhost:8443/api/v1/stap
```

This will create a connection to a STAP server. Currently the demo server is configured to create a connection to a STAP server which is accessible via the hostname "stapserver" and TCP port 5658.
