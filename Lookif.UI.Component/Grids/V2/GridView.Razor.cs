﻿using Blazored.Modal;
using Blazored.Modal.Services;
using Blazored.Toast.Services;
using Lookif.UI.Common.Models;
using Lookif.UI.Component.Attributes;
using Lookif.UI.Component.Components.SeparatedComponents;
using Lookif.UI.Component.Modals;
using Lookif.UI.Component.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using static Newtonsoft.Json.JsonConvert;

namespace Lookif.UI.Component.Grids.V2;

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
        var dataObjResponse = await Http.GetAsync($"{ModelName}/Get");
        var dataObjStr = await dataObjResponse.Content.ReadAsStringAsync();
        if (!dataObjResponse.IsSuccessStatusCode)
            throw new Exception(dataObjStr);

        var dataObj = DeserializeObject<ApiResult<List<TSelectItem>>>(dataObjStr);

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
                        var inputType = prop.GetCustomAttribute<InputTypeAttribute>();

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
                            var displayType = inputType is null ? DisplayType.Text : inputType.InputTypeEnum switch { InputTypeEnum.Text => DisplayType.Text, InputTypeEnum.Color => DisplayType.Color, _ => DisplayType.Text };
                            vph.ObjectDisplayType = displayType;
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

            if (item.Name == "Id")
            {
                var displayNameId = item.GetCustomAttribute<DisplayAttribute>()?.Name;

                PropertiesInformation.Add(
                    new PropertyInformation()
                    {
                        Displayname = displayNameId,
                        PropertyName = item.Name,
                        Key = key is not null,
                        TypeOfObject = type,
                        Width = gridColumn is not null ? gridColumn.Width : "5vw"

                    }

                    );
            }


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

    private List<List<ValuePlaceHolder>> FilteredRecords { get; set; } = [];
    private List<List<ValuePlaceHolder>> PagedRecords { get; set; } = [];
    internal List<PropertyInformation> PropertiesInformation { get; set; } = [];
    private int CurrentPage { get; set; } = 1;
    private string CurrentSortProperty { get; set; }
    private SortOrder CurrentSortOrder { get; set; } = SortOrder.Asc;

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
        var options = new ModalOptions()
        {
            Class = "size-automatic",
            HideHeader = true,
            Position = ModalPosition.Middle,
            AnimationType = ModalAnimationType.FadeInOut
        };
        var parameters = new ModalParameters();
        parameters.Add("ConfirmText", basicResource["YES"].Value);
        parameters.Add("CancelText", basicResource["NO"].Value);
        parameters.Add("Message", "آیا از حذف این مورد اطمینان دارید؟");
        var MessageForm = Modal.Show<ConfirmModal>(basicResource["WarnSignForConfirm"].Value, parameters, options);
        var result = await MessageForm.Result;
        return !result.Cancelled;
    }
    private async Task Delete(string Id)
    {
        var IsItReallyOkay = await Confirm();
        if (!IsItReallyOkay)
            return;
        await Http.DeleteAsync($"{ModelName}/Delete/{Id}");

        toastService.ShowError(basicResource["DoneDeleted"].Value);
        await OnDeleteFinished.InvokeAsync(Id);


    }
    public override async Task SetParametersAsync(ParameterView parameters)
    {


        ConvertedRecords = ConvertInputToListOfValuePlaceHolders();
        if (ConvertedRecords is null)
        {
            await base.SetParametersAsync(parameters);
            return;
        }

        FilteredRecords = ConvertedRecords;
        PagedRecords = FilteredRecords.Take(Count).ToList();
        await base.SetParametersAsync(parameters);


    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            FilteredRecords = new List<List<ValuePlaceHolder>>();

            ConvertedRecords = (Records is null || Records == default || !Records.Any()) ? await Bind() : ConvertedRecords;
            if (ConvertedRecords is null)
                return;

            FilteredRecords = ConvertedRecords;

            PagedRecords = FilteredRecords.Take(Count).ToList();
            StateHasChanged();
        }
    }




    protected async override Task OnParametersSetAsync()
    {
        await base.OnParametersSetAsync();




    }

    async Task Edit(string id)
    {
        var options = new ModalOptions()
        {
            Class = "size-automatic",
            HideHeader = true,
            Position = ModalPosition.Middle,
            AnimationType = ModalAnimationType.FadeInOut
        };
        var parameters = new ModalParameters();
        parameters.Add("Key", id);
        parameters.Add("TItem", typeof(TItem));
        parameters.Add("OnFinished", OnFinished);
        parameters.Add("Resource", Resource);
        parameters.Add("Title", "ویرایش");
        var MessageForm = Modal.Show<Form_Modal>("", parameters, options);
        var result = await MessageForm.Result;

        await OnFinished.InvokeAsync(id)!;
    }



    private string GetSortIconClass(string propertyName)
    {
        if (CurrentSortProperty != propertyName)
            return "oi-chevron-top";
            
        return CurrentSortOrder == SortOrder.Asc ? "oi-chevron-top" : "oi-chevron-bottom";
    }

    private string GetSortIconActiveClass(string propertyName)
    {
        return CurrentSortProperty == propertyName ? "active" : "";
    }

    private void ToggleSort(string propertyName)
    {
        if (CurrentSortProperty == propertyName)
        {
            // Toggle the sort order
            CurrentSortOrder = CurrentSortOrder == SortOrder.Asc ? SortOrder.Desc : SortOrder.Asc;
        }
        else
        {
            // New property, start with ascending
            CurrentSortProperty = propertyName;
            CurrentSortOrder = SortOrder.Asc;
        }

        Sort(propertyName, CurrentSortOrder);
    }

    void Sort(string propertyName, SortOrder order)
    {
        if (FilteredRecords == null || !FilteredRecords.Any())
            return;

        var property = typeof(TSelectItem).GetProperty(propertyName);
        if (property == null)
            return;

        FilteredRecords = order switch
        {
            SortOrder.Asc => FilteredRecords.OrderBy(x => 
            {
                var value = x.FirstOrDefault(v => v.ObjectName == propertyName)?.ObjectValue;
                return ConvertValueForSorting(value, property.PropertyType);
            }).ToList(),
            SortOrder.Desc => FilteredRecords.OrderByDescending(x => 
            {
                var value = x.FirstOrDefault(v => v.ObjectName == propertyName)?.ObjectValue;
                return ConvertValueForSorting(value, property.PropertyType);
            }).ToList(),
            _ => throw new ArgumentOutOfRangeException(nameof(order), order, null)
        };

        PagedRecords = FilteredRecords.Skip((CurrentPage - 1) * Count).Take(Count).ToList();
    }

    private object ConvertValueForSorting(string value, Type targetType)
    {
        if (string.IsNullOrEmpty(value))
            return null;

        try
        {
            if (targetType == typeof(DateTime))
            {
                return DateTime.TryParse(value, out DateTime date) ? date : DateTime.MinValue;
            }
            if (targetType == typeof(int))
            {
                return int.TryParse(value, out int result) ? result : 0;
            }
            if (targetType == typeof(double))
            {
                return double.TryParse(value, out double result) ? result : 0.0;
            }
            if (targetType == typeof(decimal))
            {
                return decimal.TryParse(value, out decimal result) ? result : 0m;
            }
            if (targetType == typeof(bool))
            {
                return bool.TryParse(value, out bool result) ? result : false;
            }
            if (targetType == typeof(Guid))
            {
                return Guid.TryParse(value, out Guid result) ? result : Guid.Empty;
            }
            // For string and other types, return the value as is
            return value;
        }
        catch
        {
            return value; // Fallback to string comparison if conversion fails
        }
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
