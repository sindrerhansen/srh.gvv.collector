using log4net;
using System;
using System.IO.Ports;

namespace SRH.VAR.SerielToDbDotNetService
{
    public class SerialConnect
    {
        System.Timers.Timer timer;
        SerialPort mySerialPort;
        string SerialPort;

        static ILog _log = LogManager.GetLogger(typeof(GvvLogger));
        public SerialConnect(string port)
        {
            _log.Info("Serial connecting to port: " + port);
            SerialPort = port;
            Connect(port);
            timer = new System.Timers.Timer(2000);
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
        {
            _log.Error("No data recived, resetting seriel connection");
            mySerialPort.DataReceived -= new SerialDataReceivedEventHandler(DataReceivedHandler);
            mySerialPort.Close();
            mySerialPort.Dispose();
            Connect(SerialPort);
        }

        public bool SendString(string s)
        {
            try
            {
                mySerialPort.WriteLine(s);
                return true;
            }
            catch (Exception e)
            {
                _log.Error("Failing to write to serial", e);
                return false; ;
            }           
        }

        private void Connect(string port)
        {
            try
            {
                _log.Info("Connecting to serielport: " + port);
                mySerialPort = new SerialPort(port);

                mySerialPort.BaudRate = 9600;
                mySerialPort.Parity = Parity.None;
                mySerialPort.StopBits = StopBits.One;
                mySerialPort.DataBits = 8;
                mySerialPort.Handshake = Handshake.None;
                mySerialPort.RtsEnable = true;
                mySerialPort.DataReceived -= new SerialDataReceivedEventHandler(DataReceivedHandler);
                mySerialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);

                mySerialPort.Open();
            }
            catch (Exception e)
            {
                _log.Error("Failing to connect to serial port: " + port, e);
                mySerialPort.Close();
                mySerialPort.Dispose();
                Console.WriteLine(e.ToString());
                System.Threading.Thread.Sleep(1000);
            }

        }

        private void DataReceivedHandler(object sender, SerialDataReceivedEventArgs e)
        {
            var sp = (SerialPort)sender;
            string indata = sp.ReadLine();
            timer.Stop();
            timer.Start();
            SerielDataRecived(indata);
        }


        public delegate void SerielDataRecivedDeligate(string sender);
        public event SerielDataRecivedDeligate serielDataRecivedEvent;

        private void SerielDataRecived(string data)
        {
            if (serielDataRecivedEvent != null)
                serielDataRecivedEvent.Invoke(data);
        }
    }
}
