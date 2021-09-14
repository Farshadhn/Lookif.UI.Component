using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Localization;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
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
                culture = new CultureInfo(cultureFromLocalStorage);
            }
            else
            {
                culture = new CultureInfo("fa-IR");
            }

            CultureInfo.DefaultThreadCurrentCulture = culture;
            CultureInfo.DefaultThreadCurrentUICulture = culture;
        }
    }
}
