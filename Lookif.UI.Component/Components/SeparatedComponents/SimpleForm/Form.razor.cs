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
using Microsoft.AspNetCore.Components;
using Lookif.UI.Common.Models;
using static Newtonsoft.Json.JsonConvert;
using Microsoft.Extensions.Localization;
using System.ComponentModel;
using System.Threading;
using Blazored.Modal;
using Blazored.Modal.Services;
using Lookif.UI.Component.Modals;
using Microsoft.AspNetCore.Components.Forms;
using System.IO;
using Lookif.Library.Common.CommonModels;
using System.Collections;
using util = Lookif.Library.Common.Utilities;
using Lookif.Library.Common.Utilities;

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


        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await Init();
                if (Key != default)
                    await Edit(Key);


                StateHasChanged();
            }
        }



        /// <summary>
        /// Add Or Create New
        /// </summary>  
        /// <returns></returns>
        private async Task Add()
        {
            await Task.Delay(1000);
            if (Key != default)
            {
                var IsItReallyOkay = await Confirm();

                if (!IsItReallyOkay)
                    return;
            }

            object insertOrUpdate = Activator.CreateInstance(Dto);

            foreach (var item in ItemsOfClasses)
            {
                var prop = insertOrUpdate.GetType().GetProperty(item.Name, BindingFlags.Public | BindingFlags.Instance);
                if (null == prop || !prop.CanWrite) continue;




                var targetType = prop.PropertyType.IsNullableType()
                                  ? Nullable.GetUnderlyingType(prop.PropertyType)
                                  : prop.PropertyType;





                if ((item.Type == TypeOfInput.MultipleSelectedDropDown || item.Type == TypeOfInput.DropDown ) && (item.ValueColection is not null && item.ValueColection.Any()))
                {
                     
                    //ToDo Make it generic 

                    var convertedValue = item.Type switch
                    {
                        TypeOfInput.DropDown =>
                          TypeDescriptor.GetConverter(targetType).ConvertFrom(item.ValueColection.FirstOrDefault()),
                        TypeOfInput.MultipleSelectedDropDown => GetList(item.ValueColection, targetType.GetGenericArguments()[0]),
                        _ => throw new NotImplementedException()
                    };

                    prop.SetValue(insertOrUpdate, convertedValue, null);

                    continue;
                }

                if (targetType == typeof(DateTime))
                {
                    prop.SetValue(insertOrUpdate, item.DateTime, null);
                    continue;
                }







                try
                {
                    if (string.IsNullOrEmpty(item?.Value)) continue;
                    var convertedValue = TypeDescriptor.GetConverter(targetType).ConvertFromInvariantString(item?.Value);
                    prop.SetValue(insertOrUpdate, convertedValue, null);
                    continue;
                }
                catch (Exception ex)
                {
                    var rs = relatedSource.Find(x => x.Name == prop.Name);
                    var error = basicResource["InputError"].Value;
                    toastService.ShowError($"{error}:  -'{rs}'-", basicResource["InputErrorHeader"].Value);
                    return;
                }

            }
            var returnStr = String.Empty;
            var notificationText = String.Empty;
            var notificationHeaderText = String.Empty;
            Console.WriteLine(SerializeObject(insertOrUpdate));

            if (Key == default)
            {
                var responseMessage = await Http.PostAsJsonAsync($"{ModelName}/create", insertOrUpdate);
                var res = DeserializeObject<ApiResult<object>>(await responseMessage.Content.ReadAsStringAsync());
                if (!res.IsSuccess)
                {
                    toastService.ShowError(res.Message, basicResource["InputErrorHeader"].Value);
                    return;
                }

                var efIdProp = res.Data.GetType().GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
                returnStr = efIdProp?.GetValue(res.Data)?.ToString();
                notificationText = basicResource["DoneAdding"].Value;
                notificationHeaderText = basicResource["DoneAddingHeader"].Value;
            }
            else
            {
                var responseMessage = await Http.PutAsJsonAsync($"{ModelName}/update/{Key}", insertOrUpdate);
                notificationText = basicResource["DoneEditing"].Value;
                notificationHeaderText = basicResource["DoneEditingHeader"].Value;

            }
            ItemsOfClasses = new List<ItemsOfClass>();
            await Task.Delay(200);
            StateHasChanged();
            await Init();

            await OnFinished.InvokeAsync(returnStr);
            toastService.ShowSuccess(notificationText, notificationHeaderText);
        }

        private object GetList(IEnumerable<object> valueColection, Type type)
        {
            //we need to convert everything
            if (type == typeof(Guid))
                return valueColection?.ToList().ConvertAll(x => Guid.Parse(x.ToString()));
            return valueColection;
        }

        #endregion



        #region ... Definition...

        List<LocalizedString> relatedSource { get; set; }
        private string ModelName => Dto.Name.Replace("Dto", "");

        public List<ItemsOfClass> ItemsOfClasses { get; set; } = new List<ItemsOfClass>();




        #endregion


        #region ... Functions...

        /// <summary>
        /// Confirm Alert
        /// </summary>
        /// <returns></returns>
        private async Task<bool> Confirm()
        {
            var parameters = new ModalParameters();
            parameters.Add("YES", basicResource["YES"].Value);
            parameters.Add("NO", basicResource["NO"].Value);
            var MessageForm = Modal.Show<ConfirmModal>(basicResource["WarnSignForConfirm"].Value, parameters);
            var result = await MessageForm.Result;
            return !result.Cancelled;
        }

        private async Task Edit(string id)
        {
            var dataObj = await Http.GetAsync($"{ModelName}/Get/{id}");

            var response = await dataObj.Content.ReadAsStringAsync();
            Console.WriteLine("dataObj");
            Console.WriteLine(response);
            var generic = typeof(ApiResult<>);
            Type constructed = generic.MakeGenericType(Dto);
            var res = DeserializeObject(response!, constructed);
            PropertyInfo prosp = res.GetType().GetProperty("Data", BindingFlags.Public | BindingFlags.Instance);

            var data = prosp.GetValue(res, null)!;




            foreach (var item in ItemsOfClasses)
            {
                PropertyInfo prop = data.GetType().GetProperty(item.Name, BindingFlags.Public | BindingFlags.Instance);
                Console.WriteLine(SerializeObject(prop?.Name));
                Console.WriteLine(SerializeObject(item?.Type));

                if (prop?.PropertyType == typeof(DateTime))
                {
                    item!.DateTime = (DateTime)prop.GetValue(data, null)!;

                }
                else if (prop?.PropertyType == typeof(Boolean))
                {
                    Console.WriteLine(prop.GetValue(data, null));
                    item!.Valuebool = (bool)prop.GetValue(data, null)!;
                }
                //else if (prop?.PropertyType.GetDirectInterfaces(true).Any(x => x == typeof(ICollection)) == true) 
                else if (item.Type is  TypeOfInput.MultipleSelectedDropDown)
                {

                    var desiredData = prop.GetValue(data, null)!;

                    item!.ValueColection = ((IEnumerable)desiredData).Cast<object>().ToList();

                    Console.WriteLine("item!.ValueColection");
                    Console.WriteLine(SerializeObject(item!.ValueColection));
                }
                else if (item.Type is TypeOfInput.DropDown)
                {

                    var desiredData = prop.GetValue(data, null)!;

                    item!.ValueColection = new List<object>();
                    item!.ValueColection.Add(desiredData);
                    Console.WriteLine("item!.ValueColection");
                    Console.WriteLine(SerializeObject(item!.ValueColection));
                }
                else
                {
                    item!.Value = prop?.GetValue(data, null)?.ToString();
                }

            }


        }


        /// <summary>
        /// This function is for storing data in drop down
        /// </summary>
        /// <param name="propertyInfo"></param>
        /// <param name="tableNameToBeRetrieved"></param>
        /// <returns></returns>
        private async Task<List<RelatedTo>> FillDropDown(RelatedToAttribute relatedTo, CancellationToken cancellationToken)
        {

            var listOfRelatedTo = await GetRelatedTo(relatedTo, cancellationToken);

            return listOfRelatedTo;

        }


        /// <summary>
        /// First priority is resource file
        /// Second is display name
        /// Last one is property name
        /// </summary>
        /// <returns></returns>
        private string GetDesirableValue(object property)
        {
            var rs = relatedSource.FirstOrDefault(x => x.Name == property.ToString());

            if (rs is not null)
                return rs.Value;



            var attribute = (DisplayAttribute)property.GetType().GetField(property.ToString())
                   .GetCustomAttributes<DisplayAttribute>(false).FirstOrDefault();
            if (attribute is not null)
                return attribute.Name;
            return property.ToString();

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
            Type enumType = propertyInfo.PropertyType;
            Type enumUnderlyingType = System.Enum.GetUnderlyingType(enumType);
            Array enumValues = System.Enum.GetValues(enumType);

            for (int i = 0; i < enumValues.Length; i++)
            {

                // Retrieve the value of the ith enum item.
                object value = enumValues.GetValue(i);

                var dropdownName = GetDesirableValue(value);
                // Convert the value to its underlying type (int, byte, long, ...)
                object underlyingValue = System.Convert.ChangeType(value, enumUnderlyingType);

                listOfRelatedTo.Insert(0, new RelatedTo() { Id = underlyingValue.ToString(), Name = dropdownName });
            }

            return listOfRelatedTo;
        }


        private void CheckValidation(IBrowserFile browserFile, ItemsOfClass ioc)
        {
            var limit = ioc.property.GetCustomAttribute<FileAttribute>();
            if (browserFile.Size > limit.MaxLength)
                throw new Exception(basicResource["WarnSignForErrorUpload-BigFile"].Value);


            var format = browserFile.Name.Split(".")[^1];

            if (limit.types.Any() && !limit.types.Contains(format))
                throw new Exception(basicResource["WarnSignForErrorUpload-FormatFile"].Value);

        }

        private async Task Upload(InputFileChangeEventArgs e, ItemsOfClass ioc)
        {
            try
            {
                MemoryStream ms = new();
                CheckValidation(e.File, ioc);

                await e.File.OpenReadStream(maxAllowedSize: 10512000).CopyToAsync(ms);


                var ImageFile = new UploadModel()
                {
                    File = ms.ToArray(),
                    FileName = e.File.Name
                };
                var responseMessage = await Http.PostAsJsonAsync($"FileModel/Upload", ImageFile);
                var res = DeserializeObject<ApiResult<string>>(await responseMessage.Content.ReadAsStringAsync());
                if (res.IsSuccess)
                {
                    ioc.Value = res.Data;
                }


            }
            catch (Exception ex)
            {

                toastService.ShowError(basicResource["WarnSignForErrorUpload"].Value, ex.Message);
            }

        }
        private bool CheckEligibilityToShow(HiddenDtoAttribute hiddenDtoAttribute, formStatus IsItInCreateMode)
        {
            Console.WriteLine("CheckEligibilityToShow");
            Console.WriteLine(IsItInCreateMode.ToString());
            if (hiddenDtoAttribute is null)
                return true;

            Console.WriteLine(hiddenDtoAttribute.ToString());
            Console.WriteLine(IsItInCreateMode.ToString());
            return (hiddenDtoAttribute.status, IsItInCreateMode) switch
            {
                (HiddenStatus.Edit, formStatus.Edit) =>false,
                (HiddenStatus.Create, formStatus.Create) =>false,
                _ => true
            };  
        }
        private async Task Init()
        {

            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(10000);
            ItemsOfClasses = new List<ItemsOfClass>();
            await Task.Delay(300);
            PropertyInfo[] propertyInfos = Dto.GetProperties();
            foreach (var property in propertyInfos)
            {
                var hiddenDto = property.GetCustomAttribute<HiddenDtoAttribute>();

                if (!CheckEligibilityToShow(hiddenDto, (Key != default) ? formStatus.Edit : formStatus.Create))  // What we don't want to include in our form
                    continue;
                var key = property.GetCustomAttribute<KeyAttribute>();
                if (!(key is null) || property.Name == "Id") continue;


                var displayName = property.GetCustomAttribute<DisplayAttribute>()?.Name;//Name that should be placed as its label

                var relatedTo = property.GetCustomAttribute<RelatedToAttribute>();// To check if we need to implement this field as a Dropdown or not
                var order = property.GetCustomAttribute<OrderAttribute>()?.Order ?? 100;// To check if we need to implement this field as a Dropdown or not
                var file = property.GetCustomAttribute<FileAttribute>();

                if (relatedTo is not null) // We need to retrieved and fill dropdown
                {

                    if (Key != default && !string.IsNullOrEmpty(relatedTo.FunctionToCall))
                        relatedTo.FunctionToCall = $"{relatedTo.FunctionToCall }/{Key}";
                    var list = await FillDropDown(relatedTo, cancellationTokenSource.Token);
                    ItemsOfClasses.Add(
                        new ItemsOfClass(order)
                        {
                            Name = property.Name,
                            DisplayName = displayName,
                            Collection = list,
                            Type =property.PropertyType.GetInterface(nameof(IEnumerable)) is not null ? TypeOfInput.MultipleSelectedDropDown : TypeOfInput.DropDown,
                            Value = null,
                            property = property,
                        });




                }
                else
                {
                    if (file is not null)
                        ItemsOfClasses.Add(new ItemsOfClass(order) { Name = property.Name, DisplayName = displayName, Type = TypeOfInput.File, property = property });

                    else if (property.PropertyType == typeof(String))
                        ItemsOfClasses.Add(new ItemsOfClass(order) { Name = property.Name, DisplayName = displayName, Type = TypeOfInput.Text });
                    else if (property.PropertyType == typeof(DateTime))
                        ItemsOfClasses.Add(new ItemsOfClass(order) { Name = property.Name, DisplayName = displayName, Type = TypeOfInput.DateTime });
                    else if (property.PropertyType == typeof(Boolean))
                        ItemsOfClasses.Add(new ItemsOfClass(order) { Name = property.Name, DisplayName = displayName, Value = "false", Type = TypeOfInput.CheckBox, Valuebool = false });
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


        private async Task<List<RelatedTo>> GetRelatedTo(RelatedToAttribute relatedTo, CancellationToken cancellationToken)
        {
            var entityName = relatedTo.Name.Replace("Dto", "");
            List<RelatedTo> relatedTos = new();

            var sendToAddress = $"{entityName}/{(string.IsNullOrEmpty(relatedTo.FunctionToCall) ? "Get" : relatedTo.FunctionToCall)}";

            var res = await Http.GetAsync(sendToAddress, cancellationToken);
            if (!res.IsSuccessStatusCode)
                throw new Exception("");
            var data = DeserializeObject<ApiResult<List<RelatedTo>>>(await res.Content.ReadAsStringAsync(cancellationToken));
            foreach (var item in data.Data)
            {
                var idProp = item.GetType().GetProperty("Id", BindingFlags.Public | BindingFlags.Instance);
                var displayProp = item.GetType().GetProperty(relatedTo.DisplayName, BindingFlags.Public | BindingFlags.Instance);

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
        [CascadingParameter] public IModalService Modal { get; set; }


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
        public ItemsOfClass()
        {

        }
        public string Name { get; set; }
        public string Value { get; set; } = "";
        public List<object> ValueColection { get; set; } // This is for Dropdowns
        public string DisplayName { get; set; }
        public List<RelatedTo> Collection { get; set; }
        public TypeOfInput Type { get; set; } = TypeOfInput.Text;
        public DateTime DateTime { get; set; }
        public bool Valuebool { get; set; }
        public PropertyInfo property { get; set; }
        public int Order { get; set; }



    }









    /// <summary>
    /// Types of explored fields
    /// </summary>
    public enum TypeOfInput
    {
        Text, DropDown, MultipleSelectedDropDown, DateTime, CheckBox, Enum, File
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
        public string dayOfWeek { get; set; }






        public string Id { get; set; }

    }








    #endregion











}
