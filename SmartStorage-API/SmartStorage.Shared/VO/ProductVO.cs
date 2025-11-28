using SmartStorage_Shared.Model;
using System.ComponentModel.DataAnnotations;

namespace SmartStorage_Shared.VO
{
    public partial class ProductVO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome do produto é obrigatório.")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Insira no mínimo 5 caracteres.")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "A descrição do produto é obrigatória.")]
        [StringLength(300, MinimumLength = 5, ErrorMessage = "Insira no mínimo 5 caracteres.")]
        public string? Descricao { get; set; }

        [Required(ErrorMessage = "A data de registro do produto é obrigatória.")]
        public DateTime DateRegister { get; set; }

        [Required(ErrorMessage = "A quantidade do produto é obrigatória.")]
        public int Qntd { get; set; }

        public int EmployeeId { get; set; }

        public byte[]? ProImage { get; set; }

        public static Product? Parse(ProductVO origin)
        {
            if (origin == null)
                return null;

            return new Product
            {
                ProId = origin.Id,
                ProName = origin.Name,
                ProDescription = origin.Descricao,
                ProDateRegister = origin.DateRegister,
                ProQntd = origin.Qntd,
                ProEmpId = origin.EmployeeId,
            };
        }

        public static List<Product?>? ParseList(List<ProductVO> origin)
        {
            if (origin == null)
                return null;

            return origin.Select(item => Parse(item)).ToList();
        }
    }
}
