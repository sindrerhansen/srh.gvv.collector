using System.Configuration;

namespace SRH.VAR.SerielToDbDotNetService
{
    public static class ConfigurationHolder
    {
        public static string SerialPort { get; private set; }
        public static string InfluxDbServer { get; private set; }
        public static string InfluxDatabase { get; private set; }
        static ConfigurationHolder()
        {
            SerialPort = ConfigurationManager.AppSettings["SerialPort"];
            InfluxDbServer = ConfigurationManager.AppSettings["InfluxDbServer"];
            InfluxDatabase = ConfigurationManager.AppSettings["InfluxDatabase"];
        }
    }
}
