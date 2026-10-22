using System.Globalization;

namespace SmartStorage.Blazor.Utils.Globalization
{
    public static class InputCulture
    {
        #region Properties

        /// <summary>
        /// Cultura dos campos do MudBlazor, que por padrão usam a invariante: nela a vírgula é separador
        /// de milhar, e "15,90" virava 1590 sem aviso. O separador de milhar daqui não se digita, para que
        /// "15.90", o erro do lado oposto, seja recusado pelo campo em vez de virar 1590 também.
        /// </summary>
        public static readonly CultureInfo PtBr = Create();

        #endregion

        #region Methods

        private static CultureInfo Create()
        {
            var culture = (CultureInfo)CultureInfo.GetCultureInfo("pt-BR").Clone();

            culture.NumberFormat.NumberGroupSeparator = " ";
            culture.NumberFormat.CurrencyGroupSeparator = " ";

            return culture;
        }

        #endregion
    }
}
