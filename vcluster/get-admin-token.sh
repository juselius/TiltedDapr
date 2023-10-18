#!/usr/bin/env bash

token=$(kubectl get secret -n kube-system | grep cluster-admin-token | cut -d' ' -f1)
kubectl get secret -n kube-system $token -o yaml | \
    grep ' token:' | cut -d' ' -f4 | base64 -d
