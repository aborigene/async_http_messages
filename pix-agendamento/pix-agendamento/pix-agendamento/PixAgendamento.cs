using System.Runtime.InteropServices;
using System;

namespace pix_agendamento
{
    public class PixAgendamento
    {
        public long Date { get; set; }

        public int Value { get; set; }

        public String End2EndID { get; set; }

        public String toString()
        {
            return "{\"EndToEndID\": \"" + End2EndID + "\", \"Value\": \"" + Value + "\", \"Date\": \"" + Date.ToString()+ "\"}";
        }

    }
}

