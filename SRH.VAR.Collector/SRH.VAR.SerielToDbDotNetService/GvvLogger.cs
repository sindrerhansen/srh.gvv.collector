using System;
using InfluxDB.Net;
using InfluxDB.Net.Models;
using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Linq;
using log4net;

namespace SRH.VAR.SerielToDbDotNetService
{
    public class GvvLogger
    {
        private InfluxDb InfluxClient;
        private SerialConnect SerialConnect;
        private Pong pong;

        private const double maxFlowLimit = 1000;
        private double volumeChangeLimit = 5;
        private bool volumeRecivedOnce;
        private UInt32 flowCounterChangeLimit = 1000;
        private bool flowCounterRecivedOnce;

        static ILog _log = LogManager.GetLogger(typeof(GvvLogger));

        public GvvLogger()
        {}

        public void Start()
        {
            _log.Info("Service starting");
            InfluxClient = new InfluxDb(ConfigurationHolder.InfluxDbServer, ConfigurationHolder.InfluxDatabase, "Kaffe123");
            pong = InfluxClient.PingAsync().Result;
            _log.Info($"Ping InfluDB is: {pong.Success.ToString()}");
            _log.Info($"InfluDB Version is: {pong.Version.ToString()}");
            var response = InfluxClient.CreateDatabaseAsync(ConfigurationHolder.InfluxDatabase).Result;
            Task.Factory.StartNew(() => {
                SerialConnect = new SerialConnect(ConfigurationHolder.SerialPort);
                SerialConnect.serielDataRecivedEvent -= Ser_serielDataRecivedEvent;
                SerialConnect.serielDataRecivedEvent += Ser_serielDataRecivedEvent;
            });
        }

        public void Stop()
        {
            _log.Info("Service stopped");
            SerialConnect.serielDataRecivedEvent -= Ser_serielDataRecivedEvent;
        }

        private void Ser_serielDataRecivedEvent(string sender)
        {
            sender.Trim();
            var cleanString = Regex.Replace(sender, @"\t\n\r", "");
            String[] stringSplitt = cleanString.Split('|');

            if (stringSplitt[0] == "6" && stringSplitt.Length >= 5)
            {
                double dRes;
                if(FormatHelper.StringToBoolean(stringSplitt[1]))
                {
                    var series = InfluxClient.QueryAsync(ConfigurationHolder.InfluxDatabase, "SELECT LAST(value), * from TotaleCounter").Result;
                    if(series.Count>0)
                    {
                        var lastSerie = series.FirstOrDefault();
                        var lastValue = lastSerie.Values.FirstOrDefault()[1];
                        SerialConnect.SendString("SetTotFlow" + lastValue);
                    }

                    else
                        SerialConnect.SendString("SetTotFlow" + "0");
                }
                    
                else
                {
                    if (double.TryParse(stringSplitt[2], out dRes))
                        TotalVolume = dRes;
                    if (UInt32.TryParse(stringSplitt[4], out uint uiRes))
                        TotaleCounter = uiRes;
                }


                if (double.TryParse(stringSplitt[3], out dRes))
                    Flow = dRes;

            }
            else if(stringSplitt[0] == "7" && stringSplitt.Length>=4)
            {
                if (double.TryParse(stringSplitt[1], out double dRes))
                    TureTemperature = dRes;

                if (double.TryParse(stringSplitt[2], out dRes))
                    RetureTemperature = dRes;

                if (double.TryParse(stringSplitt[3], out dRes))
                    InsideTemperature = dRes;
            }
        }

        #region Properties
        private double tureTemperature;

        public double TureTemperature
        {
            get { return tureTemperature; }
            set
            {
                if (value < 99 || value > 0)
                {
                    tureTemperature = value;
                    var point = new Point();
                    point.Measurement = "TureTemperature";
                    point.Tags.Add("PI", "SRH-GVV");
                    point.Fields.Add("value", value);
                    InfluxClient.WriteAsync(ConfigurationHolder.InfluxDatabase, point);
                    Console.WriteLine("TureTemperature changed to:" + value.ToString());
                }
            }
        }

        private double retureTemperature;
        public double RetureTemperature
        {
            get { return retureTemperature; }
            set
            {
                if (value < 99 || value > 0)
                {
                    retureTemperature = value;
                    var point = new Point();
                    point.Measurement = "RetureTemperature";
                    point.Tags.Add("PI", "SRH-GVV");
                    point.Fields.Add("value", value);
                    InfluxClient.WriteAsync(ConfigurationHolder.InfluxDatabase, point);
                    Console.WriteLine("RetureTemperature changed to:" + value.ToString());
                }
            }
        }

        private double insideTemperature;
        public double InsideTemperature
        {
            get { return insideTemperature; }
            set
            {
                if (value < 99 || value > 0)
                {
                    insideTemperature = value;
                    var point = new Point();
                    point.Measurement = "InsideTemperature";
                    point.Tags.Add("PI", "SRH-GVV");
                    point.Fields.Add("value", value);
                    InfluxClient.WriteAsync(ConfigurationHolder.InfluxDatabase, point);
                    Console.WriteLine("InsideTemperature changed to:" + value.ToString());
                }
            }
        }

        private double flow;
        public double Flow
        {
            get { return flow; }
            set
            {
                if ((value - flow)<maxFlowLimit)
                {
                    flow = value;
                    var point = new Point();
                    point.Measurement = "Flow";
                    point.Tags.Add("PI", "SRH-GVV");
                    point.Fields.Add("value", value);
                    var r = InfluxClient.WriteAsync(ConfigurationHolder.InfluxDatabase, point);
                    if (!r.Result.Success)
                        _log.Error("Failing writing to InfluxDatabase: " + ConfigurationHolder.InfluxDatabase + " Error: " + r.Result.StatusCode.ToString());
                    Console.WriteLine("Flow changed to:" + value.ToString());
                }
            }
        }

        private double totalVolume;
        public double TotalVolume
        {
            get { return totalVolume; }
            set
            {
                if (!volumeRecivedOnce || (value - totalVolume) < volumeChangeLimit)
                {
                    totalVolume = value;
                    var point = new Point();
                    point.Measurement = "TotalVolume";
                    point.Tags.Add("PI", "SRH-GVV");
                    point.Fields.Add("value", value);
                    InfluxClient.WriteAsync(ConfigurationHolder.InfluxDatabase, point);
                    Console.WriteLine("TotalVolume changed to:" + value.ToString());
                    volumeRecivedOnce = true;
                }
            }
        }



        private UInt32 totaleCounter;
        public UInt32 TotaleCounter
        {
            get { return totaleCounter; }
            set
            {
                if (!flowCounterRecivedOnce || (value - flow) < flowCounterChangeLimit)
                {
                    totaleCounter = value;
                    var point = new Point();
                    point.Measurement = "TotaleCounter";
                    point.Tags.Add("PI", "SRH-GVV");
                    point.Fields.Add("value", value);
                    InfluxClient.WriteAsync(ConfigurationHolder.InfluxDatabase, point);
                    Console.WriteLine("TotaleCounter changed to:" + value.ToString());
                    flowCounterRecivedOnce = true;
                }
            }
        }
        #endregion
    }
}
