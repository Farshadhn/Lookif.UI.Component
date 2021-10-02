﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Json;
using System.Reflection;
using System.Threading.Tasks;
using Blazored.Toast.Services;
using Lookif.UI.Component.Attributes;
using Lookif.UI.Component.Models;
using Microsoft.AspNetCore.Components;
using Lookif.UI.Common.Models;
using static Newtonsoft.Json.JsonConvert;
using Microsoft.Extensions.Localization;
using System.Globalization;
using System.ComponentModel;

namespace Lookif.UI.Component.Components.SeparatedComponents.SimpleForm
{
    public static class TypeExtensions
    {
        public static bool IsNullableType(this Type type)
        {
            return type.IsGenericType
                   && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>));
        }
    }
    public partial class Form

    {


        #region ...DI...


        [Inject] HttpClient Http { get; set; }
        [Inject] IToastService toastService { get; set; }

        #endregion



        #region ...Events...


        protected override async Task OnInitializedAsync()
        {
            await base.OnInitializedAsync();
            await Init();
            if (Key != default)
                await Edit(Key);
        }


        /// <summary>
        /// Add Or Create New
        /// </summary>
        /// <returns></returns>
        private async Task Add()
        {
            object insertOrUpdate = Activator.CreateInstance(Dto);

            foreach (var item in ItemsOfClasses)
            {
                var prop = insertOrUpdate.GetType().GetProperty(item.Name, BindingFlags.Public | BindingFlags.Instance);
                if (null == prop || !prop.CanWrite) continue;
               
                
                
                
                var targetType = prop.PropertyType.IsNullableType()
                                  ? Nullable.GetUnderlyingType(prop.PropertyType)
                                  : prop.PropertyType;
                if (targetType == typeof(DateTime))
                {
                    prop.SetValue(insertOrUpdate, item.DateTime, null);
                    continue;
                }// ToDo Make it like the others


                if (String.IsNullOrEmpty(item?.Value)) continue;




                try
                {
                    var convertedValue = TypeDescriptor.GetConverter(targetType).ConvertFromInvariantString(item?.Value);
                    prop.SetValue(insertOrUpdate, convertedValue, null);
                }
                catch (Exception ex)
                {
                    var rs = relatedSource.Find(x => x.Name == prop.Name);
                    toastService.ShowError($"لطفا در  '{rs}'، مقدار صحیح را وارد نمایید", "خطا");
                    return;
                }










            }

            var returnStr = String.Empty;

            if (Key == default)
            {

                var responseMessage = await Http.PostAsJsonAsync($"{ModelName}/create", insertOrUpdate);
                var res = DeserializeObject<ApiResult<object>>(await responseMessage.Content.ReadAsStringAsync());
                if (!res.IsSuccess)
                {
                    toastService.ShowError(res.Message, "خطا");
                    return;
                }

                var efIdProp = res.Data.GetType().GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
                returnStr = efIdProp?.GetValue(res.Data)?.ToString();
            }
            else
            {

                var responseMessage = await Http.PutAsJsonAsync($"{ModelName}/update/{Key}", insertOrUpdate);

            }
            await Clear();

            await OnFinished.InvokeAsync(returnStr);
            toastService.ShowSuccess("با موفقیت انجام شد", "موفقیت");

        }

        #endregion



        #region ... Definition...

        List<LocalizedString> relatedSource { get; set; }
        private string ModelName => Dto.Name.Replace("Dto", "");

        public IList<ItemsOfClass> ItemsOfClasses { get; set; } = new List<ItemsOfClass>();




        #endregion


        #region ... Functions...




        private async Task Edit(string id)
        {
            var dataObj = await Http.GetAsync($"{ModelName}/Get/{id}");
            var response = await dataObj.Content.ReadAsStringAsync();

            var generic = typeof(ApiResult<>);
            Type constructed = generic.MakeGenericType(Dto);
            var res = DeserializeObject(response!, constructed);
            PropertyInfo prosp = res.GetType().GetProperty("Data", BindingFlags.Public | BindingFlags.Instance);

            var data = prosp.GetValue(res, null)!;




            foreach (var item in ItemsOfClasses)
            {
                PropertyInfo prop = data.GetType().GetProperty(item.Name, BindingFlags.Public | BindingFlags.Instance);
                if (prop?.PropertyType == typeof(DateTime))
                    item!.DateTime = (DateTime)prop.GetValue(data, null)!;
                else if (prop?.PropertyType == typeof(Boolean))
                    item!.Valuebool = (bool)prop.GetValue(data, null)!;
                else
                    item!.Value = prop?.GetValue(data, null)?.ToString();

            }


        }


        /// <summary>
        /// This function is for storing data in drop down
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <param name="tableNameToBeRetrieved"></param>
        /// <returns></returns>
        private async Task<List<RelatedTo>> FillDropDown(PropertyInfo propertyInfo, string tableNameToBeRetrieved)
        {
            var displayNameForDropDown = propertyInfo.GetCustomAttribute<RelatedToAttribute>()?.DisplayName;

            var listOfRelatedTo = await GetRelatedTo(tableNameToBeRetrieved, displayNameForDropDown);

            return listOfRelatedTo;

        }

        /// <summary>
        /// Get Display name of each enum properties
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <param name="displayName"></param>
        /// <returns></returns>
        private List<RelatedTo> FillEnum(PropertyInfo propertyInfo, string displayName)
        {

            List<RelatedTo> listOfRelatedTo = new();

            System.Type enumType = propertyInfo.PropertyType;
            System.Type enumUnderlyingType = System.Enum.GetUnderlyingType(enumType);
            System.Array enumValues = System.Enum.GetValues(enumType);

            for (int i = 0; i < enumValues.Length; i++)
            {

                // Retrieve the value of the ith enum item.
                object value = enumValues.GetValue(i);
                var attribute = value.GetType().GetField(value.ToString())
                    .GetCustomAttributes<DisplayAttribute>(false).FirstOrDefault();

                var propValue = attribute?.GetType().GetProperty("Name")?.GetValue(attribute, null);
                // Convert the value to its underlying type (int, byte, long, ...)
                object underlyingValue = System.Convert.ChangeType(value, enumUnderlyingType);

                listOfRelatedTo.Insert(0, new RelatedTo() { Id = underlyingValue.ToString(), Name = propValue.ToString() });
            }

            return listOfRelatedTo;
        }






        private async Task Init()
        {
            ItemsOfClasses = new List<ItemsOfClass>();
            PropertyInfo[] propertyInfos = Dto.GetProperties();
            foreach (var property in propertyInfos)
            {
                var hiddenDto = property.GetCustomAttribute<HiddenDtoAttribute>();

                if (!(hiddenDto is null)) // What we don't want to include in our form
                    continue;
                var key = property.GetCustomAttribute<KeyAttribute>();
                if (!(key is null) || property.Name == "Id") continue;


                var displayName = property.GetCustomAttribute<DisplayAttribute>()?.Name;//Name that should be placed as its label

                var relatedTo = property.GetCustomAttribute<RelatedToAttribute>()?.Name;// To check if we need to implement this field as a Dropdown or not
                var order = property.GetCustomAttribute<OrderAttribute>()?.Order ?? 100;// To check if we need to implement this field as a Dropdown or not

                if (relatedTo is not null) // We need to retrieved and fill dropdown
                {
                    var list = await FillDropDown(property, relatedTo);
                    ItemsOfClasses.Add(new ItemsOfClass(order) { Name = property.Name, DisplayName = displayName, Collection = list, Type = TypeOfInput.DropDown });

                }
                else
                {
                    if (property.PropertyType == typeof(String))
                        ItemsOfClasses.Add(new ItemsOfClass(order) { Name = property.Name, DisplayName = displayName, Type = TypeOfInput.Text });
                    else if (property.PropertyType == typeof(DateTime))
                        ItemsOfClasses.Add(new ItemsOfClass(order) { Name = property.Name, DisplayName = displayName, Type = TypeOfInput.DateTime });
                    else if (property.PropertyType == typeof(Boolean))
                        ItemsOfClasses.Add(new ItemsOfClass(order) { Name = property.Name, DisplayName = displayName, Value = "false", Type = TypeOfInput.CheckBox });
                    else if (property.PropertyType.IsEnum)
                    {
                        var list = FillEnum(property, displayName);
                        ItemsOfClasses.Add(new ItemsOfClass(order) { Name = property.Name, DisplayName = displayName, Type = TypeOfInput.Enum, Collection = list });
                    }
                    else
                        ItemsOfClasses.Add(new ItemsOfClass(order) { Name = property.Name, DisplayName = displayName, Type = TypeOfInput.Text });
                }

            }
        }


        private async Task Clear()
        {
            await Init();
        }


        private async Task<List<RelatedTo>> GetRelatedTo(string entityName, string displayName)
        {
            entityName = entityName.Replace("Dto", "");
            List<RelatedTo> relatedTos = new List<RelatedTo>();
            var res = await Http.GetAsync($"{entityName}/Get");
            if (!res.IsSuccessStatusCode)
                throw new Exception("");
            var data = DeserializeObject<ApiResult<List<RelatedTo>>>(await res.Content.ReadAsStringAsync());
            foreach (var item in data.Data)
            {

                var idProp = item.GetType().GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
                var displayProp = item.GetType().GetProperty(displayName, BindingFlags.Public | BindingFlags.Instance);

                var a = new RelatedTo() { Name = displayProp?.GetValue(item, null)?.ToString(), Id = idProp?.GetValue(item, null)?.ToString() };

                relatedTos.Add(a);
            }
            return relatedTos;
        }

        #endregion

        #region ... Parameter...

        [Parameter]
        public IStringLocalizer Resource { get; set; }
        [Parameter] public EventCallback<string> OnFinished { get; set; }
        [Parameter] public Type Dto { get; set; }
        [Parameter] public string Key { get; set; }

        #endregion








    }





    #region ...Related Classes...

    /// <summary>
    /// Data Stored for each field
    /// </summary>
    public class ItemsOfClass
    {
        public ItemsOfClass(int order)
        {
            this.Order = order;
        }

        public string Name { get; set; }
        public string Value { get; set; } = "";
        public string DisplayName { get; set; }
        public List<RelatedTo> Collection { get; set; }
        public TypeOfInput Type { get; set; } = TypeOfInput.Text;
        public DateTime DateTime { get; set; } 
        public bool Valuebool { get; set; }

        public int Order { get; set; }
    }









    /// <summary>
    /// Types of explored fields
    /// </summary>
    public enum TypeOfInput
    {
        Text, DropDown, DateTime, CheckBox, Enum
    }



    /// <summary>
    /// This is related to drop downs that want to show related data based on its foreign key
    /// </summary>
    public class RelatedTo
    {
        //ToDo Make it Generic so we dont need to add anymore items
        public string Name { get; set; }
        public string Value { get; set; }
        public string Code { get; set; }
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string FollowUpCode { get; set; }
        public string HardwareSerial { get; set; }
        public string UnitVolume { get; set; }
        public string BatchNumber { get; set; }
        public string Stage { get; set; }
        public string UnitPerPack { get; set; }






        public string Id { get; set; }

    }








    #endregion











}
