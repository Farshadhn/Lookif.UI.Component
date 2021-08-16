
using Lookif.UI.Component.Components.SeparatedComponents.SimpleForm;
using Lookif.UI.Component.Models;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Lookif.UI.Common.Models;
using static Newtonsoft.Json.JsonConvert;
namespace Lookif.UI.Component.Utility
{
    public class BaseForm : IBaseForm
    {
        public BaseForm(HttpClient httpClient)
        {
            HttpClient = httpClient;
        }

        public HttpClient HttpClient { get; }

        public async Task<List<RelatedTo>> GetRelatedTo(string entityName, string displayName)
        {
            entityName = entityName.Replace("Dto", "");

            List<RelatedTo> relatedTos = new List<RelatedTo>();
            var data = await HttpClient.GetFromJsonAsync<ApiResult<List<RelatedTo>>>($"{entityName}/Get");

            foreach (var item in data.Data)
            {

                var idProp = item.GetType().GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
                var displayProp = item.GetType().GetProperty(displayName, BindingFlags.Public | BindingFlags.Instance);

                var a = new RelatedTo() { Name = displayProp?.GetValue(item, null)?.ToString(), Id = idProp?.GetValue(item, null)?.ToString() };

                relatedTos.Add(a);
            }
            return relatedTos;


        }

        //public async Task<T> Insert(string domainName, T input)
        //{
        //    var content = new StringContent(SerializeObject(input), Encoding.ASCII, "application/json");
        //    var responseMessage = await HttpClient.PostAsync($"{domainName}/create", content);
        //    responseMessage.EnsureSuccessStatusCode();
        //    var res = await responseMessage.Content.ReadAsStringAsync();
        //    return DeserializeObject<ApiResult<T>>(res).Data;

        //}
    }
}
