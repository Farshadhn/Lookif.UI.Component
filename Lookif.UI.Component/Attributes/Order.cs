namespace Lookif.UI.Component.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class OrderAttribute : System.Attribute
    {
        public int Order; 

        public OrderAttribute(int order)
        {
            this.Order = order;
        }
    }
}
