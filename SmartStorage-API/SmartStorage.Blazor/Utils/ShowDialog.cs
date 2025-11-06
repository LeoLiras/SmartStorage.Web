using MudBlazor;
using SmartStorage.Blazor.Pages;
using static SmartStorage.Blazor.Enums.DialogStates;


namespace SmartStorage.Blazor.Utils
{
    public class ShowDialog
    {
        private readonly IDialogService _dialogService;

        public ShowDialog(IDialogService dialogService)
        {
            _dialogService = dialogService;
        }

        public async Task ShowDialogAsync(string message, string title = "", string navigate = "", EDialogStates state = EDialogStates.Success, bool showCancel = false, bool showYes = false)
        {
            var parameters = new DialogParameters<Dialog>
            {
                { x => x.ContentText, message },
                { x => x.Navigate, navigate },
                { x => x.State, state },
                { x => x.ShowYes, showYes },
                { x => x.ShowCancel, showCancel }
            };

            await _dialogService.ShowAsync<Dialog>(title, parameters);
        }
    }
}
