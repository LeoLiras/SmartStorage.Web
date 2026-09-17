using SmartStorage_API.Data.Converter.Contract;
using SmartStorage_API.Model.Context;
using SmartStorage_Shared.Model;
using SmartStorage_Shared.VO;

namespace SmartStorage_API.Data.Converter.Implementations
{
    public class ShelfConverter : IParser<ShelfVO, Shelf>, IParser<Shelf, ShelfVO>
    {
        private readonly SmartStorageContext _context;

        public ShelfConverter(SmartStorageContext context)
        {
            _context = context;
        }

        public Shelf Parse(ShelfVO origin)
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

        public ShelfVO Parse(Shelf origin)
        {
            if (origin == null)
                return null;

            return Parse(origin, UsedVolumesOf(new[] { origin.SheId }).GetValueOrDefault(origin.SheId));
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

            var usedVolumes = UsedVolumesOf(origin.Select(s => s.SheId).ToList());

            return origin.Select(item => Parse(item, usedVolumes.GetValueOrDefault(item.SheId))).ToList();
        }

        public decimal UsedVolumeOf(int shelfId)
        {
            return UsedVolumesOf(new[] { shelfId }).GetValueOrDefault(shelfId);
        }

        private Dictionary<int, decimal> UsedVolumesOf(IReadOnlyCollection<int> shelfIds)
        {
            return _context.Enters
                .Where(e => shelfIds.Contains(e.EntSheId) && e.EntQntd > 0)
                .GroupBy(e => e.EntSheId)
                .Select(g => new { ShelfId = g.Key, Used = g.Sum(e => e.EntQntd * (e.Product.ProVolume ?? 0)) })
                .ToDictionary(x => x.ShelfId, x => x.Used);
        }

        private static ShelfVO Parse(Shelf origin, decimal usedVolume)
        {
            return new ShelfVO
            {
                Id = origin.SheId,
                Name = origin.SheName,
                DataRegister = origin.SheDataRegister,
                Volume = origin.SheVolume,
                UsedVolume = usedVolume
            };
        }
    }
}
