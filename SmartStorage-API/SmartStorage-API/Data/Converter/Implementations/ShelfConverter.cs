using SmartStorage_API.Data.Converter.Contract;
using SmartStorage_API.Data.VO;
using SmartStorage_API.Model;

namespace SmartStorage_API.Data.Converter.Implementations
{
    public class ShelfConverter : IParser<ShelfVO, Shelf>, IParser<Shelf, ShelfVO>
    {
        public Shelf Parse(ShelfVO origin)
        {
            if (origin == null)
                return null;

            return new Shelf
            {
                Id = origin.Id,
                Name = origin.Name,
                DataRegister = origin.DataRegister
            };
        }

        public ShelfVO Parse(Shelf origin)
        {
            if (origin == null)
                return null;

            return new ShelfVO
            {
                Id = origin.Id,
                Name = origin.Name,
                DataRegister = origin.DataRegister
            };
        }

        public List<Shelf> Parse(List<ShelfVO> origin)
        {
            if (origin == null)
                return null;

            return origin.Select(item => Parse(item)).ToList();
        }

        public List<ShelfVO> Parse(List<Shelf> origin)
        {
            if (origin == null)
                return null;

            return origin.Select(item => Parse(item)).ToList();
        }
    }
}
