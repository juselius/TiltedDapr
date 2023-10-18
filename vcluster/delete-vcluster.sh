#!/usr/bin/env bash

if [ $# != 1 ]; then
    echo "usage: $0 cluster"
    exit 1
fi

cluster=$1

if [ -d _$cluster ]; then
    kubectl delete -f _$cluster/cnpg.yaml
    kubectl delete -f _$cluster/ingress.yaml
    rm -rf _$cluster
fi

vcluster delete $cluster
