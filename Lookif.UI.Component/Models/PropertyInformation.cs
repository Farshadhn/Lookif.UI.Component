using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lookif.UI.Component.Models
{
    internal sealed class PropertyInformation
    {
        /// <summary>
        /// Display Name attribute
        /// </summary>
        public string Displayname { get; set; }


        /// <summary>
        /// Property Name 
        /// </summary>
        public string PropertyName { get; set; }



        /// <summary>
        /// Binded Value for Creating New
        /// </summary>
        public string Value { get; set; }


        /// <summary>
        /// Check if it a Key or not
        /// </summary>
        public bool Key{ get; set; } 


        /// <summary>
        /// Type of correspponding object
        /// </summary>
        public Type TypeOfObject { get; set; }


        public string Width { get; set; }

    }
}
