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
