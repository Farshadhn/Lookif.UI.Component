using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;



namespace Lookif.UI.Component.DropDown
{
    public partial class DropDownSelective
    {
        private string returnValue;



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

            ////Console.WriteLine($"SanitizedRecords=>{SerializeObject(SanitizedRecords)}");
        }


        #endregion

        #region ...Event...




        protected override async Task OnParametersSetAsync()
        {

            Bind();
            ////Console.WriteLine("OnParametersSetAsync1");
            if (SelectedOption is null)
                return;


            ////Console.WriteLine("OnParametersSetAsync2");
            ////Console.WriteLine(SerializeObject(SelectedOption));

            SetIdFromName(SelectedOption);
            await base.OnParametersSetAsync();

        }
        public async Task myrecordsChange(ChangeEventArgs changeEventArgs)
        {
            var SelectedValue = changeEventArgs.Value.ToString();
            var str = SetIdFromName(SelectedValue);
            if (str != "")
                await ReturnValueChanged.InvokeAsync(str);
        }


        private string SetIdFromName(string key)
        {
            ////Console.WriteLine("SetIdFromName");
            var res = new KeyValuePair<string, string>();
            if (Guid.TryParse(key, out _))
                res = SanitizedRecords.FirstOrDefault(x => x.Value.Trim() == key.Trim());
            else
                res = SanitizedRecords.FirstOrDefault(x => x.Key.Trim() == key.Trim());
            ////Console.WriteLine(SerializeObject(res));
            if (res.Value is not null)
            {
                ////Console.WriteLine("Areeeeeeeeeeeeeeeeee");
                Selected = res.Key;
                return res.Value;
            }
            return "";
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
        public string SelectedOption { get; set; }


        [Parameter]
        public string ReturnValue
        {
            get => returnValue; set
            {
                if (value != returnValue)
                    returnValue = value;
            }
        }
        [Parameter]
        public EventCallback<string> ReturnValueChanged { get; set; }

        #endregion
    }
}
