package com.dynatrace.sdk_lab.pix_processor.jms;

import com.dynatrace.sdk_lab.pix_processor.model.PixOperation;
import com.dynatrace.sdk_lab.pix_processor.model.MyQueueMessage;
import lombok.extern.slf4j.Slf4j;
import org.springframework.beans.factory.annotation.Autowired;
//import org.springframework.beans.factory.annotation.Value;
import org.springframework.jms.annotation.JmsListener;
//import org.springframework.jms.core.JmsTemplate;
import org.springframework.stereotype.Component;

import com.dynatrace.oneagent.sdk.OneAgentSDKFactory;
import com.dynatrace.oneagent.sdk.api.IncomingMessageProcessTracer;
import com.dynatrace.oneagent.sdk.api.IncomingMessageReceiveTracer;
import com.dynatrace.oneagent.sdk.api.OneAgentSDK;
import com.dynatrace.oneagent.sdk.api.OutgoingMessageTracer;
import com.dynatrace.oneagent.sdk.api.enums.ChannelType;
import com.dynatrace.oneagent.sdk.api.enums.MessageDestinationType;
import com.dynatrace.oneagent.sdk.api.infos.MessagingSystemInfo;

import javax.jms.Message;
import javax.jms.MessageListener;
import javax.jms.ObjectMessage;
import com.google.gson.*;
import org.apache.activemq.command.ActiveMQTextMessage;
import java.util.Random;
import com.dynatrace.sdk_lab.pix_processor.model.PixOperationRepository;

@Component
@Slf4j
public class JmsConsumer implements MessageListener {
    @Autowired
    PixOperationRepository pixOperationRepository;

    OneAgentSDK oneAgentSdk = OneAgentSDKFactory.createInstance();
    
    @Override
    @JmsListener(destination = "bacen.transfer")
    public void onMessage(Message message) {
        Random rand = new Random();
        int upperbound = 10000;
        int int_random = rand.nextInt(upperbound); 
        try{
            
            Gson gson = new Gson();
            System.out.println(message.toString());
            
            String message_body = ((ActiveMQTextMessage) message).getText();//getBody(String.class);
            PixOperation operation = gson.fromJson(message_body, PixOperation.class);
            //message.
            //ObjectMessage objectMessage = (ObjectMessage)message;
            //Operation operation = (Operation)objectMessage.getObject();
            //do additional processing
            String queue_name;
            int pix_ammount = operation.getValue();
            String dt_header =  "";
            
            System.out.println("Sleeping for " + int_random + " millis.");
            Thread.sleep(int_random);
            PixOperation savedOperation = pixOperationRepository.save(operation);
            
            System.out.println("SENDING: PIX Ammount: "+ operation.getValue()+ ", EndToEndID: "+ operation.getEndToEndID()+ ".");
        } catch(Exception e) {
            System.out.println("Received Exception while saving to DB: "+ e);
        }
    }
}