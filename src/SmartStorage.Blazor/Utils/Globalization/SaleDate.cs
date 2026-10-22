namespace SmartStorage.Blazor.Utils.Globalization
{
    public static class SaleDate
    {
        #region Methods

        /// <summary>
        /// O MudDatePicker guarda só o dia, à meia-noite quando o usuário escolhe outro, ou o instante em
        /// que a tela abriu quando não mexe: a venda saía com a hora da abertura da tela. O dia vem da tela
        /// e a hora vem do envio. O Kind é Local para o JSON levar o fuso do navegador.
        /// </summary>
        public static DateTime At(DateTime? chosenDay, DateTime now)
            => DateTime.SpecifyKind((chosenDay ?? now).Date + now.TimeOfDay, DateTimeKind.Local);

        #endregion
    }
}
