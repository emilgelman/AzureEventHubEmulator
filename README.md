# Azure Event Hub Emulator

An emulator that translates Azure Event Hub SDK commands using AMQP 1.0 specifics. Backend is an in memory implementation that runs locally and stores messages using .NET Channels.
The main use case is to help with local development experience and with integration testing.

See instructions for

* [Running from Docker](#running-from-docker)
* [Configuration](#configuration)

## Running from Docker Hub

```bash
docker run --rm -it -p 0.0.0.0:5671:5671 -e EMULATOR__TOPICS=test,test2 emilgelman/ehemulator:latest

```

## Configuration

Set the `TOPIC_NAMES` environment variable to a comma-separated list of Event Hubs to create. For example:

> TOPIC_NAMES=topic1,topic2

Use the following connection string in your application, replace `test` with your Event Hub name:

> "Endpoint=sb://localhost/;SharedAccessKeyName=all;SharedAccessKey=none;EnableAmqpLinkRedirect=false;EntityPath=test"

## Example

See `IntegrationTests` project for an example of how to use the emulator with the .NET Azure Event Hub SDK.

## Supported Features

Current implementation is considered experimental and is intended for local development use only. Following features are supported:

* Configuring multiple topics (Event Hubs)
* Each topic is represented in memory with a single partition
* Producing and Consuming messages using the native Azure Event Hubs SDK (AMQPS 1.0)

## Not Supported Features

* Multiple partitions
* Offset management (consuming from a specific offset)
* Any Advanced Flow Control