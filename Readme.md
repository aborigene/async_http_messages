# legacy_messaging_system_dynatrace_sdk

The aim of this lab is to demonstrate how to use the Dynatrace SDK to overcome difficult situations on not supported technology. This lab consists of 4 microservices that communicate with each other throug a legacy messaging system or through a technology no supported.

## Lab details

Below is the exact flow for this lab:

 -> bacen_receiver (.NetCore) -> ActiveMQ (not supported on .NetCore) -> bacen_processor (.NetCore) -> ActiveMQ (not supported on .NetCore) -> pix_receiver (Java) -> Legacy messaging system (MySQL bases messagin system) -> pix_sensibiliza (Java) -> File on disk

With the Dynatrace SDK it is possible to create connection on these not supported messaging systems. Here the challenge is to add the Dynatrace correlation header to the messages. So we instrumente the code with our SDK. We them collect ou header from the Dynatrace SDK and we add that to the message property. The process is the same for .NETCore and Java.

Attention here is that we must be able to add the header without breaking the application. This is easy for ActiveMQ for the legacy DB based messaging system this may be a problem. On this lab correlation works, but on actual customers there may be no space to propagate this header without breaking the application.

## Requirements

- git
- docker
- docker-compose
- .netCore
- node modules:
    - opentelemetry/api 1.0.3,
    - opentelemetry/exporter-jaeger: 0.24.0,
    - opentelemetry/sdk-node: 0.24.0,
    - opentelemetry/sdk-trace-base: 0.24.1-alpha.4,
    - express: 4.17.1,
    - mongoose: 6.0.4,
    - node-rdkafka: 2.11.0,
    - uuid: 8.3.2
    - just run ```npm install``` on the base folder for this example and all dependencies will be installed.
- Jaeger (support for jaegertracing/all-in-one:1.25 and older because of Go version)
- Kafka broker running locally
- MongoDB
- Just ```run docker-compose up -d``` and this will bring up Jaeger, Kafka and MongoDB on containers.
- Dynatrace

## Starting the lab

Make sure that Docker is running.

1. Install dependencies:
```
npm install
```
2. Bring requirements up:
```
docker-compose up -d
```
3. Bring producer up:
```
node producer.js
```
4. On a separate terminal window bring consumer up:
```
node consumer-flow.js
```
5. Send an HTTP request to node:
```
curl "127.0.0.1:8081?message=Sample%20Message%201234"
```
6. Generate traffic using the traffic generator:
```
./curl.sh
```

## TODO

1. Make the information from the message be added to the MongoDB, right now it always insert into MongoDB the same static data.
2. Add everything, incluidng application code and dependencies to docker so docker-compose can bring everything up with just one command.

