namespace Lookif.UI.Component.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class RelatedToAttribute : System.Attribute
    {
        public string Name;
        public string DisplayName;
        public double version;
        public string FunctionToCall;
        public RelatedToAttribute(string name, string DisplayName = "Name", string functionToCall = "")
        {
            this.DisplayName = DisplayName;
            FunctionToCall = functionToCall;
            Name = name;
            version = 1.0;
        }
    }
}
