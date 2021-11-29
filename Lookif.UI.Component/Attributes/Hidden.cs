using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lookif.UI.Component.Attributes
{
    /// <summary>
    /// This will make property hidden to selectDto
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class HiddenAttribute : System.Attribute
    {
      
    }
    public enum HiddenStatus
    { 
    Edit,Create,EditAndCreate
    }
    public enum formStatus
    {
        Edit, Create
    }
    /// <summary>
    /// This will make property hidden to Dto
    /// </summary>
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class HiddenDtoAttribute : System.Attribute
    {
        public HiddenDtoAttribute(HiddenStatus hiddenStatus = HiddenStatus.EditAndCreate)
        {
            status = hiddenStatus;
        }
        public HiddenStatus status { get; set; }
    }
}
