using System;
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
using Lookif.Library.Common.Exceptions;
using Lookif.UI.Component.Models;
using System.Text;

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


        bool ready = false;
        #region ...Events...

        #region ... Built in ...

        protected async override Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender)
            {
                await Init();
                if (Key != default)
                    await Edit(Key);
                await FillAllDropDownsTogether();
                ready = true;
                StateHasChanged();
            }
        }

        private async Task FillAllDropDownsTogether()
        {
            if (_requestedModels is { Count: > 0 })
            {
                HttpRequestMessage request = new(HttpMethod.Post, "RequestedModel/RequestedModel");

                request.Content = new StringContent(SerializeObject(_requestedModels),
                                                            Encoding.UTF8,
                                                            "application/Json");
                using var response = await Http.SendAsync(request, new CancellationTokenSource(100000).Token);
                var resInStreing = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    var resInString = await response.Content.ReadAsStringAsync();
                    var resInObj = DeserializeObject<Dictionary<string, List<DropDownHolder>>>(resInString);
                    foreach (var item in resInObj)
                    {
                        ItemsOfClasses.First(x => x.Name == item.Key).Collection = item.Value.Select(x => new RelatedTo() { Name = x.Value, Id = x.Id }).ToList();
                    }



                }
            }
        }


        #endregion

        #region ... Add ... 
        /// <summary>
        /// Add Or Create New
        /// </summary>  
        /// <returns></returns>
        private async Task Add()
        {
            if (Key != default)
            {
                var IsItReallyOkay = await Confirm();

                if (!IsItReallyOkay)
                    return;
            }
            object insertOrUpdate = Activator.CreateInstance(Dto);


            ConvertWholeObject(ref insertOrUpdate);
            var returnStr = String.Empty;


            if (!CheckRequiedItems(insertOrUpdate))
                return;


            string notificationText;
            string notificationHeaderText;

            if (Key == default)
            {
                var responseMessage = await Http.PostAsJsonAsync($"{ModelName}/create", insertOrUpdate);
                var resInString = await responseMessage.Content.ReadAsStringAsync();
                var res = DeserializeObject<ApiResult<object>>(resInString);
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
                var resInString = await responseMessage.Content.ReadAsStringAsync();
                var res = DeserializeObject<ApiResult<object>>(resInString);

                if (!res.IsSuccess)
                {

                    toastService.ShowError(res.Message, basicResource["InputErrorHeader"].Value);
                    return;
                }
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



        #region ... Add Related ... 

        //private async Task<bool> CheckResponse(HttpResponseMessage httpResponseMessage)
        //{
        //    if (httpResponseMessage.IsSuccessStatusCode)
        //        return true;
        //    var error = await httpResponseMessage.Content.ReadAsStringAsync();
        //    var res = DeserializeObject<ApiResult<object>>(error);

        //}


        private void ConvertWholeObject(ref object insertOrUpdate)
        {
            foreach (var item in ItemsOfClasses)
            {
                var prop = insertOrUpdate.GetType().GetProperty(item.Name, BindingFlags.Public | BindingFlags.Instance);
                if (null == prop || !prop.CanWrite) continue;
                var targetType = prop.PropertyType;
                try
                {

                    var IsItCollection = item.Type == TypeOfInput.MultipleSelectedDropDown || item.Type == TypeOfInput.DropDown || item.Type == TypeOfInput.Enum;
                    var IsItDateTime = (targetType == typeof(DateTime) || targetType == typeof(DateTime?));
                    var IsItBoolean = (targetType == typeof(Boolean) || targetType == typeof(Boolean?));

                    Console.WriteLine(SerializeObject(item.Name));
                    Console.WriteLine(SerializeObject(item.Type));
                    if (IsItCollection)
                    {
                        Console.WriteLine("IsItCollection");
                        ConvertCollection(insertOrUpdate, item, prop, targetType);
                    }
                    else if (IsItDateTime)
                    {
                        Console.WriteLine("IsItDateTime");
                        ConvertDateTime(insertOrUpdate, item, prop);
                    }
                    else if (IsItBoolean)
                    {
                        Console.WriteLine("IsItBoolean");
                        ConvertBoolean(insertOrUpdate, item, prop);
                    }
                    else
                    {
                        Console.WriteLine("other");
                        ConvertOtherValues(insertOrUpdate, item, prop, targetType);
                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine("converrrrrrrrrrrrrrrrrrrt33");
                    Console.WriteLine(ex.Message);
                    var rs = relatedSource.Find(x => x.Name == prop.Name);
                    var error = basicResource["InputError"].Value;
                    toastService.ShowError($"{error} :  -'{rs}'-Detail:'{ex.Message}'", basicResource["InputErrorHeader"].Value);
                    return;
                }

            }


        }

        private void ConvertOtherValues(object insertOrUpdate, ItemsOfClass item, PropertyInfo prop, Type targetType)
        {
            var convertedValue = default(object);
            var converter = TypeDescriptor.GetConverter(targetType);
            if (string.IsNullOrEmpty(item?.Value))
                convertedValue = null;

            else
            {
                if (converter.CanConvertFrom(typeof(string)))
                {
                    // convertedValue = converter.ConvertFromInvariantString(item?.Value); //ToDo Check Why we needed ConvertFromInvariantString
                    convertedValue = converter.ConvertFromString(item?.Value);
                }

            }
            prop.SetValue(insertOrUpdate, convertedValue, null);
        }

        private void ConvertDateTime(object insertOrUpdate, ItemsOfClass item, PropertyInfo prop)
        {
            Console.WriteLine(SerializeObject(item.DateTime));
            prop.SetValue(insertOrUpdate, item.DateTime, null);
        }
        private void ConvertBoolean(object insertOrUpdate, ItemsOfClass item, PropertyInfo prop)
        {
            prop.SetValue(insertOrUpdate, item.Valuebool, null);
        }
        private bool CheckRequiedItems(object insertOrUpdate)
        {
            //Check required
            foreach (var item in ItemsOfClasses.Where(x => x.Required))
            {
                var value = insertOrUpdate.GetPropValue(item.Name);

                if (value is null)
                {
                    var error = basicResource["InputError"].Value;
                    var errorDetail = basicResource["InputRequireError"].Value;

                    var name = relatedSource.FirstOrDefault(x => x.Name == item.Name)?.Value;
                    errorDetail = errorDetail.Replace("{field}", name);
                    toastService.ShowError(errorDetail, basicResource["InputErrorHeader"].Value);
                    return false;
                }
                else if (value.GetType() == typeof(string) && value == default)
                {
                    var error = basicResource["InputError"].Value;
                    var errorDetail = basicResource["InputRequireError"].Value;

                    var name = relatedSource.FirstOrDefault(x => x.Name == item.Name)?.Value;
                    errorDetail = errorDetail.Replace("{field}", name);
                    toastService.ShowError(errorDetail, basicResource["InputErrorHeader"].Value);
                    return false;
                }
                else if (value.GetType() != typeof(string) && value.ToString() == Activator.CreateInstance(value.GetType()).ToString())
                {
                    var error = basicResource["InputError"].Value;
                    var errorDetail = basicResource["InputRequireError"].Value;

                    var name = relatedSource.FirstOrDefault(x => x.Name == item.Name)?.Value;
                    errorDetail = errorDetail.Replace("{field}", name);
                    toastService.ShowError(errorDetail, basicResource["InputErrorHeader"].Value);
                    return false;
                }
            }
            return true;
        }
        private void ConvertCollection(object insertOrUpdate, ItemsOfClass item, PropertyInfo prop, Type targetType)
        {
            //Console.WriteLine(item.Value);
            //Console.WriteLine(SerializeObject(item.ValueColection));
            //Console.WriteLine(SerializeObject(item.ValueColection.FirstOrDefault()));
            //Console.WriteLine(SerializeObject(item.Type));
            //Console.WriteLine(SerializeObject(targetType.FullName));
            var convertedValue = default(object);

            if (item.ValueColection is not null && item.ValueColection.FirstOrDefault() is not null)
                convertedValue = item.Type switch
                {
                    TypeOfInput.DropDown or TypeOfInput.Enum =>
                     TypeDescriptor.GetConverter(targetType).ConvertFrom(item.ValueColection.FirstOrDefault().ToString()),
                    TypeOfInput.MultipleSelectedDropDown => GetList(item.ValueColection, targetType.GetGenericArguments()[0]),
                    _ => null
                };
            else
                convertedValue = null;
            Console.WriteLine(SerializeObject(convertedValue));
            Console.WriteLine("Done");
            prop.SetValue(insertOrUpdate, convertedValue, null);
        }
        private object GetList(IEnumerable<object> valueColection, Type type)
        {
            //we need to convert everything
            Console.WriteLine("چندتایی دارپ داون");
            Console.WriteLine(SerializeObject(valueColection));
            try
            {
                if (type == typeof(Guid))
                    return valueColection?.ToList().ConvertAll(x => Guid.Parse(x.ToString()));
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message);
            }

            Console.WriteLine("چندتایی دارپ 2");
            return valueColection;
        }
        #endregion 





        #endregion
        #endregion



        #region ... Definition...

        List<LocalizedString> relatedSource { get; set; }
        private string ModelName => Dto.Name.Replace("Dto", "");

        public List<ItemsOfClass> ItemsOfClasses { get; set; } = new List<ItemsOfClass>();

        private List<RequestedModel> _requestedModels { get; set; } = new List<RequestedModel>();


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
            Console.WriteLine(SerializeObject(id));
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
                {
                    item!.DateTime = (DateTime)prop.GetValue(data, null)!;

                }
                else if (prop?.PropertyType == typeof(Boolean))
                {
                    item!.Valuebool = (bool)prop.GetValue(data, null)!;
                }
                else if (item.Type is TypeOfInput.DropDown or TypeOfInput.Enum)
                {

                    var desiredData = prop.GetValue(data, null)!;
                    item!.ValueColection = new List<string>() { desiredData.ToString() };
                }
                else if (item.Type is TypeOfInput.MultipleSelectedDropDown)
                {
                    var desiredData = prop.GetValue(data, null)!;
                    if (desiredData is List<Guid>)
                        item!.ValueColection = ((List<Guid>)desiredData).Select(x => x.ToString()).ToList();
                    else
                        item!.ValueColection = ((IEnumerable)desiredData).Cast<string>().ToList();

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
        private async Task<List<RelatedTo>> FillDropDown(RelatedToAttribute relatedTo, string dropdownName, CancellationToken cancellationToken)
        {

            var listOfRelatedTo = await GetRelatedTo(relatedTo, dropdownName, cancellationToken);

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
        private bool CheckEligibilityToShow(HiddenDtoAttribute hiddenDtoAttribute, formStatus formStatus)
        {
            if (hiddenDtoAttribute is null)
                return true;

            return (hiddenDtoAttribute.status, formStatus) switch
            {
                (HiddenStatus.Edit, formStatus.Edit) => false,
                (HiddenStatus.Create, formStatus.Create) => false,
                (HiddenStatus.EditAndCreate, formStatus.Edit) => false,
                (HiddenStatus.EditAndCreate, formStatus.Create) => false,
                _ => true
            };
        }

        private async Task Init()
        {
            ready = false;
            CancellationTokenSource cancellationTokenSource = new CancellationTokenSource(100000);
            ItemsOfClasses = new List<ItemsOfClass>();
            _requestedModels = new();
            await Task.Delay(100);
            PropertyInfo[] propertyInfos = Dto.GetProperties();
            foreach (var property in propertyInfos)
            {

                var hiddenDto = property.GetCustomAttribute<HiddenDtoAttribute>();

                if (!CheckEligibilityToShow(hiddenDto, (Key != default) ? formStatus.Edit : formStatus.Create))  // What we don't want to include in our form
                    continue;
                var key = property.GetCustomAttribute<KeyAttribute>();
                if (key is not null || property.Name == "Id") continue;

                var displayName = property.GetCustomAttribute<DisplayAttribute>()?.Name;//Name that should be placed as its label

                var relatedTo = property.GetCustomAttribute<RelatedToAttribute>();// To check if we need to implement this field as a Dropdown or not
                var order = property.GetCustomAttribute<OrderAttribute>()?.Order ?? 100;// To check if we need to implement this field as a Dropdown or not
                var file = property.GetCustomAttribute<FileAttribute>();
                var required = property.GetCustomAttribute<RequiredAttribute>() is null ? false : true;
                var appearance = property.GetCustomAttribute<AppearanceAttribute>() ?? new AppearanceAttribute(0, 0, 0);

                if (relatedTo is not null) // We need to retrieved and fill dropdown
                {

                    if (Key != default && !string.IsNullOrEmpty(relatedTo.FunctionToCall))
                        relatedTo.FunctionToCall = $"{relatedTo.FunctionToCall}/{Key}";
                    var list = await FillDropDown(relatedTo, property.Name, cancellationTokenSource.Token);
                    ItemsOfClasses.Add(
                        new ItemsOfClass(order)
                        {
                            Name = property.Name,
                            DisplayName = displayName,
                            Type = property.PropertyType.GetInterface(nameof(IEnumerable)) is not null ? TypeOfInput.MultipleSelectedDropDown : TypeOfInput.DropDown,
                            Value = null,
                            property = property,
                            Required = required,
                            Appearance = new Appearance(appearance.DivSize, appearance.LabelSize, appearance.InputSize)
                        });




                }
                else
                {
                    if (file is not null)
                        ItemsOfClasses.Add(new ItemsOfClass(order) { Name = property.Name, DisplayName = displayName, Type = TypeOfInput.File, property = property, Required = required, Appearance = new Appearance(appearance.DivSize, appearance.LabelSize, appearance.InputSize) });

                    else if (property.PropertyType == typeof(String))
                        ItemsOfClasses.Add(new ItemsOfClass(order) { Name = property.Name, DisplayName = displayName, Type = TypeOfInput.Text, Required = required, Appearance = new Appearance(appearance.DivSize, appearance.LabelSize, appearance.InputSize) });
                    else if (property.PropertyType == typeof(DateTime) || property.PropertyType == typeof(DateTime?))
                        ItemsOfClasses.Add(new ItemsOfClass(order) { Name = property.Name, DisplayName = displayName, Type = TypeOfInput.DateTime, Required = required, Appearance = new Appearance(appearance.DivSize, appearance.LabelSize, appearance.InputSize) });
                    else if (property.PropertyType == typeof(Boolean))
                        ItemsOfClasses.Add(new ItemsOfClass(order) { Name = property.Name, DisplayName = displayName, Value = "false", Type = TypeOfInput.CheckBox, Valuebool = false, Required = required, Appearance = new Appearance(appearance.DivSize, appearance.LabelSize, appearance.InputSize) });
                    else if (property.PropertyType.IsEnum)
                    {
                        var list = FillEnum(property, displayName);
                        ItemsOfClasses.Add(new ItemsOfClass(order) { Name = property.Name, DisplayName = displayName, Type = TypeOfInput.Enum, Collection = list, Required = required, Appearance = new Appearance(appearance.DivSize, appearance.LabelSize, appearance.InputSize) });
                    }
                    else
                        ItemsOfClasses.Add(new ItemsOfClass(order) { Name = property.Name, DisplayName = displayName, Type = TypeOfInput.Text, Required = required, Appearance = new Appearance(appearance.DivSize, appearance.LabelSize, appearance.InputSize) });


                }

            }
            await FillAllDropDownsTogether();
            ready = true;
        }


        private async Task Clear()
        {

            await Init();



        }


        private async Task<List<RelatedTo>> GetRelatedTo(RelatedToAttribute relatedTo, string dropdownName, CancellationToken cancellationToken)
        {
            var entityName = relatedTo.Name.Replace("Dto", "");
            var sendToAddress = $"{entityName}/{(string.IsNullOrEmpty(relatedTo.FunctionToCall) ? $"GetDropDown/{relatedTo.DisplayName}" : relatedTo.FunctionToCall)}";

            _requestedModels.Add(new RequestedModel()
            {
                FunctionToCall = sendToAddress,
                ModelName = dropdownName
            });
            return new();

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
        public List<string> ValueColection { get; set; } // This is for Dropdowns
        public string DisplayName { get; set; }
        public List<RelatedTo> Collection { get; set; }
        public TypeOfInput Type { get; set; } = TypeOfInput.Text;
        public DateTime DateTime { get; set; }
        public bool Valuebool { get; set; }
        public PropertyInfo property { get; set; }
        public int Order { get; set; }
        public bool Required { get; set; }
        public Appearance Appearance { get; set; }


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
