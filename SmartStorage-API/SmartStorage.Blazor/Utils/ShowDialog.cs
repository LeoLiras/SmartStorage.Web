using MudBlazor;
using SmartStorage.Blazor.Pages;


namespace SmartStorage.Blazor.Utils
{
    public class ShowDialog
    {
        private readonly IDialogService _dialogService;

        public ShowDialog(IDialogService dialogService)
        {
            _dialogService = dialogService;
        }

        public async Task ShowDialogAsync(string message, string title = "", Color color = Color.Info, string Navigate = "")
        {
            var parameters = new DialogParameters<Dialog>
        {
            { x => x.ContentText, message },
            { x => x.Color, color },
            {x => x.Navigate, Navigate }
        };

            await _dialogService.ShowAsync<Dialog>(title, parameters);
        }
    }
}
