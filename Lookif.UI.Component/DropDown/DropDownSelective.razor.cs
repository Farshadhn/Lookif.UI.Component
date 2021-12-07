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


namespace Lookif.UI.Component.DropDown
{
    public partial class DropDownSelective<T>
    {
        private List<T> returnValue;

        [Inject] IToastService toastService { get; set; }
        [Inject] IJSRuntime js { get; set; }

        bool firstRender = true;


        #region ...Function...

        private void Bind()
        {
            firstRender = false;
            SanitizedRecords = new();
            Console.WriteLine("SerializeObject(Records)");
            Console.WriteLine(SerializeObject(Records));
            if (!Records.Any())
                return;
            foreach (var Record in Records)
            {

                var property = Record.GetType().GetProperty(Key);
                var targetObject = property.GetValue(Record, null);
                if (targetObject is null)
                    continue;
                var DropDownValue = targetObject.ToString();

                property = Record.GetType().GetProperty(Value);
                var DropDownKey = (T)property.GetValue(Record, null);
                if (!SanitizedRecords.Any(x => x.Key.Equals(DropDownKey)))
                    SanitizedRecords.Add(new DropdownContextHolder<T>(DropDownValue, DropDownKey));
            }


            if (SelectedOption is not null && !SelectedOption.Equals(default(List<T>)))
            {
                if (Multiple)
                {
                    Console.WriteLine("Multiple");
                    Console.WriteLine(SerializeObject(SelectedOption)); 
                    SelectedOption.AsParallel().ForAll(x=>SetIdFromKey(x,false));
                    Console.WriteLine("After  Multiple");
                    Console.WriteLine(SerializeObject(SanitizedRecords));
                   
                }
                else
                {
                    Console.WriteLine("SerializeObject(SelectedOption)");
                    Console.WriteLine(SerializeObject(SelectedOption));
                    SetIdFromKey(SelectedOption.FirstOrDefault(),false);
                    Selected = SanitizedRecords.FirstOrDefault(x => x.Status).Content;
                }
                
            }

        }


        #endregion

        #region ...Event...


        protected override void OnParametersSet()
        {
            if (firstRender)
            {
                Bind();
            }


        }


        /// <summary>
        /// When you select sth or type sth. we get this event
        /// </summary>
        /// <param name="changeEventArgs"></param>
        /// <returns></returns>
        public async Task myrecordsChange(ChangeEventArgs changeEventArgs)
        {
            var SelectedValue = changeEventArgs.Value.ToString();
            try
            {
                SetIdFromName(SelectedValue);
                var res = new List<T>
                {
                    SanitizedRecords.FirstOrDefault(x => x.Status).Key
                };
                Console.WriteLine(SerializeObject(SelectedValue));
                Console.WriteLine(SerializeObject(res));
                await ReturnValueChanged.InvokeAsync(res);
            }
            catch (Exception)
            {
                await ReturnValueChanged.InvokeAsync(default);
            }





        }

        private async Task ChangeList(DropdownContextHolder<T> record)
        {
            await Task.Delay(10);
            SetIdFromKey(record.Key,false);
            await ReturnValueChanged.InvokeAsync(SanitizedRecords.Where(x => x.Status).Select(x => x.Key).ToList());

        }

        private void SetIdFromName(string Content, bool reset = true)
        {

            var res = new DropdownContextHolder<T>();
            if (reset)
                SanitizedRecords.AsParallel().ForAll(x => x.Status = false);
            res =  SanitizedRecords.FirstOrDefault(x => x.Content.Trim() == Content.Trim());
            if (res is null)
                throw new Exception($@"{Content} not found");
            res.Status = !res.Status;

        }
        private void SetIdFromKey(T key, bool reset = true)
        {

            var res = new DropdownContextHolder<T>();
            if (reset)
                SanitizedRecords.AsParallel().ForAll(x => x.Status = false); 
            res =  SanitizedRecords.FirstOrDefault(x => x.Key.ToString() == key.ToString());
             
            if (res is null)
                throw new Exception($@"{key} not found");
            res.Status = !res.Status;

        }
        #endregion

        #region ...Definition...

        private List<DropdownContextHolder<T>> SanitizedRecords { get; set; } //At first we fetch all data after that we create Dictionary of all name and Ids
        private List<DropdownContextHolder<T>> FilteredRecords { get; set; }
        public int Count { get; set; } = 10;
        public bool Show { get; set; } = false;
        public string Text { get; set; } = "";
        public string Selected { get; set; } = ""; //Content


        #endregion

        #region ...Parameter...

        [Parameter] public IReadOnlyList<object> Records { get; set; }

        [Parameter] public string FormName { get; set; }
        [Parameter] public string Key { get; set; }
        [Parameter] public string Value { get; set; }

        [Parameter]
        public List<T> SelectedOption { get; set; }
        [Parameter]
        public bool Multiple { get; set; }

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
            Content=content;
            Key=key;
            Status=status;
        }

        public DropdownContextHolder()
        {

        }
        public string Content { get; init; }
        public T Key { get; init; }
        public bool Status { get; set; }


    }
}
