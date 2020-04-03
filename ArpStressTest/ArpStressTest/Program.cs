using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;

namespace ArpStressTest
{
    class Program
    {
        static void Main(string[] args)
        {
            var handler = new HttpClientHandler();
            handler.UseCookies = false;
            handler.ServerCertificateCustomValidationCallback = (requestMessage, certificate, chain, policyErrors) => true;


            var logFilePath = @"C:\Logs\wydajnosc.txt";
            List<string> message = new List<string>();
            var dataStart = DateTime.Now;
            var content1 = File.ReadAllText(@"JavaScript.json");
            List<Task> tasks = new List<Task>();

            var cookie = ConfigurationManager.AppSettings["cookie"] as string;
            var id = ConfigurationManager.AppSettings["id"] as string;
            var count = Convert.ToInt32(ConfigurationManager.AppSettings["count"]);

            for (int i = 0; i < count; i++)
            {
                tasks.Add(new TaskFactory().StartNew((ii) =>
                {
                    using (var httpClient = new HttpClient(handler))
                    {
                        using (var request = new HttpRequestMessage(new HttpMethod("POST"), "https://demo.pivotal.pl/ArpPortalWeb/UseCase/Execute"))
                        {
                            request.Headers.TryAddWithoutValidation("Cookie", cookie);

                            request.Content = new StringContent("{\"useCaseName\":\"GetApplications\",\"arguments\":{\"Id\":\"" + id + "\"}}");
                            request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse("application/json; charset=utf-8");

                            var response = httpClient.SendAsync(request).Result;
                            var responseContent123 = response.Content.ReadAsStringAsync().Result;
                            message.Add($"odczyt i: {ii}, {responseContent123.ToString()}");
                        }
                    }
                }, i));
            }

            Task.WaitAll(tasks.ToArray());

            var przetworzone = message.Where(x => x.Contains("jan")).Count();
            var zapis = message.Where(x => x.Contains("afterSave")).Count();

            File.AppendAllText(logFilePath, $"DataStart: {dataStart}, count:{tasks.Count}, odczyt:{przetworzone}, zapis:{zapis} ,dataKoniec:{DateTime.Now}, sekundy:{(DateTime.Now - dataStart).TotalSeconds}, przet/sek:{przetworzone / (DateTime.Now - dataStart).TotalSeconds}  {Environment.NewLine}");

        }
    }
}
