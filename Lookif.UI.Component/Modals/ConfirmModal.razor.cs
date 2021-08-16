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
            await OnCancelEventCallback.InvokeAsync();
            await BlazoredModal.CloseAsync(ModalResult.Cancel());
           
        }

        async Task OnConfirm(object data)
        {
            await OnConfirmEventCallback.InvokeAsync(data);
            await BlazoredModal.CloseAsync(ModalResult.Ok(true));
        }

        [Parameter] public object Data{ get; set; }
        [Parameter] public EventCallback<object> OnConfirmEventCallback { get; set; }
        [Parameter] public EventCallback OnCancelEventCallback { get; set; }
        [Parameter] public string Title { get; set; }
        [CascadingParameter] BlazoredModalInstance BlazoredModal { get; set; }
    }
}
