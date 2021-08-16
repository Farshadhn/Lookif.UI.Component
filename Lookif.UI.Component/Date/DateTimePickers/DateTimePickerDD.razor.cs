﻿ 
using Microsoft.AspNetCore.Components;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using static System.Globalization.CultureInfo;
namespace Lookif.UI.Component.Date.DateTimePickers
{
    public partial class DateTimePickerDD
    {

        private IEnumerable<(int RowNumber, string Name)> MonthNames = DateTimeFormatInfo.CurrentInfo.MonthNames.Select((value, i) => (i, value));

        private int Month => CurrentCulture.Calendar.GetMonth(Now);
        private int Day { get; set; }
        private int Year => CurrentCulture.Calendar.GetYear(Now);




        [Parameter]
        public DateTime Value { get; set; }




        private int selectedYear = CurrentUICulture.Calendar.GetYear(Now);
        private int selectedMonth = CurrentUICulture.Calendar.GetMonth(Now);
        private int selectedDay = CurrentUICulture.Calendar.GetDayOfMonth(Now);

        private int SelectedDay
        {
            get => selectedDay;
            set
            {
                if (value == selectedDay)
                    return;
                selectedDay = value;
                ValueChanged.InvokeAsync(Latest);
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

        private DateTime Latest => DateTime.Parse($"{selectedYear}-{selectedMonth}-{selectedDay}");


        protected override Task OnParametersSetAsync()
        {
            if (Value != default)
            {
                
                var Input = new DateTime(Value.Year, Value.Month, Value.Day);

                var d = CurrentUICulture.Calendar.GetDayOfMonth(Input);
                var m = CurrentUICulture.Calendar.GetMonth(Input);
                var y = CurrentUICulture.Calendar.GetYear(Input);

                if (SelectedDay != d)
                    SelectedDay = d;

                if (SelectedMonth != m)
                    SelectedMonth = m;

                if (SelectedYear != y)
                    SelectedYear = y;
            }

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
                 
                SelectedYear = CurrentUICulture.Calendar.GetYear(Now);
                SelectedMonth = CurrentUICulture.Calendar.GetMonth(Now);
                SelectedDay = CurrentUICulture.Calendar.GetDayOfMonth(Now);
            }

            return base.OnAfterRenderAsync(firstRender);
        }
    }
}
