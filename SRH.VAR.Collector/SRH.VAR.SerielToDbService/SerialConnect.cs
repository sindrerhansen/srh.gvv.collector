using System;
using System.Diagnostics;
using System.IO.Ports;
using System.Threading;

namespace SRH.VAR.SerielToDbService
{
    public class SerialConnect
    {
        Timer timer;
        public SerialConnect(string port)
        {


            var mySerialPort = new SerialPort(port);

            mySerialPort.BaudRate = 9600;
            mySerialPort.Parity = Parity.None;
            mySerialPort.StopBits = StopBits.One;
            mySerialPort.DataBits = 8;
            mySerialPort.Handshake = Handshake.None;

            mySerialPort.DataReceived -= new SerialDataReceivedEventHandler(DataReceivedHandler);
            mySerialPort.DataReceived += new SerialDataReceivedEventHandler(DataReceivedHandler);
            mySerialPort.ErrorReceived += MySerialPort_ErrorReceived;
            mySerialPort.Disposed += MySerialPort_Disposed;
            
            mySerialPort.Open();
        }


        private void MySerialPort_Disposed(object sender, EventArgs e)
        {
            Console.WriteLine("MySerialPort_Disposed");
        }

        private void MySerialPort_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            Console.WriteLine("MySerialPort_ErrorReceived");
        }

        private void DataReceivedHandler( object sender,
                    SerialDataReceivedEventArgs e)
        {
            var sp = (SerialPort)sender;
            string indata = sp.ReadLine();
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
