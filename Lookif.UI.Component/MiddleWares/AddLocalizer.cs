using Blazored.LocalStorage;
using Lookif.UI.Component.Utility;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Lookif.UI.Component.MiddleWares
{
    public static class Localizer
    {

        public static IServiceCollection AddLocalizer(this IServiceCollection services)
        {
            services.AddLocalization();
            services.AddBlazoredLocalStorage();

            return services;
        }
        public async static Task<WebAssemblyHost> AddDefaultCulture(this WebAssemblyHost host)
        {
            await host.SetDefaultCulture();

            return host;
        }
        public async static Task SetDefaultCulture(this WebAssemblyHost host)
        {
            var localStorage = host.Services.GetRequiredService<ILocalStorageService>();
            var cultureFromLocalStorage = await localStorage.GetItemAsync<string>("culture");

            CultureInfo culture;
            if (cultureFromLocalStorage != null)
            {
                if (cultureFromLocalStorage == "fa-IR")
                    culture = PersianCulture.GetPersianCulture();
                else
                    culture = new CultureInfo(cultureFromLocalStorage);
            }
            else
            {
                //PersianCulture.GetPersianCulture();
                culture = PersianCulture.GetPersianCulture();
            }

            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }
    }
}
