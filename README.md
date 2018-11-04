# Netpips.Server
[![Build Status](https://travis-ci.org/PierreRoudaut/Netpips.Server.svg?branch=master)](https://travis-ci.org/PierreRoudaut/Netpips.Server)

Manage downloads, media files and libraries for Plex Media Server through a REST API

## About
Netpips is a companion server app for Plex Media Server. It's main focus is to manage and automate the download of TV Shows, Movies, downloads on your server. Once download are completed, the target files will be automatically "smartly" renamed and moved to their according media folder and subtitles will be fetched

For example, a file title _The.Big.Bang.Theory.S11E23.HDTV.x264-SVA[eztv].mkv_
will be renamed and moved to: 

__TV Shows / Suits / Season 07 / Suits - S07E16 - GoodBye.mkv__


## Features

## Getting Started

## Prerequisites

The server should have installed the following CLI dependecies
+ __filebot__ (to fetch subtitles and rename episodes)
+ __aria2c__ (to generate torrent files based magnet links)
+ __transmission-remote__ (to interface P2P torrent download)
+ __mediainfo__ (to)


The server on which Netpips.Server is installed should have a `netpips` user with the following directories

```
downloads/
logs/
medialibrary/
    Movies/
    TV Shows/
    Podcasts/
    Music/
.torrent_done.sh
```

_.torrent_done.sh_:
```bash
#!/bin/bash
if [ $# -eq 1 ]; then
    TR_TORRENT_HASH=$1
fi
curl "http://localhost:5000/api/torrentDone/$TR_TORRENT_HASH"
```

## Installing

A step by step series of examples that tell you how to get a development env running
