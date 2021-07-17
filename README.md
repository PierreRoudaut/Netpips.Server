# Netpips.Server

Manage downloads, media files and libraries for Plex Media Server through a REST API


## About
Netpips is a companion server app for Plex Media Server. It's main goal is to enable users to collaboratively download content (TV Shows, Movies, etc) to the Plex library folders while keeping them organized.

## Installing

Netpips is Web API project running on ASP.NET Core 2.1.
1. Clone the repo
2. Execute the following commands
```bash
cd Netpips
dotnet restore
dotnet build
dotnet run
```
3. Head to http://localhost:5000/api/swagger for the full API documentation

## Download Workflow

Netpips supports two types of download methods:
- Direct Download Link (DDL). e.g uptobox.com, rapidgator.com
- Peer to peer (P2P). e.g magnet links, torrent urls

However, these two types of download run the same workflow

1. Once a download runs to completion, a job will be triggered to extract (if .rar or .zip), rename and move "smartly" all of the downloaded files to their assumed media folder, in order to facilitate [Plex's Naming Convention](https://support.plex.tv/articles/categories/media-preparation/). e.g _The.Big.Bang.Theory.S11E23.HDTV.x264-SVA[eztv].mkv_
would be renamed and moved to:  _TV Shows / Suits / Season 07 / Suits - S07E16 - GoodBye.mkv_

2. After renaming, if the downloaded file is a TV Show or a Movie, matching english subtitles will be fetched

3. Once all files of a given download have been renamed and moved, a notification email containing the summary of the download will be sent to the user who started the download

## Search for content

## TV Show subscription

## Administer Users

## Media library management

## Features

## Prerequisites

The server should have installed the following CLI dependecies
+ __filebot__ (to fetch subtitles and rename episodes)
+ __aria2c__ (to generate torrent files based magnet links)
+ __transmission-remote__ (to interface P2P torrent download)
+ __mediainfo__ (to compute video file duration, should _filebot_ fail to do so)


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
 
