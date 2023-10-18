# Tilted Dapr Demo

Demo client-server app to showcase the use of Dapr with Tilt, and optionally vcluster.

## Prerequisites

* dapr cli
* tilt cli
* node
* .NET 7
* vcluster cli (optional)

```
nmp install
dotnet tool restore
```

If you are using vcluster, edit `./vcluster/templates/secrets.yaml` and add your own credentials.

Environment:
```
export LOG_LEVEL=3
export CLIENT_PORT=8080
export SERVER_PORT=8085
export SERVER_PROXY_PORT=8095
export TILT_PORT=8050
```

The client listens on http://localhost:8080/.

## Run locally

```
export SERVER_PROXY_PORT=8085
dapr run -- dotnet run
```

## Run with Tilt

```
tilt up
```

## Run Tilt with vcluster

```
cd vcluster; ./create_vcluster.sh $USER
vcluster connect $USER
tilt up
cd vcluster; ./delete_vcluster.sh $USER
```
