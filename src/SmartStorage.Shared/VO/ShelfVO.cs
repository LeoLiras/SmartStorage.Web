using SmartStorage_Shared.HypermediaSupport;
using SmartStorage_Shared.Model;
using System.ComponentModel.DataAnnotations;

namespace SmartStorage_Shared.VO
{
    public partial class ShelfVO : ISupportHyperMedia
    {
        public const decimal UsableFraction = 0.9m;

        public int Id { get; set; }

        [Required(ErrorMessage = "O nome da prateleira é obrigatório.")]
        [StringLength(100, MinimumLength = 5, ErrorMessage = "Insira no mínimo 5 caracteres.")]
        public string Name { get; set; }

        [Required(ErrorMessage = "A data de registro da prateleira é obrigatória.")]
        public DateTime DataRegister { get; set; }

        [Range(0.001, double.MaxValue, ErrorMessage = "O volume da prateleira deve ser maior que zero.")]
        public decimal? Volume { get; set; }

        public decimal UsedVolume { get; set; }

        public decimal? UsableVolume => Volume * UsableFraction;

        public decimal? FreeVolume => UsableVolume - UsedVolume;

        public decimal? Occupancy => UsableVolume > 0 ? UsedVolume / UsableVolume * 100 : null;

        public List<HyperMediaLink> Links { get; set; } = new List<HyperMediaLink>();

        public static Shelf Parse(ShelfVO origin)
        {
            if (origin == null)
                return null;

            return new Shelf
            {
                SheId = origin.Id,
                SheName = origin.Name,
                SheDataRegister = origin.DataRegister,
                SheVolume = origin.Volume
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
