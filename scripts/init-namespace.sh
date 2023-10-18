#!/usr/bin/env bash

ns=${1:-default}

init_namespace() {
    kubectl create ns $ns
    kubectl annotate ns/$ns linkerd.io/inject=enabled
}

kubectl get ns $ns > /dev/null 2>&1 || init_namespace