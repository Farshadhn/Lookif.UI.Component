namespace Lookif.UI.Component.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class RelatedToAttribute : System.Attribute
    {
        public string Name;
        public string DisplayName;
        public double version;

        public RelatedToAttribute(string name,string DisplayName ="Name")
        {
            this.DisplayName = DisplayName;
            Name = name;
            version = 1.0;
        }
    }
}
