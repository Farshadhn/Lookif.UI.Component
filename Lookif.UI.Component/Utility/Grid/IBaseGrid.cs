using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lookif.UI.Component.Utility.Grid
{
    interface IBaseGrid<T>
    {
        /// <summary>
        /// Send domain name as parameter to get List
        /// </summary>
        /// <param name="domainName"></param>
        /// <returns></returns>
        Task<List<T>> GetAll(string domainName);
    }
}
