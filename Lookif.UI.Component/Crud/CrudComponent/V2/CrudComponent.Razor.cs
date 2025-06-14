﻿using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Lookif.UI.Common.Models;
using static Newtonsoft.Json.JsonConvert;
using Microsoft.Extensions.Localization;
using Blazored.Modal;
using Lookif.UI.Component.Components.SeparatedComponents;
using Blazored.Modal.Services;

namespace Lookif.UI.Component.Crud.CrudComponent.V2;

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



    [CascadingParameter] public IModalService Modal { get; set; }


    protected override async Task OnParametersSetAsync()
    {
          await base.OnParametersSetAsync();
    }
    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender) {
            await base.OnAfterRenderAsync(firstRender);
            await Bind();
        }

    }
 
    private async Task Bind(string inserted = default)
    {
        var dataObj = await Http.GetAsync($"{ModelName}/Get");

        var content = await dataObj.Content.ReadAsStringAsync();

        var data = DeserializeObject<ApiResult<List<TSelectItem>>>(content);

        Records = data.Data;
        StateHasChanged();
    }


    private async Task OnDeleteFinished(string Id)
    {
        await Bind();
    }

    private async Task OnClearCache()
    {
        await Http.DeleteAsync("RequestedModel/ClearCache");
    }

    async Task InsertNew()
    {
        var options = new ModalOptions()
        {
            Class = "size-automatic",
            HideHeader = true,
            Position = ModalPosition.Middle,
            AnimationType = ModalAnimationType.FadeInOut
        };
        var parameters = new ModalParameters();

        parameters.Add("TItem", typeof(TItem));
        parameters.Add("Resource", Resource);
        var MessageForm = Modal.Show<Form_Modal>(basicResource["Add"], parameters, options);
        var result = await MessageForm.Result;
        if (result.Confirmed || !result.Cancelled )
            await Bind();
    }

}