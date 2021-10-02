using Blazored.Toast.Services;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using static Newtonsoft.Json.JsonConvert;


namespace Lookif.UI.Component.DropDown
{
    public partial class DropDownSelective<T>
    {
        private T returnValue;

        [Inject] IToastService toastService { get; set; }

        #region ...Function...

        private void Bind()
        {
            SanitizedRecords = new Dictionary<string, string>();

            foreach (var Record in Records)
            {

                var property = Record.GetType().GetProperty(Key);
                var DropDownKey = property.GetValue(Record, null).ToString();


                property = Record.GetType().GetProperty(Value);
                var DropDownValue = property.GetValue(Record, null).ToString();

                SanitizedRecords.Add(DropDownKey, DropDownValue);
            }
        }


        #endregion

        #region ...Event...




        protected override async Task OnParametersSetAsync()
        {

            Bind();

            if (SelectedOption is null)
                return;


            //SetIdFromName(SelectedOption);
            await base.OnParametersSetAsync();

        }
        public async Task myrecordsChange(ChangeEventArgs changeEventArgs)
        {
            // var SelectedValue = (T)Convert.ChangeType(changeEventArgs.Value, typeof(T));
            var SelectedValue = changeEventArgs.Value.ToString();

            var (obj, IsItChanged) = SetIdFromName(SelectedValue);
            if (string.IsNullOrEmpty(obj.key) && string.IsNullOrEmpty(obj.value))
            {

                changeEventArgs.Value = " ";
                Selected = " ";
                toastService.ShowError("لطفا مقدار صحیح را از داخل لیست انتخاب فرمایید.!", "خطا");
                return;

            }
            if (IsItChanged)
            {
                Selected = obj.key;
                var finalRes = (T)TypeDescriptor.GetConverter(typeof(T)).ConvertFromInvariantString(obj.value);
                await ReturnValueChanged.InvokeAsync(finalRes);
            }

        }


        private ((string key, string value), bool IsItChanged) SetIdFromName(string key)
        {
            var res = new KeyValuePair<string, string>();
            if (key is not string)
                res = SanitizedRecords.FirstOrDefault(x => x.Value.Trim() == key.Trim());
            else
                res = SanitizedRecords.FirstOrDefault(x => x.Key.Trim() == key.Trim());

            return ((res.Value is not null) ? ((res.Key, res.Value), true) : (default, false));

        }
        #endregion

        #region ...Definition...

        private Dictionary<string, string> SanitizedRecords { get; set; } //At first we fetch all data after that we create Dictionary of all name and Ids
        private Dictionary<string, string> FilteredRecords { get; set; }
        public int Count { get; set; } = 10;
        public bool Show { get; set; } = false;
        public string Text { get; set; } = "";
        public string Selected { get; set; } = "";


        #endregion

        #region ...Parameter...

        [Parameter] public IReadOnlyList<object> Records { get; set; }

        [Parameter] public string FormName { get; set; }
        [Parameter] public string Key { get; set; }
        [Parameter] public string Value { get; set; }

        [Parameter]
        public T SelectedOption { get; set; }


        [Parameter]
        public T ReturnValue
        {
            get => returnValue; set
            {
                if (!value.Equals(returnValue))
                    returnValue = value;
            }
        }
        [Parameter]
        public EventCallback<T> ReturnValueChanged { get; set; }

        #endregion
    }
}
