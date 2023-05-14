using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using static Newtonsoft.Json.JsonConvert;


namespace Lookif.UI.Component.DropDown;

public partial class DropDownSelective<T>
{
    [Parameter(CaptureUnmatchedValues = true)]
    public IReadOnlyDictionary<string, object> AdditionalAttributes { get; set; }

    [Parameter] public EventCallback<T> OnChange { get; set; }


    private List<T> returnValue;

    [Inject] IToastService toastService { get; set; }
    [Inject] IJSRuntime js { get; set; }

    bool firstRender = true;

    #region ...Function...

    public async Task Clear()
    {

        resetAll();
        Selected = null;
        await ReturnValueChanged.InvokeAsync(default);
    }

    public void ReBind(IReadOnlyCollection<object> newValue)
    {
        if (newValue is null)
            throw new ArgumentNullException(nameof(newValue));

        ConvertToDropdownContextHolder(newValue);
    }

    public void ReSelect(List<T> seletedOptions)
    {
        if (seletedOptions is not { Count: < 1 })
            ShowAlreadySelectedOptions(seletedOptions);
    }


    private void ConvertToDropdownContextHolder(IReadOnlyCollection<object> newValue)
    {
        SanitizedRecords = new(); 
        foreach (var Record in newValue)
        {
            var property = Record.GetType().GetProperty(Value);

            var targetObject = property.GetValue(Record, null);
            if (targetObject is null)
                continue;
            var DropDownValue = targetObject.ToString();

            property = Record.GetType().GetProperty(Key);
            var DropDownKey = (T)property.GetValue(Record, null);
            if (!SanitizedRecords.Any(x => x.Key.Equals(DropDownKey)))
                SanitizedRecords.Add(new DropdownContextHolder<T>(DropDownValue, DropDownKey));
        }
        
    }

    private void Bind()
    {
        firstRender = false;

        if (Records is null or { Count: < 1 })
            return;


        ConvertToDropdownContextHolder(Records);
        ShowAlreadySelectedOptions(SelectedOption);

    }

    private void ShowAlreadySelectedOptions(List<T> seletedOptions)
    {
        bool DoesItHaveSelectedValue = seletedOptions is not null && !seletedOptions.Equals(default(List<T>)); 
        if (!DoesItHaveSelectedValue)
            return; 
        if (Multiple)
            DefineInMultipleChoice(seletedOptions);
        else
            DefineInSingleChoice(seletedOptions);


    }

    private void DefineInSingleChoice(List<T> seletedOptions)
    {
        SetIdFromKey(seletedOptions.FirstOrDefault(), false);
        Selected = SanitizedRecords?.FirstOrDefault(x => x.Status)?.Content;
    }

    private void DefineInMultipleChoice(List<T> seletedOptions)
    {
        for (int i = 0; i < seletedOptions.Count; i++)
        {
            SetIdFromKey(seletedOptions[i], false);
           
        } 
    }

    private void resetAll()
    {
        for (int i = 0; i < SanitizedRecords.Count; i++)
        {
            SanitizedRecords[i].Status = false;

        } 
    }

    #endregion

    #region ...Event...


    protected override void OnParametersSet()
    {
        if (firstRender)
            Bind();



    }


    /// <summary>
    /// When you select sth or type sth. we get this event
    /// </summary>
    /// <param name="changeEventArgs"></param>
    /// <returns></returns>
    public async Task myrecordsChange(ChangeEventArgs changeEventArgs)
    {
        try
        {
            var SelectedValue = changeEventArgs.Value.ToString();
            Selected = SelectedValue;
            SetIdFromName(SelectedValue);
            var res = new List<T>
            {
                SanitizedRecords.FirstOrDefault(x => x.Status).Key
            };

            if (res.FirstOrDefault() is null)
                await OnChange.InvokeAsync(default);
            else
                await OnChange.InvokeAsync(res.FirstOrDefault());

            await ReturnValueChanged.InvokeAsync(res);
        }
        catch (Exception)
        {
            Selected = "";
            await OnChange.InvokeAsync(default);
            await ReturnValueChanged.InvokeAsync(default);
        }





    }

    private async Task ChangeList(DropdownContextHolder<T> record)
    {
        await Task.Delay(10);
        SetIdFromKey(record.Key, false);
        await ReturnValueChanged.InvokeAsync(SanitizedRecords.Where(x => x.Status).Select(x => x.Key).ToList());

    }

    private void SetIdFromName(string Content, bool reset = true)
    {

        var res = new DropdownContextHolder<T>();
        if (reset)
            resetAll();
        res = SanitizedRecords.FirstOrDefault(x => x.Content.Trim() == Content.Trim());
        if (res is null)
            throw new Exception($@"{Content} not found");
        res.Status = !res.Status;

    }
    private void SetIdFromKey(T key, bool reset = true)
    {
        if (key is null)
            return;
        if (key.Equals(default(T)))
            return; 
        var toCompare = (key.GetType().IsEnum) ? SerializeObject(key) : key.ToString();
         
        var res = new DropdownContextHolder<T>();
        if (reset)
        {
            resetAll();
        } 
        res = SanitizedRecords.FirstOrDefault(x =>  x.Key.ToString() == toCompare);

        if (res is null)
            throw new Exception($@"{key} not found");
        res.Status = !res.Status;

    }
    #endregion

    #region ...Definition...

    private List<DropdownContextHolder<T>> SanitizedRecords { get; set; } = new(); //At first we fetch all data after that we create Dictionary of all name and Ids


    public bool Show { get; set; } = false;
    public string Text { get; set; } = "";
    public string Selected { get; set; } = ""; //Content


    #endregion

    #region ...Parameter...

    [Parameter] public IReadOnlyCollection<object> Records { get; set; }

    [Parameter] public string FormName { get; set; }
    [Parameter] public string Key { get; set; }
    [Parameter] public string Value { get; set; }

    [Parameter]
    public List<T> SelectedOption { get; set; }
    [Parameter]
    public bool Multiple { get; set; }
    [Parameter]
    public bool Simple { get; set; } = true;

    [Parameter]
    public List<T> ReturnValue
    {
        get => returnValue; set
        {
            if (value is not null && !value.Equals(returnValue))
                returnValue = value;
        }
    }
    [Parameter]
    public EventCallback<List<T>> ReturnValueChanged { get; set; }



    #endregion
}

internal class DropdownContextHolder<T>
{

    public DropdownContextHolder(string content, T key, bool status = false)
    {
        Content = content;
        Key = key;
        Status = status;
    }

    public DropdownContextHolder()
    {

    }
    public string Content { get; init; }
    public T Key { get; init; }
    public bool Status { get; set; }


}
