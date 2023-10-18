#!/usr/bin/env bash

top="$(cd "$(dirname "$BASH_SOURCE[0]")" >/dev/null 2>&1 && pwd)"

if [ $# = 1 -o $# = 2 ]; then
    env=$1
    ns=${2:-acmeco}
else
    echo "usage: configure-dapr.sh env [namespace]"
    exit 1
fi

case $env in
    production|staging|*review|*tilt) ;;
    *)
        echo "WARNING: non-standard environment: $env"
        echo "Do you want to continue? (y/[n])"
        read a
        case $a in
            y*) ;;
            *) exit 0 ;;
        esac
        ;;
esac

$top/init-namespace.sh $ns

if [ -d $top/../dapr ]; then
    for i in $top/../dapr/*.yaml; do
        sed "s/<x>-/$env-/g" $i | kubectl apply -n $ns -f -
    done
fi