using SmartStorage_Shared.Model;
using System.ComponentModel.DataAnnotations;

namespace SmartStorage_Shared.VO
{
    public partial class ShelfVO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "O nome da prateleira é obrigatório.")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Insira no mínimo 5 caracteres.")]
        public string? Name { get; set; }

        [Required(ErrorMessage = "A data de registro da prateleira é obrigatória.")]
        public DateTime DataRegister { get; set; }

        public static Shelf Parse(ShelfVO origin)
        {
            if (origin == null)
                return null;

            return new Shelf
            {
                SheId = origin.Id,
                SheName = origin.Name,
                SheDataRegister = origin.DataRegister
            };
        }

        public static List<Shelf> ParseList(List<ShelfVO> origin)
        {
            if (origin == null)
                return null;

            return origin.Select(item => Parse(item)).ToList();
        }
    }
}
