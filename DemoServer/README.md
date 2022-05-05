# ARINC 834A DemoServer
1. [1. Communication examples](#1-communication-examples)
   1. [1.1. Avionics-Parameters -ADIF](#11-avionics-parameters--adif)
      1. [1.1.1. Rest Interface](#111-rest-interface)
         1. [1.1.1.1. Get all parameter names](#1111-get-all-parameter-names)
         2. [1.1.1.2. Get Single Parameter](#1112-get-single-parameter)
         3. [1.1.1.3. Get Single or multiple parameters](#1113-get-single-or-multiple-parameters)
      2. [1.1.2. WebSocket Interface](#112-websocket-interface)
         1. [1.1.2.1. Timeout](#1121-timeout)
         2. [Responses](#responses)
         3. [1.1.2.2. Continuous Subscription](#1122-continuous-subscription)
         4. [1.1.2.3. On Change Subscription](#1123-on-change-subscription)
         5. [1.1.2.4. On Update Subscription](#1124-on-update-subscription)
2. [2. Installation](#2-installation)
   1. [2.1. Build Docker image and start server](#21-build-docker-image-and-start-server)
   2. [2.2. Start server stand-alone](#22-start-server-stand-alone)
   3. [2.3. To avoid issues with TLS security on SPDY disable SPDY](#23-to-avoid-issues-with-tls-security-on-spdy-disable-spdy)
   4. [2.4. Create new test certificate](#24-create-new-test-certificate)
   5. [2.5. Loading Certificate](#25-loading-certificate)
      1. [2.5.1. From File](#251-from-file)
      2. [2.5.2. From CertificateStore](#252-from-certificatestore)
3. [3. For testing with Firefox configuration changes may be necessary to allow self-signed certificates](#3-for-testing-with-firefox-configuration-changes-may-be-necessary-to-allow-self-signed-certificates)

> **This material is derived from ARINC Standards prepared by the Airlines Electronic Engineering Committee (AEEC).  
> Lufthansa Group is a long-standing member of the AEEC.**

This project contains a simple demo server for performance measurements.

Server written in C# with ASP.NET Core using .Net6

>Note: The Demo Server generates some example Avionics parameters at 10Hz, with actual timestamps, but only date_utc and time_utc have changinge values, reflecting the current date and time.

---

## 1. Communication examples
Currently only ADIF service parameters are implemented.
### 1.1. Avionics-Parameters -ADIF
#### 1.1.1. Rest Interface
##### 1.1.1.1. Get all parameter names
Will return all parmaeter names but without values
>GET: /a834a/adif/v1/parameters

CURL Example:
~~~shell
curl -X 'GET' \
  'https://localhost:5001/a834a/adif/v1/parameters' \
  -H 'accept: */*'
~~~
Response:
~~~json
{
  "parameters": [
    {
      "name": "airline_id",
      "settable": false
    },
    {
      "name": "ac_icao24",
      "settable": false
    },
    {...}
  ]
}
~~~

##### 1.1.1.2. Get Single Parameter
>GET: /a834a/adif/v1/parameters/{name}

Example to get utc time:
>GET: /a834a/adif/v1/parameters/time_utc

Reponse:

~~~json
{
  "name": "time_utc",
  "value": "11:55:00",
  "timestamp": 15684651325
}
~~~

##### 1.1.1.3. Get Single or multiple parameters
Parameters an be provided after the query ?params= as a comma sperated list, whereas comma is escaped as %2C:
>GET: /a834a/adif/v1/parameters?params={name}%2C{name}

< OR >

Provide each parameter as single query request:
>GET: /a834a/adif/v1/parameters?params=name&params=name

This generates more overheat, but should be supported, as stadard libraries may produce them by default.

Example to get date&time:
>GET: /a834a/adif/v1/parameters?params=date_utc%2Ctime_utc

< OR >

>GET: /a834a/adif/v1/parameters?params=date_utc&params=time_utc

Response example:
~~~json
{
  "parameters": [
    {
      "name": "time_utc",
      "value": "11:55:00",
      "timestamp": 15684651325
    },
    {
      "name": "date_utc",
      "value": "25.09.2021",
      "timestamp": 15684651325
    }
  ]
}
~~~

#### 1.1.2. WebSocket Interface
Subscription for WebSockets are made on the url:
>/a834a/adif/v1/parameters/subscribe

> Known Limitation: Currently all parameters cannot be request with wildcard asterisk '*'

##### 1.1.2.1. Timeout
If no subscription is made within 10 seconds, the server will send an error message and close the WebSocket connection.

~~~json
{
   "returnCode": "Error",
   "reason": "TimeOut"
}	
~~~

##### Responses
If a subscription is fully accepted, the Server returns an ok-message directly after the subscription request:
~~~json
{
   "returnCode":"Ok"
}
~~~

If some parameters are unknown, the unknowm parameters will be attahed to the end:

~~~json
{
  "returnCode":"Ok",
  "unknownParameters":[
    "fuel_price",
    "coffee_temp"
    ]
}
~~~

Otherwise an error message is send, stating an error code:
i.e.

~~~json
{
   "returnCode": "Error",
   "reason": "BadFormat"
}	
~~~

##### 1.1.2.2. Continuous Subscription
Continous subscription will deliver data on client requested interval, whereas the interval is given in ms.

Request Message:
~~~json
{
   "method": "subscribeParameters",
   "arguments": {
      "type": "Continuous",
      "interval": 2000,
      "parameters": [
         "date_utc",
         "time_utc",
      ]
   }
}	
~~~
##### 1.1.2.3. On Change Subscription
On change subscription will deliver data when a value of a parameter has changed. Intially after the ok message, one message, with all current values is delivered.

Request Message:
~~~json
{
   "method": "subscribeParameters",
   "arguments": {
      "type": "On Change",      
      "parameters": [
         "date_utc",
         "time_utc",
      ]
   }
}	
~~~

##### 1.1.2.4. On Update Subscription
On Update Subscription will deliver data as soon as values got updated, even if value has not changed (will be markable in updated timestamp).

Request Message:
~~~json
{
   "method": "subscribeParameters",
   "arguments": {
      "type": "On Update",      
      "parameters": [
         "date_utc",
         "time_utc",
      ]
   }
}	
~~~

---




## 2. Installation

curl commands contain -k for accepting self-signed certificate!

### 2.1. Build Docker image and start server

```shell
cd DemoServer
docker build -f DemoServer/DockerfileRpi -t lhgflightops/arinc-834-demoserver .
docker run --rm -p 8080:8080 -p 8443:8443 lhgflightops/arinc-834-demoserver:latest
```

### 2.2. Start server stand-alone

```shell
dotnet run --urls "https://+:8443;http://+:8080"
```

### 2.3. To avoid issues with TLS security on SPDY disable SPDY

This option is only necessary when working with a self-signed certificate.

~~~json
{
  "Kestrel": {
    "EndpointDefaults": {
      "Protocols": "Http1"
    }
  }
}
~~~

### 2.4. Create new test certificate

```shell
dotnet dev-certs https -ep demo_cert.pxf -p "arinc834democert"
```

### 2.5. Loading Certificate

#### 2.5.1. From File

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

#### 2.5.2. From CertificateStore

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



## 3. For testing with Firefox configuration changes may be necessary to allow self-signed certificates

Before changes configuration test if the connection is refused.

```config
network.websocket.allowInsecureFromHTTPS
```