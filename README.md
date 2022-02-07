[![CodeQL](https://github.com/JulianusIV/PubSubHubBubReciever/actions/workflows/codeql-analysis.yml/badge.svg?branch=master)](https://github.com/JulianusIV/PubSubHubBubReciever/actions/workflows/codeql-analysis.yml)
[![Docker Image CI](https://github.com/JulianusIV/PubSubHubBubReciever/actions/workflows/docker-ci.yml/badge.svg?branch=master)](https://github.com/JulianusIV/PubSubHubBubReciever/actions/workflows/docker-ci.yml)

# PubSubHubBubReciever

**An ASP.NET Core API to forward PuSH notifications from Google's [PubSubHubBub](https://pubsubhubbub.appspot.com) to a [Discord webhook](https://discord.com/developers/docs/resources/webhook#execute-webhook).**

## Why?

There are many Solutions to forward YouTube notifications to Webhooks, like [IFTTT](https://ifttt.com/) or [Zapier](https://zapier.com/), however these have to poll the YouTube API (at least in Zapiers case) which is not only slow, but can also cause problems like exceeding quota limits. This is using a service provided by Google, that only requires communication between the servers once on startup and once everytime the lease is renewed, and whenever a item is added to the feed. That makes it so this is incredibly fast (current tests have not been slower than around 30 seconds) and doesnt constantly take up network resources, or eat away on quota.

## How does it work?

On Startup completion the program reads the topics from the ``data.json`` file, and subscribes to them at the hub, this triggers a HTTP-GET from to haub to the given callback url. The callback url is a base url, like ``https://my-reciever/FeedSubscriber`` with the TopicID appended to it ``https://my-reciever/FeedSubscriber/<topic_id>``
The API Processes this request and sends back the ``hub.challenge`` parameter, if the request was valid, which completes the subscribe action.
On recieving a new subscribe a lease refresh is also scheduled using a timer with the lease time given in the ``hub.lease_seconds`` parameter of the previous GET method.
Whenever that timer runs through the subscription flow is triggered again.

Whenever a new item is added to the feed the hub sends a HTTP-POST, which is again verified, and upon verification gets passed to a discord webhook, to send out the notification on discord.

## How to use

### Setup

Currently this is in early development, so i cannot give any guarantee on stability, and any changeset might break at any time.
If you want to set this up on your own Server anyways you can either find the latest commit on [Docker Hub](https://hub.docker.com/r/julianusiv/pubsubhubbubreciever/tags), or you can build this repository yourself (I will be setting up releases in the future aswell) and run the executable on your server.

As you might have noticed this also needs a Domain for the Callback URL, and since it sends and recieves tokens and secrets to ensure the identity of the hub aswell as the admin(you) it is highly adviced to force SSL for this Callback URL. On my own server this is done using [Nginx](https://www.nginx.com/), forcing SSL and forwarding the requests to Port 80 of a Docker container.

### Usage

On the first run the program will generate template ``data.json`` and ``leases.json`` files for you to fill in with the basic Data, you can see these templates [here](https://github.com/JulianusIV/PubSubHubBubReciever/blob/master/PubSubHubBubReciever/data.json.template) and [here](https://github.com/JulianusIV/PubSubHubBubReciever/blob/master/PubSubHubBubReciever/leases.json.template).

Fill these in with valid data, and run the program again (or just set this up before the first run).

If you want to add any further topics to subscribe to, you can either do so by editing said files again and restarting the application, or (the preferred way) you use the Manage endpoint and send it HTTP-POST to create a new entry, HTTP-DELETE to delete an existing entry or HTTP-PATCH to modify an existing entry (proper documentation on that endpoint soon™).
That is where you will need the ``AdminToken`` set in the data.json.
These requests can be done manually by using tools like [Postman](https://www.postman.com/), [Thunder Client](https://www.thunderclient.com/) (if you like VSCode), or other REST API clients
