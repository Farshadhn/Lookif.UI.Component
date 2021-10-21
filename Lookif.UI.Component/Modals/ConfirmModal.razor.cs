using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Blazored.Modal;
using Blazored.Modal.Services;
using Microsoft.AspNetCore.Components;

namespace Lookif.UI.Component.Modals
{
    public partial class ConfirmModal
    {
         


        async Task OnCancel()
        { 
            await BlazoredModal.CloseAsync(ModalResult.Cancel());
           
        }

        async Task OnConfirm()
        { 
            await BlazoredModal.CloseAsync(ModalResult.Ok(true));
        }
         
        [Parameter] public string Title { get; set; }
        [Parameter] public string YES { get; set; }
        [Parameter] public string NO { get; set; }
        [CascadingParameter] BlazoredModalInstance BlazoredModal { get; set; }
    }
}
