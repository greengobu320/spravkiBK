using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net.Http;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace spravkiBK
{
    internal class GetEIAPData
    {

        public Tuple <Dictionary <string,string>, DataTable> getData()
        {
            Dictionary <string, string > resultDict = new Dictionary <string, string> ();
            DataTable dataTable=new DataTable ();  
            return Tuple.Create(resultDict, dataTable);
        }
        public async Task <Dictionary<string,string>> GetAsyncData(string value)
        {
            string baseUrl = "http://ekpiprom.ais3.tax.nalog.ru:9400/inias/csc/pf-is/app-dnpw/service/dnpwsearch/api/v1/search/query";
            string baseQuery = "{\"Query\": \"" + value + "\" ,\"PageNumber\": 0,\"ObjectTypesToSearch\": [3]}";           
           return await GetData(baseUrl, baseQuery);
        }

        public async Task<Dictionary <string,string >> GetAsyncDetailData(string url, string value)
        {
               return await GetData(url, value);
        }
        private async Task <Dictionary <string,string>> GetData (string url, string data, int timeOut=30)
        {
            var resultDict=new Dictionary <string,string>
            {{"value", string.Empty  }, {"result", string.Empty }, {"message", string.Empty  }};
            int tryCount = 0;
            while (tryCount <3)
            {
                using (var client=new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(timeOut);
                    var content = new StringContent(data, Encoding.UTF8, "application/json");
                    try
                    {
                        var response = await client.PostAsync(url, content);
                        response.EnsureSuccessStatusCode();
                        var responseBody = await response.Content.ReadAsStringAsync();
                        resultDict["value"] = responseBody;
                        resultDict["result"] = "success";
                        break;
                    }
                    catch (Exception ex)
                    {
                        tryCount++;
                        resultDict["message"] = ex.Message;
                        resultDict["result"] = "error";
                    }
                }
            }
            return resultDict;
        }

    }


}
