using System;
using System.Collections.Generic;
using System.Net;
using Newtonsoft.Json;
using System.Linq;
using System.Diagnostics;
using RestSharp;
using Autodesk.Revit.DB;

namespace Viper
{

    public class SystemData
    {

        [JsonProperty("indicies")]
        public List<List<int>> indicies { get; set; }
        [JsonProperty("geom")]
        public List<List<double>> geom{ get; set; }

        [JsonProperty("symbols")]
        public List<List<int>> symbols { get; set; }


        
    }

    public static class rvClient
    {
       // private static readonly HttpClient client = new HttpClient();
        private static string url = "http://localhost:3000/";

        public static string getNextTask()
        {
            string result = "";
            using (var wb = new WebClient())
            {
                result = wb.DownloadString(url + "new");
            }
            return result;
        }

        public static string submitComplete(string endpoint, string job, string hashed)
        {
            string result = "";
            using (var wb = new WebClient())
            {
                wb.QueryString.Add("job", job);
                wb.QueryString.Add("done", hashed);
                wb.QueryString.Add("real", "true");
                result = wb.DownloadString(url + endpoint);
            }
            return result;
        }

        public static void DumpPoints(List<ISerialGeom> data, List<XYZ> points, string path)
        {
            //open file stream
            string datastr = JsonConvert.SerializeObject(data.Select(x => x.Serialize()));
            string pntsstr = JsonConvert.SerializeObject(points.Select(x => new[] { x.X, x.Y, x.Z }));
            string json = JsonConvert.SerializeObject(new { data = datastr, points = pntsstr });
            System.IO.File.WriteAllText(path, json);
        }

        public static void DumpList(List<ISerialGeom> data,string path)
        {
            //open file stream
            string datastr = JsonConvert.SerializeObject(data.Select(x => x.Serialize()));
            System.IO.File.WriteAllText(path, datastr);
        }

        public static SystemData SendPoints(List<ISerialGeom> data, List<XYZ> points)
        {
            string datastr = JsonConvert.SerializeObject(data.Select(x => x.Serialize()));
            string pntsstr = JsonConvert.SerializeObject(points.Select(x => new[] { x.X, x.Y, x.Z } ));
            var client = new RestClient("http://10.0.1.37:8888/"); 
            var request = new RestRequest(Method.POST);
            request.RequestFormat = DataFormat.Json;
            request.AddJsonBody(new { data = datastr, points = pntsstr });
            request.AddHeader("Content-Type", "application/json");
            IRestResponse response = client.Execute(request);
            return JsonConvert.DeserializeObject<SystemData>(response.Content);

        }

        public static string submitsheet(string job, List<TwoPoint> data)
        {
            
            string datastr = JsonConvert.SerializeObject(data.Select(x => x.Serialize()));

            datastr = datastr.Replace("&", "");

            Debug.WriteLine(datastr);
            string result = "";
            using (var wb = new WebClient())
            {
                wb.Headers.Add(HttpRequestHeader.ContentType, "application/json");
                result = wb.UploadString(job, "POST", datastr);
            }
            Debug.WriteLine(result);
            return result;
        }
    }

    class ViperClient:WebClient
    {
        private string _url;
         
        public ViperClient(string url)
        {
            _url = url;
        }

        public string SendImport(List<TwoPoint> data)
        {
            string datastr = JsonConvert.SerializeObject(data);
            QueryString.Add("data", datastr);

            return "";
        }
    }


}
