#!/usr/bin/env bash
#

if [ $# != 1 ]; then
    echo "usage: $0 cluster"
    exit 1
fi

cluster=$1

configure_cluster_files () {
    mkdir -p _$cluster

    cd templates
    cp shell.nix ../_$cluster

    for i in *.yaml; do
        sed "s/<x>/$cluster/g" $i > ../_$cluster/$i
    done
}

configure_cluster_files
cd ../_$cluster

kubectl apply -f cnpg.yaml
kubectl apply -f ingress.yaml

vcluster create --chart-version=0.15.5 --connect=false -f values.yaml $cluster

vcluster connect $cluster -- kubectl apply -f cluster-auth-rbac.yaml
vcluster connect $cluster -- kubectl apply -f secrets.yaml

vcluster connect $cluster -- dapr init -k
vcluster connect $cluster -- kubectl apply -f tracing.yaml

token=$(vcluster connect $cluster -- ../get-admin-token.sh)
sed -i "s/token: .*/token: $token/" kubeconfig.yaml

[ -d ~/.kube/config.d ] && cp kubeconfig.yaml ~/.kube/config.d/vcluster-$cluster.yaml

