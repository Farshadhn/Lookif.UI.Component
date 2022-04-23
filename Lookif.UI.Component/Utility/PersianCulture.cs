using System.Globalization;
using System.Threading;
using static System.Net.Mime.MediaTypeNames;

namespace Lookif.UI.Component.Utility
{
    public static class PersianCulture  
    {
        public static void  SetPersianCulture()
        {
            CultureInfo info = new CultureInfo("fa-Ir");
            info.DateTimeFormat.Calendar = new PersianCalendar();
            info.DateTimeFormat.MonthNames = new[] { "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند", "" };
            info.DateTimeFormat.MonthGenitiveNames = new[] { "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند", "" };
            info.DateTimeFormat.AbbreviatedMonthNames = new[] { "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند", "" };
            info.DateTimeFormat.AbbreviatedMonthGenitiveNames = new[] { "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند", "" };
            info.DateTimeFormat.AbbreviatedDayNames = new string[] { "ی", "د", "س", "چ", "پ", "ج", "ش" };
            info.DateTimeFormat.ShortestDayNames = new string[] { "ی", "د", "س", "چ", "پ", "ج", "ش" };
            info.DateTimeFormat.DayNames = new string[] { "یکشنبه", "دوشنبه", "ﺳﻪشنبه", "چهارشنبه", "پنجشنبه", "جمعه", "شنبه" };
            info.DateTimeFormat.AMDesignator = "ق.ظ";
            info.DateTimeFormat.PMDesignator = "ب.ظ";
            info.DateTimeFormat.ShortDatePattern = "yyyy/MM/dd";
            info.DateTimeFormat.ShortTimePattern = "HH:mm";
            info.DateTimeFormat.LongTimePattern = "HH:mm:ss";
            info.DateTimeFormat.FullDateTimePattern = "yyyy/MM/dd HH:mm:ss";
            CultureInfo.DefaultThreadCurrentCulture = info;
            CultureInfo.DefaultThreadCurrentUICulture = info;
            //Thread.CurrentThread.CurrentUICulture = info;
            
        }
        public static CultureInfo GetPersianCulture()
        {
            CultureInfo info = new CultureInfo("fa-IR");
            info.DateTimeFormat.Calendar = new PersianCalendar();
            info.DateTimeFormat.MonthNames = new[] { "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند", "" };
            info.DateTimeFormat.MonthGenitiveNames = new[] { "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند", "" };
            info.DateTimeFormat.AbbreviatedMonthNames = new[] { "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند", "" };
            info.DateTimeFormat.AbbreviatedMonthGenitiveNames = new[] { "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور", "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند", "" };
            info.DateTimeFormat.AbbreviatedDayNames = new string[] { "ی", "د", "س", "چ", "پ", "ج", "ش" };
            info.DateTimeFormat.ShortestDayNames = new string[] { "ی", "د", "س", "چ", "پ", "ج", "ش" };
            info.DateTimeFormat.DayNames = new string[] { "یکشنبه", "دوشنبه", "ﺳﻪشنبه", "چهارشنبه", "پنجشنبه", "جمعه", "شنبه" };
            info.DateTimeFormat.AMDesignator = "ق.ظ";
            info.DateTimeFormat.PMDesignator = "ب.ظ";
            info.DateTimeFormat.ShortDatePattern = "yyyy/MM/dd";
            info.DateTimeFormat.ShortTimePattern = "HH:mm";
            info.DateTimeFormat.LongTimePattern = "HH:mm:ss";
            info.DateTimeFormat.FullDateTimePattern = "yyyy/MM/dd HH:mm:ss";
            return info;

        }
    }
}
