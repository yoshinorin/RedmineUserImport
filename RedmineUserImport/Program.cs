using System;
using System.Text;
using System.IO;
using System.Net.Http;
using System.Threading;

namespace RedmineUserImport
{
    class Program
    {
        private static readonly string logDir = Path.Combine(Environment.CurrentDirectory, "log");
        private static readonly string logFile = Path.Combine(Environment.CurrentDirectory, logDir,"create.log");
        private static readonly string urlParameter = "/users.json?key=";
        private static readonly string contentType = "application/json";

        static void Main(string[] args)
        {
            Console.WriteLine("Waiting to enter values... Please input URL and APIKey and CSV File's path below..." + Environment.NewLine);
            var input = Console.ReadLine().Split(' ');
            HttpClient client = null;

            try
            {
                Directory.CreateDirectory(logDir);
                if (!File.Exists(logFile))
                {
                    File.Create(logFile).Dispose();
                }
                WriteLog("[INFO] " +  DateTime.Now + " Start.");

                //Convert to absolute path.
                if (!Path.IsPathRooted(input[2]))
                {
                    input[2] = Path.GetFullPath(input[2]);
                }

                int i = 0;
                if (!ValidateInput(input, ref i))
                {
                    return;
                }

                client = new HttpClient();
                if (!ValidateConnection(input[0], client))
                {
                    return;
                }

                using (var sr = new StreamReader(input[2], Encoding.UTF8))
                using (var csv = new CsvHelper.CsvReader(sr))
                {
                    csv.Configuration.HasHeaderRecord = false;
                    csv.Configuration.RegisterClassMap<RedmineUserImport.UserDetailMap>();
                    var users = csv.GetRecords<UserDetail>();

                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(contentType));
                    int span = i * 1000;
                    string uri = string.Concat(input[0], urlParameter, input[1]);
                    foreach (var user in users)
                    {
                        string json = Newtonsoft.Json.JsonConvert.SerializeObject(new User(user));
                        var response = client.PostAsync(uri, new StringContent(json, Encoding.UTF8, contentType)).Result;

                        if (response.StatusCode.ToString() == "Created")
                        {
                            WriteLog(string.Concat("Success: ", user.Mail));
                        }
                        else
                        {
                            WriteLog(string.Concat("Faild: ", user.Mail, " " , response.Content.ReadAsStringAsync().Result));
                        }
                        Thread.Sleep(span);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
            }
            finally
            {
                if (client != null)
                {
                    client.Dispose();
                }
                WriteLog("[INFO] " + DateTime.Now + " Finish.");
                Console.WriteLine(Environment.NewLine + "Finish. Please enter any keys.");
                Console.ReadLine();
            }
        }

        static bool ValidateInput(string[] input, ref int i)
        {
            if (input.Length > 4)
            {
                WriteLog("Please input three or four arguments.");
                return false;
            }

            if (input.Length == 4)
            {
                if (!int.TryParse(input[3], out i))
                {
                    WriteLog("Please input a numeric value for ther fourth argument.");
                    return false;
                }
            }

            if (!File.Exists(input[2]))
            {
                WriteLog("Not found CSV file.");
                return false;
            }

            if (!input[0].ToLower().StartsWith("http://") && !input[0].ToLower().StartsWith("https://"))
            {
                WriteLog("Please add 'http://' or 'https://' on your url.");
                return false;
            }
            return true;
        }

        static bool ValidateConnection(string url, HttpClient client)
        {
            var response = client.GetAsync(url).Result;
            if (response.StatusCode.ToString() != "OK")
            {
                WriteLog(string.Concat("Can not reach ", url));
                return false;
            }
            return true;
        }

        static void WriteLog(string s)
        {
            Console.WriteLine(s);
            File.AppendAllText(logFile, s + Environment.NewLine, Encoding.UTF8);
        }
    }
}
