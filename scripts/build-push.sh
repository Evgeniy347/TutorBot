#!/bin/bash

TIME=$(date +"%H.%M") 

CURRENT_BRANCH=$(git rev-parse --abbrev-ref HEAD)

docker buildx build --platform linux/arm64 -t tutorbot:$CURRENT_BRANCH -t tutorbot:$CURRENT_BRANCH.$TIME  -f "./Dockerfile" ./ || exit 1

docker save tutorbot:$CURRENT_BRANCH tutorbot:$CURRENT_BRANCH.$TIME | bash -c "dd bs=4M status=progress" | ssh gtr@192.168.0.221 "docker load"  || exit 1
 