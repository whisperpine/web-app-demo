#!/bin/bash

docker build -t web-app-demo . \
    --platform linux/amd64,linux/arm64 \
    --pull
