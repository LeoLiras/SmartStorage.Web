using SmartStorage_Shared.Model;

namespace SmartStorage_Shared.VO
{
    public partial class EnterVO
    {
        public int Id { get; set; }

        public int ProductId { get; set; }

        public string? ProductName { get; set; }

        public int ProductQuantity { get; set; }

        public decimal ProductPrice { get; set; }

        public int ShelfId { get; set; }

        public string? ShelfName { get; set; }

        public DateTime DateEnter { get; set; }

        public static Enter Parse(EnterVO origin)
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

        public static List<Enter> Parse(List<EnterVO> origin)
        {
            if (origin == null)
                return null;

            return origin.Select(item => Parse(item)).ToList();
        }
    }
}
