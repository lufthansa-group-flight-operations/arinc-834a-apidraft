# Dummy STAP Server

> **This material is derived from ARINC Standards prepared by the Airlines Electronic Engineering Committee (AEEC).  
> Lufthansa Group is a long-standing member of the AEEC.**

## About this image

This image provides a simplified ARINC 834 STAP Server. Purpose of the server is command turn-around time measurement and protocol integration capability with ARINC 834A.
Using the tag "latest" x64 and arm architecture is supported.

## How to Start

To Start the STAP Server simply type:

```shell
docker run \
--name stapserver \
-d -i \
-p 0.0.0.0:5658:5658 \
lhgflightops/arinc-834-stapserver:latest
```

## Functions

The server is limited in function. As its focus was measurement of command turn-around times, the server accepts following commands, validates them and return the respective response.

### Status

After a valid "status" request a status answer will be sent containing simulated A429/A717 channels to be used for transmission or subscription.

### Transmit

Server accepts transmit commands and returns after parsing/validation a response according A834 specification. Valid  transmit channels are sent in the response of the "status" request.

### Subscribe

Server accepts subscribe commands and returns after parsing/validation a response according A834 specification. Valid receive channels are sent in the response of the "status" request.

### Data

Currently, no data is sent.
