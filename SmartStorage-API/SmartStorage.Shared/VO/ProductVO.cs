using SmartStorage_Shared.Model;

namespace SmartStorage_Shared.VO
{
    public partial class ProductVO
    {
        public int Id { get; set; }

        public string? Name { get; set; }

        public string? Descricao { get; set; }

        public DateTime DateRegister { get; set; }

        public int Qntd { get; set; }

        public int? EmployeeId { get; set; }

        public byte[]? ProImage { get; set; }

        public static Product Parse(ProductVO origin)
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

        public static List<Product> ParseList(List<ProductVO> origin)
        {
            if (origin == null)
                return null;

            return origin.Select(item => Parse(item)).ToList();
        }
    }
}
