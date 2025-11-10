using SmartStorage_Shared.Model;
using System.ComponentModel.DataAnnotations;

namespace SmartStorage_Shared.VO
{
    public partial class SaleVO
    {
        public string? ProductName { get; set; }

        public int ProductId { get; set; }

        public string? ShelfName { get; set; }

        public decimal EnterPrice { get; set; }

        public decimal SaleTotal { get; set; }

        public int Id { get; set; }

        [Required(ErrorMessage = "O ID da entrada relativa a venda é obrigatório.")]
        public int IdEnter { get; set; }

        [Required(ErrorMessage = "A quantidade da venda é obrigatória.")]
        public int Qntd { get; set; }

        [Required(ErrorMessage = "A data da venda é obrigatória.")]
        public DateTime DateSale { get; set; }

        public static Sale Parse(SaleVO origin)
        {
            if (origin == null)
                return null;

            return new Sale
            {
                SalId = origin.Id,
                SalEntId = origin.IdEnter,
                SalQntd = origin.Qntd,
                SalDateSale = origin.DateSale
            };
        }

        public static List<Sale> ParseList(List<SaleVO> origin)
        {
            if (origin == null)
                return null;

            return origin.Select(item => Parse(item)).ToList();
        }
    }
}
