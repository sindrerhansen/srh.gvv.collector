using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace SRH.VAR.SerielToDbService
{
    public class WriteToDb : IDisposable
    {
        private static string _database;
        private static string _url;
        private static HttpClient _httpClient;
        public WriteToDb(string url, string dbName)
        {
            _httpClient = new HttpClient();
            _database = dbName;
            _url = url;

            var stringContent = new StringContent(_database, Encoding.UTF8, "text/plain");

            _httpClient.PostAsync(_url + "/query ", stringContent);

            Console.WriteLine($"Database: {_database} created");
        }

        public void WriteToTbAsync(string sensorName, double value)
        {
            var stringContent = new StringContent($"{sensorName} value={value}", Encoding.UTF8, "text/plain");

            _httpClient.PostAsync(_url + "/write?db=" + _database, stringContent);
        }

        public void WriteMulipelValuesToDB(Dictionary<string, double> dic)
        {

            var valueString = "";
            foreach (var item in dic)
            {
                valueString += $"{item.Key} value={item.Value}/n";
            }

            var stringContent = new StringContent(valueString, Encoding.UTF8, "text/plain");

            _httpClient.PostAsync(_url + "/write?db=" + _database, stringContent);
        }

        public void Dispose()
        {
            if(_httpClient!=null)
                _httpClient.Dispose();
        }


    }
}
