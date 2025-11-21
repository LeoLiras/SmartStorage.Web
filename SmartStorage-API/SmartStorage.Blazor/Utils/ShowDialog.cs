using MudBlazor;
using SmartStorage.Blazor.Enums;
using SmartStorage.Blazor.Pages.Dialog;

namespace SmartStorage.Blazor.Utils
{
    public class ShowDialog
    {
        private readonly IDialogService _dialogService;

        public ShowDialog(IDialogService dialogService)
        {
            _dialogService = dialogService;
        }

        public async Task<bool> ShowDialogAsync(string message, string title = "", string navigate = "", EDialogStates state = EDialogStates.Success, bool showCancel = false, bool showYes = false)
        {
            var parameters = new DialogParameters<Dialog>
            {
                { x => x.ContentText, message },
                { x => x.Navigate, navigate },
                { x => x.State, state },
                { x => x.ShowYes, showYes },
                { x => x.ShowCancel, showCancel }
            };

            var dialog = await _dialogService.ShowAsync<Dialog>(title, parameters);

            var result = await dialog.Result;

            if (result is null || result.Canceled)
                return false;

            return true;
        }
    }
}
