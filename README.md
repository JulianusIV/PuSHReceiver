[![CodeQL](https://github.com/JulianusIV/PubSubHubBubReciever/actions/workflows/codeql-analysis.yml/badge.svg?branch=master)](https://github.com/JulianusIV/PubSubHubBubReciever/actions/workflows/codeql-analysis.yml)
[![Docker Image CI](https://github.com/JulianusIV/PubSubHubBubReciever/actions/workflows/docker-ci.yml/badge.svg?branch=master)](https://github.com/JulianusIV/PubSubHubBubReciever/actions/workflows/docker-ci.yml)
[![CodeFactor](https://www.codefactor.io/repository/github/julianusiv/pushreceiver/badge)](https://www.codefactor.io/repository/github/julianusiv/pushreceiver)
[![Run MSTest](https://github.com/JulianusIV/PubSubHubBubReciever/actions/workflows/run-mstest.yml/badge.svg)](https://github.com/JulianusIV/PubSubHubBubReciever/actions/workflows/run-mstest.yml)

DUE TO INCONSISTENCIES WITH THE IMPLEMENTATION OF PuSH ON APPSPOT I HAVE DECIDED TO ARCHIVE THIS REPOSITORY. 
IF YOU NEED A WAY TO SEND UPLOAD NOTIFICATIONS TO DISCORD FROM YOUTUBE SPECIFICALLY I RECOMMEND CHECKING OUT THE [SAPPHIRE BOT](https://sapph.xyz/).

# PubSubHubBubReciever

**An ASP.NET Core API to consume PuSH notifications from Google's [PubSubHubBub](https://pubsubhubbub.appspot.com) Protocol, and send them for example to a [Discord webhook](https://discord.com/developers/docs/resources/webhook#execute-webhook).**

## Why?

There are many Solutions to forward YouTube notifications to Webhooks, like [IFTTT](https://ifttt.com/) or [Zapier](https://zapier.com/), however these have to poll the YouTube API, which is either slow (for example IFTTT polls the API once every hour to preserve quota, so if the video is published right after one polling run it can take up to an hour until the webhook is executed), or can cause problems like exceeding quota limits. This application is using a service provided by Google, that only requires communication between the servers once on startup, once every time the lease is renewed (typically 5 days), and whenever an item is added to the feed. That makes it incredibly fast (outside of failures in this reciever the slowest case was somewhere around 30 seconds) and doesn't constantly take up network resources, or eat away on quota.

## How does it work?

On Startup completion the program reads the topics from a mysql database, and subscribes to them at the hub, this triggers an HTTP-GET from the hub to the given callback URL. The callback URL is a base URL, like ``https://my-reciever/FeedSubscriber`` with the TopicID appended to it ``https://my-reciever/FeedSubscriber/<topic_id>``
The API Processes this request and sends back the ``hub.challenge`` parameter, if the request was valid, which completes the subscribe action.
On receiving a new subscribe, a lease refresh is also scheduled using a timer with the lease time given in the ``hub.lease_seconds`` parameter of the previous GET method.
Whenever that timer runs out, the subscription flow is triggered again.

Whenever a new item is added to the feed the hub sends an HTTP-POST, which is again verified, and upon verification gets passed to a discord webhook, to send out the notification on discord.

## How to use

### Setup

Currently, this is in development, so I cannot give any guarantee on stability, and any changeset might break at any time.
If you want to set this up on your own Server anyway, you can either find the latest commit on [Docker Hub](https://hub.docker.com/r/julianusiv/pubsubhubbubreciever/tags), or you can build this repository yourself (I will be setting up releases in the future as well) and run the executable on your server.

If you want to run this with Docker you can either use run:

```sh
docker run --name=PuSHReciever \
  -v &PWD/settings.ini:/app/settings.ini
  -v $PWD/Plugins:/app/Plugins \
  -d julianusiv/pubsubhubbubreciever:latest
```
but remember that you have to run a mysql container aswell

or use a compose file similar to this:

```yml
version: '3.5'
services:
  app:
    image: "julianusiv/pubsubhubbubreciever:latest"
    restart: unless-stopped
    volumes:
      - ./Plugins:/app/Plugins
      - ./settings.ini:/app/settings.ini
    depends_on:
      - db
    ports:
      - '80:80' #omit in prod and route traffic internally instead

  db:
    image: "mysql"
    restart: unless-stopped
    # make envvars the same as used in connectionstring -> root password can differ
    environment:
      MYSQL_ROOT_PASSWORD: "youshallnotpass"
      MYSQL_DATABASE: "example"
      MYSQL_USER: "example"
      MYSQL_PASSWORD: "example"
    volumes:
      - push_data_volume:/var/lib/mysql
    # dont publish this port unless you need to access the database remotely
    #ports:
    #  - '3306:3306'
```

As you might have noticed this also needs a Domain for the Callback URL, and since it sends and receives tokens and secrets to ensure the identity of the hub, and will soon provide a login, it is highly advised to force SSL for this Callback URL. On my own server this is done using [Nginx Proxy Manager](https://nginxproxymanager.com/), forcing SSL and forwarding the requests to Port 80 of the Docker container.

### Usage

On the first run the program will set up the database for you, you can then log in to the dashboard at localhost:80 with the default user and password "Admin" and "Nimda". There you can create, edit and delete leases.

IF YOU ARE RUNNING THIS IN DOCKER DO NOT SIMPLY SHUT DOWN THE CONTAINER
Use the shutdown button on the webpanel instead, this will ensure all subscriptions are properly unsubscribed.
If you are running this in CLI you can just use ctrl + c.
