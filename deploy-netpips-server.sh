#!/bin/bash

set -ex

script_name=`basename "$0"`

if [[ "$USER" != 'netpips' ]]; then
    echo "current user ($USER) is not 'netpips'"
    exit 1
fi

if [[ "$#" -ne 1 ]]; then
    echo "./$script_name [VERSION]"
    exit 1
fi

VERSION=$1

sudo service netpips-server stop 2> /dev/null || true
sudo rm -rf /var/netpips/server
sudo mkdir -p /var/netpips/server

git checkout tags/netpips-server-$VERSION
dotnet restore
dotnet build
dotnet ef database update
dotnet publish -c 'Release' -o '/var/netpips/server'
sudo service netpips-server start
sudo service netpips-server status
