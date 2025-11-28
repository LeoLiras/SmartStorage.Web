using SmartStorage_Shared.Model;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace SmartStorage_Shared.VO
{
    public partial class EnterVO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "A seleção do produto é obrigatória.")]
        public int ProductId { get; set; }

        public string? ProductName { get; set; }

        [Required(ErrorMessage = "A quantidade da entrada é obrigatória.")]
        [DefaultValue(typeof(int), "0")]
        public int ProductQuantity { get; set; }

        [Required(ErrorMessage = "O preço da entrada é obrigatório.")]
        public decimal ProductPrice { get; set; }

        [Required(ErrorMessage = "A seleção da prateleira é obrigatória.")]
        public int ShelfId { get; set; }

        public string? ShelfName { get; set; }

        [Required(ErrorMessage = "A data da entrada é obrigatória.")]
        public DateTime DateEnter { get; set; }

        public static Enter? Parse(EnterVO origin)
        {
            if (origin == null)
                return null;

            return new Enter
            {
                EntId = origin.Id,
                EntProId = origin.ProductId,
                EntQntd = origin.ProductQuantity,
                EntPrice = origin.ProductPrice,
                EntSheId = origin.ShelfId,
                EntDateEnter = origin.DateEnter,
            };
        }

        public static List<Enter?>? ParseList(List<EnterVO> origin)
        {
            if (origin == null)
                return null;

            return origin.Select(item => Parse(item)).ToList();
        }
    }
}
