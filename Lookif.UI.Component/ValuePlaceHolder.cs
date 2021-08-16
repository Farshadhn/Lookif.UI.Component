using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lookif.UI.Component
{
    /// <summary>
    /// We use this place holder for places like grid view to convert objects to a predefined value,displayname objects
    /// </summary>
    public class ValuePlaceHolder
    {

        public string ObjectValue { get; set; }
        public string ObjectName{ get; set; }
        public string ObjectDisplayName{ get; set; }
        public Color Color{ get; set; }

    }
}
