#!/bin/bash

TIME=$(date +"%H.%M") 

CURRENT_BRANCH=$(git rev-parse --abbrev-ref HEAD)

# Compression mode: gzip (default), pigz, zstd, or none
COMPRESS=${COMPRESS:-gzip}

docker buildx build --platform linux/arm64 -t tutorbot:$CURRENT_BRANCH -t tutorbot:$CURRENT_BRANCH.$TIME  -f "./Dockerfile" ./ || exit 1

case "$COMPRESS" in
    gzip)
        docker save tutorbot:$CURRENT_BRANCH tutorbot:$CURRENT_BRANCH.$TIME \
            | gzip -c \
            | dd bs=4M status=progress \
            | ssh gtr@192.168.0.221 "gunzip -c | docker load"  || exit 1
        ;;
    pigz)
        docker save tutorbot:$CURRENT_BRANCH tutorbot:$CURRENT_BRANCH.$TIME \
            | pigz -c \
            | dd bs=4M status=progress \
            | ssh gtr@192.168.0.221 "unpigz -c | docker load"  || exit 1
        ;;
    zstd)
        docker save tutorbot:$CURRENT_BRANCH tutorbot:$CURRENT_BRANCH.$TIME \
            | zstd -c \
            | dd bs=4M status=progress \
            | ssh gtr@192.168.0.221 "zstd -d -c | docker load"  || exit 1
        ;;
    none)
        docker save tutorbot:$CURRENT_BRANCH tutorbot:$CURRENT_BRANCH.$TIME \
            | dd bs=4M status=progress \
            | ssh gtr@192.168.0.221 "docker load"  || exit 1
        ;;
    *)
        echo "Unknown COMPRESS value: $COMPRESS (use: gzip, pigz, zstd, none)"
        exit 1
        ;;
esac
 