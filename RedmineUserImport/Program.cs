﻿using System;
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
        private static readonly string userApiEndpoint = "/users.json?key=";
        private static readonly string headerValue = "application/json";

        static void Main(string[] args)
        {
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

                int i = 0;
                ValidateInput(input, ref i);

                client = new HttpClient();
                if (!ValidateConnection(input[0], client))
                {
                    return;
                }

                int span = i * 1000;
                using (var sr = new StreamReader(input[2], Encoding.UTF8))
                using (var csv = new CsvHelper.CsvReader(sr))
                {
                    csv.Configuration.HasHeaderRecord = false;
                    csv.Configuration.RegisterClassMap<RedmineUserImport.UserDetailMap>();
                    var users = csv.GetRecords<UserDetail>();

                    client.DefaultRequestHeaders.Accept.Add(new System.Net.Http.Headers.MediaTypeWithQualityHeaderValue(headerValue));
                    string uri = string.Concat(input[0], userApiEndpoint, input[1]);
                    foreach (var user in users)
                    {
                        string json = Newtonsoft.Json.JsonConvert.SerializeObject(new User(user));
                        var response = client.PostAsync(uri, new StringContent(json, Encoding.UTF8, headerValue)).Result;
                        var status = response.StatusCode.ToString();

                        if (status == "Created")
                        {
                            WriteLog(string.Concat("Success: ", user.Mail));
                            Console.WriteLine(string.Concat("Success: ", user.Mail));
                        }
                        else
                        {
                            WriteLog(string.Concat("Faild: ", user.Mail, " " , response.Content.ReadAsStringAsync().Result));
                            Console.WriteLine(string.Concat(string.Concat("Faild: ", user.Mail, " ", response.Content.ReadAsStringAsync().Result)));                            
                        }
                        Thread.Sleep(span);
                    }
                }
            }
            catch (Exception ex)
            {
                WriteLog(ex.Message);
                Console.WriteLine(ex.Message);
            }
            finally
            {
                client.Dispose();
                WriteLog("[INFO] " + DateTime.Now + " Finish.");
                Console.WriteLine(Environment.NewLine + "Finish. Please enter any keys.");
                Console.ReadLine();
            }
        }

        static bool ValidateInput(string[] input, ref int i)
        {
            if (input.Length > 4)
            {
                Console.WriteLine("Please input three or four arguments.");
                return false;
            }

            if (input.Length == 4)
            {
                if (!int.TryParse(input[3], out i))
                {
                    Console.WriteLine("Please input a numeric value for ther fourth argument.");
                    return false;
                }
            }

            if (!input[0].ToLower().StartsWith("http://") && !input[0].ToLower().StartsWith("https://"))
            {
                Console.WriteLine("Please add 'http://' or 'https://' on your url.");
                return false;
            }
            return true;
        }

        static bool ValidateConnection(string url, HttpClient client)
        {
            var response = client.GetAsync(url).Result;
            if (response.StatusCode.ToString() != "OK")
            {
                Console.WriteLine("Can not reach {0}", url);
                return false;
            }
            return true;
        }

        static void WriteLog(string s)
        {
            File.AppendAllText(logFile, s + Environment.NewLine, Encoding.UTF8);
        }
    }
}