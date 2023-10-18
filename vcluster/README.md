# Oceanbox k8s vcluster setup

The script `./create-vcluster.sh` provisions a personal vcluster on a Kubernetes cluster, for usage
with Tilt. It also automatically provisions a local `Dapr` installation on the cluster, and sets up a
CNPG psql database cluster on the host system, and tunnels it to the vcluster for Archmeister. In
addition, it sets up an ingress and a kubeconfig.yaml for convenient access, if `vcluster connect` isn't
available.

Before use the template files need to be adapted appropriately (add secrets, etc.).