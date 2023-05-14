using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using static System.Globalization.CultureInfo;

namespace Lookif.UI.Component.Date.DateTimePickers
{
    public partial class DateTimePickerDDGregorian
    {

        private IEnumerable<(int RowNumber, string Name)> MonthNames = DateTimeFormatInfo.InvariantInfo.MonthNames.Select((value, i) => (i, value));

        private int Month => DateTime.Now.Year;
        private int Day => DateTime.Now.Day;
        private int Year => DateTime.Now.Month;





        [Parameter]
        public DateTime Value { get; set; }
        [Parameter]
        // public bool CultureType { get; set; } = true;
        public int YearValue { get; set; }
        public int DaysValue { get; set; }








        private int selectedYear = DateTime.Now.Year;
        private int selectedMonth = DateTime.Now.Month;
        private int selectedDay = DateTime.Now.Day;

        private int SelectedDay
        {
            get => selectedDay;
            set
            {
                Console.WriteLine(value);
                Console.WriteLine(SelectedDay);

                if (value == selectedDay)
                    return;
                selectedDay = value;
                ValueChanged.InvokeAsync(Latest);
                Console.WriteLine(SelectedDay);
            }

        }
        private int SelectedMonth
        {
            get => selectedMonth;
            set
            {
                selectedMonth = value;
                ValueChanged.InvokeAsync(Latest);
            }
        }


        private int SelectedYear
        {
            get => selectedYear;
            set
            {
                selectedYear = value;
                ValueChanged.InvokeAsync(Latest);
            }
        }
        
        private DateTime Latest => (DateTime.TryParse($"{selectedYear}-{selectedMonth}-{selectedDay}", new CultureInfo("en-US") , new DateTimeStyles() , out DateTime d)) ?
                      d : DateTime.Parse($"{selectedYear}-{selectedMonth}-1");

        


        protected override void OnAfterRender(bool firstRender)
        {
         
            if (!firstRender)
                return;

            DaysValue = DateTime.DaysInMonth(SelectedYear, SelectedMonth);
            YearValue = DateTime.Now.Year;

            Value = DateTime.ParseExact($"{selectedYear}-{selectedMonth}-{selectedDay}", "yyyy-M-d", new CultureInfo("en-US"));
           
            Console.WriteLine(Value);
            Console.WriteLine(Value.Year);
            Console.WriteLine(Value.Month);
            Console.WriteLine(Value.Day);
   
          
            Calendar myCal = CultureInfo.InvariantCulture.Calendar;



            if (Value != default)
            {
                var Input = new DateTime(Value.Year, Value.Month, Value.Day);

                var d = SelectedDay;
                var m = SelectedMonth;
                var y = SelectedYear;
                //var d = myCal.GetDayOfMonth(Input);
                //var m = myCal.GetMonth(Input);
                //var y = myCal.GetYear(Input);

                if (SelectedDay != d)
                    SelectedDay = d;

                if (SelectedMonth != m)
                    SelectedMonth = m;

                if (SelectedYear != y)
                    SelectedYear = y;
            }
        }


        protected override Task OnParametersSetAsync()
        {
            DaysValue = DateTime.DaysInMonth(SelectedYear, SelectedMonth);
            YearValue = DateTime.Now.Year;
            return base.OnParametersSetAsync();
        }

            [Parameter]
        public EventCallback<DateTime> ValueChanged { get; set; }

        private static DateTime Now => DateTime.Now;


        protected override Task OnInitializedAsync()
        {

            ValueChanged.InvokeAsync(Latest);
            return base.OnInitializedAsync();
        }

        protected override Task OnAfterRenderAsync(bool firstRender)
        {

            if (Value == default)
            {

                SelectedYear = DateTime.Now.Year;
                SelectedMonth = DateTime.Now.Month;
                SelectedDay = DateTime.Now.Day;
            }

            return base.OnAfterRenderAsync(firstRender);
        }
    }
}
