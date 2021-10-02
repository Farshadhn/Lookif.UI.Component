using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Blazored.Modal;
using Blazored.Modal.Services;
using Blazored.Toast;
using Microsoft.Extensions.DependencyInjection;

namespace Lookif.UI.Component.Extension
{
    public static class ServiceCollectionExtensions
    {
        public static IServiceCollection AddLookif(this IServiceCollection services)
        {
            services.AddBlazoredModal(); 
            services.AddBlazoredToast(); 
            return services;
        }
    
    }
}
