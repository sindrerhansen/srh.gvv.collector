
using InfluxDB.Net;
using InfluxDB.Net.Models;
using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SRH.VAR.SerielToDbService
{
    class Program
    {
        private static InfluxDb InfluxClient;
        private static string DbName = "GVV";

        #region Properties
        private static double tureTemperature;

        public static double TureTemperature {
            get { return tureTemperature; }
            set { 
                if(value!=tureTemperature)
                {
                    tureTemperature = value;
                    var point = new Point();
                    point.Measurement = "TureTemperature";
                    point.Tags.Add("PI", "SRH-GVV");
                    point.Fields.Add("value", value);
                    var r = InfluxClient.WriteAsync(DbName, point);
                    Console.WriteLine("TureTemperature changed to:" + value.ToString());
                }        
            }
        }

        private static double retureTemperature;
        public static double RetureTemperature
        {
            get { return retureTemperature; }
            set
            {
                if (value != retureTemperature)
                {
                    retureTemperature = value;
                    var point = new Point();
                    point.Measurement = "RetureTemperature";
                    point.Tags.Add("PI", "SRH-GVV");
                    point.Fields.Add("value", value);
                    var r = InfluxClient.WriteAsync(DbName, point);
                    Console.WriteLine("RetureTemperature changed to:" + value.ToString());
                }
            }
        }

        private static double flow;
        public static double Flow
        {
            get { return flow; }
            set
            {
                if (value != flow)
                {
                    flow = value;
                    var point = new Point();
                    point.Measurement = "Flow";
                    point.Tags.Add("PI", "SRH-GVV");
                    point.Fields.Add("value", value);
                    var r = InfluxClient.WriteAsync(DbName, point);
                    Console.WriteLine("Flow changed to:" + value.ToString());
                }
            }
        }

        private static double totalVolume;
        public static double TotalVolume
        {
            get { return totalVolume; }
            set
            {
                if (value != totalVolume)
                {
                    totalVolume = value;
                    var point = new Point();
                    point.Measurement = "TotalVolume";
                    point.Tags.Add("PI", "SRH-GVV");
                    point.Fields.Add("value", value);
                    var r = InfluxClient.WriteAsync(DbName, point);
                    Console.WriteLine("TotalVolume changed to:" + value.ToString());
                }
            }
        }



        private static UInt32 totaleCounter;
        public static UInt32 TotaleCounter
        {
            get { return totaleCounter; }
            set
            {
                if (value != totaleCounter)
                {
                    totaleCounter = value;
                    var point = new Point();
                    point.Measurement = "TotaleCounter";
                    point.Tags.Add("PI", "SRH-GVV");
                    point.Fields.Add("value", value);
                    var r = InfluxClient.WriteAsync(DbName, point);
                    Console.WriteLine("TotaleCounter changed to:" + value.ToString());
                }
            }
        }
        #endregion
        static void Main(string[] args)
        {
            
            InfluxClient = new InfluxDb("http://192.168.1.11:8086", "GVV", "Kaffe123");
            var pong = InfluxClient.PingAsync().Result;
            Console.WriteLine($"Ping InfluDB is: {pong.Success.ToString()}");
            Console.WriteLine($"InfluDB Version is: {pong.Version.ToString()}");
            var response = InfluxClient.CreateDatabaseAsync(DbName).Result;
            Task.Factory.StartNew(() => {
                var ser = new SerialConnect("COM5");
                ser.serielDataRecivedEvent += Ser_serielDataRecivedEvent;
            });

            Console.ReadLine();
        }

        private static void Ser_serielDataRecivedEvent(string sender)
        {
            //Console.WriteLine("Data from Event");
            //Console.WriteLine(sender);
            sender.Trim();
            var cleanString = Regex.Replace(sender, @"\t\n\r", "");
            String[] stringSplitt = cleanString.Split('|');
            
            if(stringSplitt[0] =="6" && stringSplitt.Length>=4)
            {
                double dRes;
                UInt32 uiRes;
                if (double.TryParse(stringSplitt[1], out  dRes))
                    TotalVolume = dRes;
                if (double.TryParse(stringSplitt[2], out  dRes))
                    Flow = dRes;
                if (UInt32.TryParse(stringSplitt[3], out uiRes))
                    TotaleCounter = uiRes;
            }       
        }

        private static double? SetDoublePropertyFromString(string s)
        {
            //if (p.GetType().Equals(typeof(Double)))
            //{
            //    if (double.TryParse(s, out var result))
            //        return  result;
            //}

            if (double.TryParse(s, out var result))
                return result;
            return null;
        }
    }
}
