> **This material is derived from ARINC Standards prepared by the Airlines Electronic Engineering Committee (AEEC).  
> Lufthansa Group is a long-standing member of the AEEC.**

# ARINC 834A iOS PoC
Frank Daniel Hundrieser  
Deutsche Lufthansa AG

## Pre Req
* XCode 11
* iOS 13
* A834-A Demo Server

## Installation
Open project with XCode 11 and it should compile & run without any issue.

## Introduction
This represents a PoC of using REST & WebSockets in iOS Apps with only core libraries.
The PoC is capable of connecting to a remote server via REST or WebSocket.
The App provides an implementation of five Use-Cases:

1. REST - Polling Avionc Parameters
2. REST - File Viewer
3. REST - Video Stream
4. WebSocket- Subscription Avionic Parameters
5. WebSocket - A429 raw data as an example of socket upgrade

For every Use-Case there is at least one parent view and a service.

The PoC does not claim to present an optimal solution. For easier understanding of a
single Use-Case a view and service for every Use-Case is provided. Many errors are not handled 
as they should and the PoC contains much duplicate code.

## Use-Cases

### REST - Polling Avionc Parameters
For the polling Use-Case the PollingView visualizes the avionic parameters from the PollingService.
The PollingService collects avionic parameters via REST GET command from a remote server.
A timer in the service triggers the polling and can be stopped at any time. Since the service is running
in global content the polling is running even if the view is switched.

### REST - File Viewer
The file view gets the list of files from a remote server via REST GET command. For every file in the list
the RAW content is pulled from the server. The file view can show the content of the file regarding if it
is a PDF or any other file. For PDFs a PDF View is provided for any other file the bytes get represented as
characters.

### REST - Video Stream
Not yet implemented

### WebSocket- Subscription Avionic Parameters
The subscription simply connects via WebSocket to the remote server. From the server the avionic parameters
are published and collected from the SubscriptionService. The collected avionic parameters get visualized in
the SubscriptionView.

### WebSocket - A429 raw data as an example of socket upgrade
Not yet implemented

## Summary
It is possible to use REST and WebSockets to collect/publish data to a remote server synchronous and
asynchronous. The iOS core libraries provide all necessary tools needed to get the job done. Public 
libraries for WebSockets and REST and WebSockets are available which makes life easier.
