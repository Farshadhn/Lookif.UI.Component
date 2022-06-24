using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Threading.Tasks;
using Blazored.Modal;
using Blazored.Modal.Services;
using Lookif.UI.Component.Attributes;
using Lookif.UI.Component.Components.SeparatedComponents;
using Lookif.UI.Component.Models;
using Microsoft.AspNetCore.Components;
using Lookif.UI.Common.Models;
using Microsoft.Extensions.Localization;
using Blazored.Toast.Services;
using Lookif.UI.Component.Modals;
using static Newtonsoft.Json.JsonConvert;

namespace Lookif.UI.Component.Grids
{
    public partial class GridView<TSelectItem, TItem> where TItem : class
    {


        private int _count = 100;
        [Inject] HttpClient Http { get; set; }
        [Inject] IToastService toastService { get; set; }
        private string ModelName => typeof(TItem).Name.Replace("Dto", "");


        List<LocalizedString> relatedSource;
        List<LocalizedString> commonResource; //Used For Buttons 



        private async Task<List<List<ValuePlaceHolder>>> Bind()
        {
            var dataObj = await Http.GetFromJsonAsync<ApiResult<List<TSelectItem>>>($"{ModelName}/Get");
            Records = dataObj.Data;

            return ConvertInputToListOfValuePlaceHolders(dataObj.Data);

        }
        public GridView()
        {


            FindAllProperties();

        }


        /// <summary>
        /// Convert Input to a beknown List
        /// </summary>
        /// <param name="records"></param>
        /// <returns></returns>
        public List<List<ValuePlaceHolder>> ConvertInputToListOfValuePlaceHolders(List<TSelectItem> records = null)
        {
             
            if (Records is not null)
            { 
                if (records is null)
                    records = Records; 
                ConvertedRecords = new List<List<ValuePlaceHolder>>();
                foreach (var input in records)
                { 
                    var temp = new List<ValuePlaceHolder>();
                    foreach (var item in PropertiesInformation)
                    {
                        try
                        { 
                            var vph = new ValuePlaceHolder();
                            var prop = input.GetType().GetProperty(item.PropertyName);
                            vph.ObjectName = item.PropertyName;
                            vph.ObjectDisplayName = item.Displayname;
                            if (item.TypeOfObject == typeof(DateTime))
                            {

                                DateTime a = (DateTime)Convert.ChangeType(prop.GetValue(input, null), typeof(DateTime))!;
                                vph.ObjectValue = a.ToString("yyyy/MM/dd");

                            }
                            else if (item.TypeOfObject == typeof(bool))
                            {
                                bool a = (bool)Convert.ChangeType(prop.GetValue(input, null), typeof(bool))!;

                                vph.ObjectValue = a ? commonResource.FirstOrDefault(x => x.Name == "YES").Value : commonResource.FirstOrDefault(x => x.Name == "NO").Value;
                            }
                            else
                            {

                                vph.ObjectValue = prop.GetValue(input, null)?.ToString();

                            }
                            temp.Add(vph);
                        }
                        catch (Exception e)
                        { 

                        }

                    }
                    ConvertedRecords.Add(temp);
                }
                return ConvertedRecords;
            }
            return null;
        }


        /// <summary>
        /// To find all properties from unknown input
        /// </summary>
        private void FindAllProperties()
        {
            PropertiesInformation = new List<PropertyInformation>();
            foreach (var item in typeof(TSelectItem).GetProperties())
            {

                var key = item.GetCustomAttribute<KeyAttribute>();
                var hiddenItem = item.GetCustomAttribute<HiddenAttribute>();
                var gridColumn = item.GetCustomAttribute<GridColumnAttribute>();
                Type type = item.PropertyType;
                if ((!(hiddenItem is null) || type == typeof(Guid)) && (key is null))
                {
                    continue;
                }
                var displayName = item.GetCustomAttribute<DisplayAttribute>()?.Name;

                PropertiesInformation.Add(
                    new PropertyInformation()
                    {
                        Displayname = displayName,
                        PropertyName = item.Name,
                        Key = key is not null,
                        TypeOfObject = type,
                        Width = gridColumn is not null ? gridColumn.Width : "5vw"

                    }

                    );

            }
        }






        #region ...Definition...

        private List<List<ValuePlaceHolder>> FilteredRecords { get; set; }
        private List<List<ValuePlaceHolder>> PagedRecords { get; set; }
        internal List<PropertyInformation> PropertiesInformation { get; set; }
        private int CurrentPage { get; set; } = 1;
        public int Count
        {
            get => _count;
            set
            {
                _count = value;
                CurrentPage = 1;
                PagedRecords = FilteredRecords.Take(_count).ToList();
            }
        }

        #endregion


        #region ...Parameter...
        [Parameter] public IStringLocalizer Resource { get; set; }
        [Parameter] public List<TSelectItem> Records { get; set; }
        [Parameter] public List<List<ValuePlaceHolder>> ConvertedRecords { get; set; }

        [Parameter] public string FormName { get; set; }
        [Parameter] public bool DeleteBtn { get; set; } = true;
        [Parameter] public bool EditBtn { get; set; } = true;



        [Parameter] public EventCallback<string> OnFinished { get; set; }
        [Parameter] public EventCallback<string> OnDeleteFinished { get; set; }

        [CascadingParameter] public IModalService Modal { get; set; }




        #endregion



        #region  ...Events...

        private async Task<bool> Confirm()
        {
            var parameters = new ModalParameters();
            parameters.Add("YES", basicResource["YES"].Value);
            parameters.Add("NO", basicResource["NO"].Value);
            var MessageForm = Modal.Show<ConfirmModal>(basicResource["WarnSignForConfirm"].Value, parameters);
            var result = await MessageForm.Result;
            return !result.Cancelled;
        }

        private async Task Delete(string Id)
        {
            var IsItReallyOkay = await Confirm();
            if (!IsItReallyOkay)
                return;
            await Http.DeleteAsync($"{ModelName}/Delete/{Id}");

            toastService.ShowError(basicResource["DoneDeleted"].Value, basicResource["DoneDeletedHeader"].Value);
            await OnDeleteFinished.InvokeAsync(Id);


        }
        public override async Task SetParametersAsync(ParameterView parameters)
        {
            await base.SetParametersAsync(parameters);

            ConvertedRecords = ConvertInputToListOfValuePlaceHolders();
            if (ConvertedRecords is null)
                return;
            FilteredRecords = ConvertedRecords;
            PagedRecords = FilteredRecords.Take(Count).ToList();
        }

        protected async override Task OnParametersSetAsync()
        {
            await base.OnParametersSetAsync();

            FilteredRecords = new List<List<ValuePlaceHolder>>();

            ConvertedRecords = (Records is null || Records == default) ? await Bind() : ConvertedRecords;
            if (ConvertedRecords is null)
                return;

            FilteredRecords = ConvertedRecords;

            PagedRecords = FilteredRecords.Take(Count).ToList();
            StateHasChanged();



        }





        async Task Edit(string id)
        {

            var parameters = new ModalParameters();
            parameters.Add("Key", id);
            parameters.Add("TItem", typeof(TItem));
            parameters.Add("OnFinished", OnFinished);
            parameters.Add("Resource", Resource);
            var MessageForm = Modal.Show<Form_Modal>(basicResource["Edit"], parameters);
            var result = await MessageForm.Result;

            await OnFinished.InvokeAsync(id)!;
        }



        void Sort(string propertyName, SortOrder order)
        {
            var property = typeof(TSelectItem).GetProperty(propertyName);
            switch (order)
            {
                case SortOrder.Asc when !(property is null):
                    FilteredRecords = FilteredRecords.OrderBy(x => x.Where(z => z.ObjectName == propertyName)).ToList();
                    break;
                case SortOrder.Desc when !(property is null):
                    FilteredRecords = FilteredRecords.OrderByDescending(x => property.GetValue(x, null)).ToList();
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(order), order, null);
            }
            PagedRecords = FilteredRecords.Take(Count).ToList();

        }

        void Search()
        {
            var containsAll = ConvertInputToListOfValuePlaceHolders();
            var containsSearch = false;
            var temp = new List<List<ValuePlaceHolder>>();

            foreach (var item in PropertiesInformation)
            {

                if (string.IsNullOrEmpty(item.Value) || item.Value == default || string.IsNullOrWhiteSpace(item.Value))
                    continue;
                containsSearch = true;
                foreach (var record in containsAll)
                {

                    if (record.Where(x => x.ObjectName == item.PropertyName && x.ObjectValue.Contains(item.Value)).Any())
                    {

                        temp.Add(record);
                    }
                }

            }

            if (containsSearch)
            {
                FilteredRecords = temp;

            }
            else
            {
                FilteredRecords = containsAll;

            }
            PagedRecords = FilteredRecords.Take(Count).ToList();

        }


        void ChangePage(int page)
        {
            CurrentPage = page;
            page--;

            PagedRecords = (page * Count < FilteredRecords.Count()) ? FilteredRecords.Skip(page * Count).Take(Count).ToList() : FilteredRecords.Skip((page - 1) * Count).Take(Count).ToList();

        }



        #endregion




        #region ...Enum...


        enum SortOrder
        {
            Asc, Desc
        }


        #endregion

    }
}
