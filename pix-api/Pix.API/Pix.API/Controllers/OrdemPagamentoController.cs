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
//using System.Web.Mvc;

// In SDK-style projects such as this one, several assembly attributes that were historically
// defined in this file are now automatically added during build and populated with
// values defined in project properties. For details of which attributes are included
// and how to customise this process see: https://aka.ms/assembly-info-properties


// Setting ComVisible to false makes the types in this assembly not visible to COM
// components.  If you need to access a type in this assembly from COM, set the ComVisible
// attribute to true on that type

[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM.

[assembly: Guid("511159ec-0bc3-4f50-9223-56578624d53d")]

namespace Pix.API.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class OrdemPagamentoController : System.Web.Mvc.ControllerBase
    {
        private static readonly string[] status = new[]
        {
            "started", "bacen", "other_institution", "finished"
        };

        private readonly ILogger<OrdemPagamentoController> _logger;
        private readonly MySqlConnection _connection;

        public OrdemPagamentoController(ILogger<OrdemPagamentoController> logger)
        {
            _logger = logger;
        }

        [Route("ordem_pagamento/{id}")]
        [HttpGet]
        public string Get(String id)
        {
            IOneAgentSdk OneAgentSdk = OneAgentSdkFactory.CreateInstance();
            IIncomingRemoteCallTracer tracer = OneAgentSdk.TraceIncomingRemoteCall("GetOrdemPagamentoStatus", "Pix.API", "http://pix.status/");
            
            using var connection = new MySqlConnection("Server = 127.0.0.1; Port = 3306; Database = pix; Uid = root; Pwd = 123456789");
            //server = localhost;portuser=root;password=123456789;database=pix");
            connection.Open();
            Console.WriteLine("Getting correlation from database...");
            using var command = new MySqlCommand("SELECT dt_tag, start_time FROM dynatrace_correlation where End_To_EndId = '" + id + "';", connection);
            using var reader = command.ExecuteReader();
            string incomingDynatraceStringTag = "";
            long start_time = 0;
            if (reader.HasRows)
            {
                while (reader.Read())
                {
                    var value = reader.GetValue(0);
                    if ((string)value != "")
                    {
                        Console.WriteLine("Found DT Tag.");
                        incomingDynatraceStringTag = (String)value;
                        start_time = (long)reader.GetValue(1);
                        Console.WriteLine("Start Time: " + start_time);
                    }
                    else
                    {
                        Console.WriteLine("DT Tag not found.");
                        //return_value = "NO DATA";
                    }
                }
            }
            
            connection.Close();

            

            if (incomingDynatraceStringTag != "") tracer.SetDynatraceStringTag(incomingDynatraceStringTag);
            tracer.SetProtocolName("HTTP");

            tracer.Start();
            //EXECUTE NORMAL API TRANSACTIONS AND GET PIX STATUS

            //using var connection2 = new MySqlConnection("Server = 127.0.0.1; Port = 3306; Database = pix; Uid = root; Pwd = 123456789");
            connection.Open();
            //using var connection2 = new MySqlConnection("Server = 127.0.0.1; Port = 3306; Database = pix; Uid = root; Pwd = 123456789");
            using var command2 = new MySqlCommand("SELECT End_To_EndId FROM pix_operation where End_To_EndId = '" + id + "';", connection);
            using var reader2 = command2.ExecuteReader();

            String return_value = "";
            if(reader2.HasRows)
            {
                while (reader2.Read())
                {
                    var value = reader2.GetValue(0);
                    Console.WriteLine("Valor consultado na base: "+ reader2.GetValue(0));
                    if ((string)value != "")
                    {
                        Console.WriteLine("Ordem Pagamento encontrada.");
                        return_value = id;
                    }
                    else
                    {
                        Console.WriteLine("Ordem Pagamento NÃO encontrada.");
                        return_value = "NO DATA";
                    }
                }
            }
            else
            {
                Console.WriteLine("Ordem Pagamento NÃO encontrada.");
                return_value = "NO DATA";
            }
            Console.WriteLine("EndToEndID: " + return_value);


            if ((incomingDynatraceStringTag != "")&&(return_value != "NO DATA"))
            {
                long current_time = DateTimeOffset.Now.ToUnixTimeMilliseconds();
                long total_time = (current_time - start_time);// / 60 / 1000;
                Console.WriteLine("Star time: " + start_time);
                Console.WriteLine("Current time: " + current_time);
                Console.WriteLine("Tempo PIXTotalTime: " + total_time);
                OneAgentSdk.AddCustomRequestAttribute("PIXTotalTime", total_time);
            }
            else
            {
                Console.WriteLine("Tempo PIXTotalTime está vazio" );
            }
            
            connection.Close();
            tracer.End();
            return return_value; // == "NO DATA" ? new HttpStatusCodeResult(404) : new HttpStatusCodeResult(200);
        }

        protected override void ExecuteCore()
        {
            throw new NotImplementedException();
        }
    }
}
