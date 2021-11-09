using System.Runtime.InteropServices;

using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Apache.NMS;
using System.Threading;
using Apache.NMS.Util;
using Dynatrace.OneAgent.Sdk.Api;
using Dynatrace.OneAgent.Sdk.Api.Enums;
using Dynatrace.OneAgent.Sdk.Api.Infos;
using MySqlConnector;


//using Newtonsoft.Json;

// In SDK-style projects such as this one, several assembly attributes that were historically
// defined in this file are now automatically added during build and populated with
// values defined in project properties. For details of which attributes are included
// and how to customise this process see: https://aka.ms/assembly-info-properties


// Setting ComVisible to false makes the types in this assembly not visible to COM
// components.  If you need to access a type in this assembly from COM, set the ComVisible
// attribute to true on that type.

[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM.

[assembly: Guid("a53b019e-4ade-4148-bcd6-e1a698932b92")]


namespace pix_agendamento.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class PixAgendamentoController : ControllerBase
    {
        private static readonly string[] status = new[]
        {
            "started", "bacen", "other_institution", "finished"
        };

        private readonly ILogger<PixAgendamentoController> _logger;

        public PixAgendamentoController(ILogger<PixAgendamentoController> logger)
        {
            _logger = logger;
        }
        [Route("atualiza/{id}")]
        [HttpPost]
        public PixAgendamento Post(String id)
        {
            IOneAgentSdk OneAgentSdk = OneAgentSdkFactory.CreateInstance();
            IIncomingRemoteCallTracer incomingRemoteCallTracer = OneAgentSdk.TraceIncomingRemoteCall("GetOrdemPagamentoStatus", "Safra.PIX", "http://pix.status/");

            string incomingDynatraceStringTag = id;
            incomingRemoteCallTracer.SetDynatraceStringTag(incomingDynatraceStringTag);
            incomingRemoteCallTracer.SetProtocolName("HTTP");

            incomingRemoteCallTracer.Start();
            DateTime now = DateTime.Now;
            long unixTime = ((DateTimeOffset)now).ToUnixTimeSeconds();

            PixAgendamento newPix = new PixAgendamento
            {
                Date = unixTime,
                Value = 200,
                End2EndID = id
            };

            string destination_queue = "queue://bacen.transfer";
            string connection_url = "activemq:tcp://127.0.0.1:61616";
            Uri connecturi = new Uri(connection_url);

            Console.WriteLine("About to connect to " + connecturi);

            // NOTE: ensure the nmsprovider-activemq.config file exists in the executable folder.
            IConnectionFactory factory = new NMSConnectionFactory(connecturi);

            //IOneAgentSdk oneAgentSdk = OneAgentSdkFactory.CreateInstance();

            
            IMessagingSystemInfo messagingSystemInfo = OneAgentSdk.CreateMessagingSystemInfo(MessageSystemVendor.ACTIVE_MQ, destination_queue, MessageDestinationType.QUEUE, ChannelType.TCP_IP, connection_url);

            IConnection connection;
            bool connected = false;
            while (!connected)
            {

                try
                {
                    Console.WriteLine("Atempting connection...");
                    connection = factory.CreateConnection("guest", "guest");
                    connected = true;
                    using (connection)
                    using (ISession session = connection.CreateSession())
                    {
                        IDestination destination = SessionUtil.GetDestination(session, destination_queue);
                        Console.WriteLine("Using destination: " + destination);

                        // Create a consumer and producer
                        using (IMessageProducer producer = session.CreateProducer(destination))
                        {
                            // Start the connection so that messages will be processed.
                            connection.Start();
                            producer.DeliveryMode = MsgDeliveryMode.Persistent;
                            // Produce a message
                            SendMessage(session, producer, messagingSystemInfo, newPix);
                            connection.Stop();
                        }
                    }
                }
                catch (Apache.NMS.NMSConnectionException exception)
                {
                    Console.WriteLine("ActiveMQ still not up, waiting 1s to retry...");
                    Console.WriteLine(exception.Message);
                    Thread.Sleep(1000);
                }
            }
            incomingRemoteCallTracer.End();
            return newPix;
        }

        [Route("transferencia/{id}")]
        [HttpPut]
        public PixAgendamento Put(String id)
        {
            IOneAgentSdk OneAgentSdk = OneAgentSdkFactory.CreateInstance();

            IOutgoingRemoteCallTracer tracer = OneAgentSdk.TraceOutgoingRemoteCall(
                "GetOrdemPagamentoStatus", "Safra.PIX",
                "http://pix.status/", ChannelType.OTHER, "FimdoPIX:1234");
            tracer.SetProtocolName("HTTP");
            long current_time = DateTimeOffset.Now.ToUnixTimeMilliseconds();
            //IOutgoingWebRequestTracer tracer = OneAgentSdk.TraceOutgoingWebRequest("/OrdemPagamento/ordem_pagamento/", "GET");
            tracer.Start();
            Console.WriteLine("SDK state: " + OneAgentSdk.CurrentState);


            //IOutgoingWebRequestTracer tracer = OneAgentSdk.TraceOutgoingWebRequest("OrdemPagamentoStatus", "GET");
            string tag = tracer.GetDynatraceStringTag();
            Console.WriteLine("Outgoing DT Tag: " + tag);
            //tracer.InjectTracingHeaders();

            using var dbconnection = new MySqlConnection("Server = 127.0.0.1; Port = 3306; Database = pix; Uid = root; Pwd = 123456789");
            //server = localhost;portuser=root;password=123456789;database=pix");
            dbconnection.Open();
            using var command = new MySqlCommand("insert into dynatrace_correlation values (\"" + id + "\", \"" + tag + "\", \"" + current_time + "\");", dbconnection);
            int insertResult = command.ExecuteNonQuery();
            tracer.End();
            
            DateTime now = DateTime.Now;
            long unixTime = ((DateTimeOffset)now).ToUnixTimeSeconds();

            PixAgendamento newPix = new PixAgendamento {
                Date = unixTime,
                Value = 200,
                End2EndID = id
            };

            string destination_queue = "queue://bacen.transfer";
            string connection_url = "activemq:tcp://127.0.0.1:61616";
            Uri connecturi = new Uri(connection_url);

            Console.WriteLine("About to connect to " + connecturi);

            // NOTE: ensure the nmsprovider-activemq.config file exists in the executable folder.
            IConnectionFactory factory = new NMSConnectionFactory(connecturi);

            //IOneAgentSdk oneAgentSdk = OneAgentSdkFactory.CreateInstance();

            
            IMessagingSystemInfo messagingSystemInfo = OneAgentSdk.CreateMessagingSystemInfo(MessageSystemVendor.ACTIVE_MQ, destination_queue, MessageDestinationType.QUEUE, ChannelType.TCP_IP, connection_url);

            IConnection connection;
            bool connected = false;
            while (!connected)
            {    
                try
                {
                    Console.WriteLine("Atempting connection...");
                    connection = factory.CreateConnection("guest", "guest");
                    connected = true;
                    using (connection)
                    using (ISession session = connection.CreateSession())
                    {
                        IDestination destination = SessionUtil.GetDestination(session, destination_queue);
                        Console.WriteLine("Using destination: " + destination);

                        // Create a consumer and producer
                        using (IMessageProducer producer = session.CreateProducer(destination))
                        {
                            // Start the connection so that messages will be processed.
                            connection.Start();
                            producer.DeliveryMode = MsgDeliveryMode.Persistent;
                            // Produce a message
                            SendMessage(session, producer, messagingSystemInfo, newPix);
                            connection.Stop();
                        }
                    }
                }
                catch (Apache.NMS.NMSConnectionException exception)
                {
                    Console.WriteLine("ActiveMQ still not up, waiting 1s to retry...");
                    Console.WriteLine(exception.Message);
                    Thread.Sleep(1000);
                }
            }
            tracer.End();
            return newPix;
        }

        private void SendMessage(ISession session, IMessageProducer producer, IMessagingSystemInfo messagingSystemInfo, PixAgendamento pix)
        {
            IOneAgentSdk OneAgentSdk = OneAgentSdkFactory.CreateInstance();
            IOutgoingMessageTracer outgoingMessageTracer = OneAgentSdk.TraceOutgoingMessage(messagingSystemInfo);

            outgoingMessageTracer.Start();

            ITextMessage request = session.CreateTextMessage(pix.toString());

            string outgoing_tag = outgoingMessageTracer.GetDynatraceStringTag();
            request.Properties[OneAgentSdkConstants.DYNATRACE_MESSAGE_PROPERTYNAME] = outgoing_tag;

            Console.WriteLine("Sending message to Java:" + request.Text);
            producer.Send(request);

            outgoingMessageTracer.SetCorrelationId(request.NMSCorrelationID);    // optional
            outgoingMessageTracer.SetVendorMessageId(request.NMSMessageId); // optional
            outgoingMessageTracer.End();
        }
    }
}

