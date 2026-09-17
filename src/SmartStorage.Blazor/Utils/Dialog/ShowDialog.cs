using MudBlazor;
using SmartStorage.Blazor.Authentication;
using SmartStorage.Blazor.Enums;
using SmartStorage.Blazor.Pages.Dialog;

namespace SmartStorage.Blazor.Utils.ShowDialog
{
    public class ShowDialog
    {
        private readonly IDialogService _dialogService;

        private readonly SessionExpiration _session;

        public ShowDialog(IDialogService dialogService, SessionExpiration session)
        {
            _dialogService = dialogService;
            _session = session;
        }

        public async Task<bool> ShowDialogAsync(string message, string title = "", string navigate = "", EDialogStates state = EDialogStates.Success, bool showCancel = false, bool showYes = false, bool ignoreExpiredSession = false)
        {
            if (_session.IsExpired && !ignoreExpiredSession)
                return false;

            var parameters = new DialogParameters<Dialog>
            {
                { x => x.ContentText, message },
                { x => x.Navigate, navigate },
                { x => x.State, state },
                { x => x.ShowYes, showYes },
                { x => x.ShowCancel, showCancel }
            };

            DialogOptions options = new DialogOptions
            {
                BackdropClick = false
            };

            var dialog = await _dialogService.ShowAsync<Dialog>(title, parameters, options);

            var result = await dialog.Result;

            if (result is null || result.Canceled)
                return false;

            return true;
        }
    }
}
