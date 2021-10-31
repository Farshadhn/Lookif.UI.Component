using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lookif.UI.Component.Attributes
{
    [System.AttributeUsage(System.AttributeTargets.Property)]
    public class FileAttribute : System.Attribute
    {

        public long MaxLength = 0;
        public List<string> types = new();
        /// <summary>
        /// 
        /// </summary>
        /// <param name="MaxLength"></param>
        /// <param name="types">comma separated</param>
        public FileAttribute(long MaxLength, string types = "")
        {
            this.MaxLength = MaxLength;
            this.types = !string.IsNullOrEmpty(types)?types.Split(",").ToList(): new();
        }
    }
}
