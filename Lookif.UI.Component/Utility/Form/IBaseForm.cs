using Lookif.UI.Component.Components.SeparatedComponents.SimpleForm;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lookif.UI.Component.Utility
{
    interface IBaseForm
    {
        ///// <summary>
        ///// Insert new 
        ///// </summary>
        ///// <param name="data"></param>
        ///// <param name="domainName"></param>
        ///// <returns></returns>
        //Task<T> Insert(string domainName, T data);


        Task<List<RelatedTo>> GetRelatedTo(string entityName, string displayName);
         



    }
}
