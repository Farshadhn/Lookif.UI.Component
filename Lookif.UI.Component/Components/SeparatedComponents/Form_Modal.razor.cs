using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Blazored.Modal;
using Blazored.Modal.Services;
using Lookif.UI.Component.Attributes;
using Lookif.UI.Component.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using static Newtonsoft.Json.JsonConvert;



namespace Lookif.UI.Component.Components.SeparatedComponents
{
    public partial class Form_Modal
    {


        #region ...Events...

        public override async Task SetParametersAsync(ParameterView parameters)
        {
            await base.SetParametersAsync(parameters);
            MyTItem = TItem;
            MyKey = Key;

        }

        #endregion



        #region ... Definition...

        public Type MyTItem { get; set; }
        public string MyKey { get; set; }


        #endregion


        #region ... Functions...



        async Task Close() => await BlazoredModal.CloseAsync(ModalResult.Ok(true));
        async Task Cancel() => await BlazoredModal.CancelAsync();

        async Task MyOnFinished(string s)
        {
            await OnFinished.InvokeAsync();
            await BlazoredModal.CloseAsync(ModalResult.Ok(true));
        }





        #endregion

        #region ... Parameter...

        [Parameter]
        public IStringLocalizer Resource { get; set; }
        [CascadingParameter] BlazoredModalInstance BlazoredModal { get; set; }
        [Parameter] public EventCallback<string> OnFinished { get; set; }
        [Parameter] public string Key { get; set; }
        [Parameter] public Type TItem { get; set; }

        #endregion








    }















}