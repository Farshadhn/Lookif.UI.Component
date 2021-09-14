using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Lookif.UI.Common.Models;
using static Newtonsoft.Json.JsonConvert;
using Microsoft.Extensions.Localization;

namespace Lookif.UI.Component.Crud.CrudComponent
{
    public partial class CrudComponent<TItem, TSelectItem>
        where TSelectItem : TItem
        where TItem : class
    {
        DateTime? value = DateTime.Now;


        [Inject] HttpClient Http { get; set; }
        public List<TSelectItem> Records = new List<TSelectItem>();

        private string ModelName => typeof(TItem).Name.Replace("Dto", "");
        [Parameter]
        public string FormName { get; set; }
        [Parameter]
        public IStringLocalizer Resource { get; set; }
        protected override async Task OnInitializedAsync()
        { 
            await base.OnInitializedAsync();
            await Bind();

        }
        private async Task Bind(string inserted = default)
        {  
            var dataObj = await Http.GetFromJsonAsync<object>($"{ModelName}/Get");
             
            var data = DeserializeObject<ApiResult<List<TSelectItem>>>(dataObj.ToString()!);
             
            Records = data.Data;

        }





      
    }




}