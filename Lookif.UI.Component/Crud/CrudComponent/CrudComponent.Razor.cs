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

        [Parameter] public bool DeleteBtn { get; set; } = true;

        private string ModelName => typeof(TItem).Name.Replace("Dto", "");
        [Parameter]
        public string FormName { get; set; }
        [Parameter]
        public IStringLocalizer Resource { get; set; }
        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
                await Bind();
            await base.OnAfterRenderAsync(firstRender);


        }

        private async Task Bind(string inserted = default)
        {
            var dataObj = await Http.GetAsync($"{ModelName}/Get");

            var content = await dataObj.Content.ReadAsStringAsync();
            
            var data = DeserializeObject<ApiResult<List<TSelectItem>>>(content);

            Records = data.Data;

        }


        private async Task OnDeleteFinished(string Id)
        {
            await Bind();
        }

        private async Task OnClearCache()
        {
            await Http.DeleteAsync("RequestedModel/ClearCache");
        }


    }




}