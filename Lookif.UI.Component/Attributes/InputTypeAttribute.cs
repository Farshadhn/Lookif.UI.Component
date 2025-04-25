namespace Lookif.UI.Component.Attributes;
[System.AttributeUsage(System.AttributeTargets.Property)]
public class InputTypeAttribute : System.Attribute
{
    public InputTypeEnum InputTypeEnum { get; set; }
    public InputTypeAttribute(InputTypeEnum inputTypeEnum)
    {
        InputTypeEnum = inputTypeEnum;
    }


}
public enum InputTypeEnum
{
    Text, Color
}
