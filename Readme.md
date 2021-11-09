# async_http_messages

This is a simple application demonstrating the usage of Dynatrace SDK to correlate two different async transactions.

## Lab details

PixAgendamento simulates the begining of isntant wire transfer when a PUT is received.
Pix.API is a different process that will receive GET continuously until the whole process clears.

In normal circunstances these two transactions will be on different PurePaths, using the SDK will allow two things:

- Both transactions to be placed on the same PurePath
- Add the whole timing through a Custom Request Attribute

## Requirements

- git
- docker
- docker-compose
- .netCore
- JDK

## Starting the lab



1. Go into each of the folders and compile the applications
  1. For dotnet build the solutions
  2. For the Java part, instal using Maven wraper present on the repository: ```mvn clean package -DskipTests=true```
2. Bring requirements up. This will bring up Mysql and ActiveMQ
```
docker-compose up -d
```
3. Start PixAgendament, on the folder with the binaries execute:
```
.\PixAgendamento.exe
```
4. Start Java part. On the project root run:
```
java -jar target/pix_processor-0.0.1-SNAPSHOT.jar
```
5. Start PixAPI, on the folder with the binaries execute:
```
.\Pix.API.exe --urls="http://localhost:5005"
```
6. Send requests to the APIs. This will send request locally to the 
```
./curl.sh
```

