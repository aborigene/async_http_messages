#!/bin/bash
test_time=$1
elapsed_time=0
while [ 0 ]; do
	# curl 127.0.0.1:3000
	# curl  "127.0.0.1:8082"
	RAND=$RANDOM
	curl -X PUT "http://localhost:5000/PixAgendamento/transferencia/$RAND" -H  "accept: text/plain" -v -d ""
	GET_RESULT="NO DATA"
	while [ "$GET_RESULT" == "NO DATA"  ]; do
		echo "Is PIX operation finished???"
		GET_RESULT=`curl "http://localhost:5005/OrdemPagamento/ordem_pagamento/$RAND"`
		echo "Result: $GET_RESULT"
	done

	# sleep 60
	elapsed_time=$(($elapsed_time+1))
done
