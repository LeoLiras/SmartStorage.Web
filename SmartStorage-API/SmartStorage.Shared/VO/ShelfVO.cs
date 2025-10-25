using SmartStorage_Shared.Model;

namespace SmartStorage_Shared.VO
{
    public partial class ShelfVO
    {
        public int Id { get; set; }

        public string? Name { get; set; }

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
