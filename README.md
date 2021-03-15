# Simple Event Store


C# Wrapper for EventStore that allows Event Handlers to be easily registered with NetCore 5


## Event Store (Docker Setup)
docker run --name EventStoreDB -it -d --restart always -p 2113:2113 -p 1113:1113 eventstore/eventstore:latest --insecure --run-projections=All --enable-external-tcp --enable-atom-pub-over-http
