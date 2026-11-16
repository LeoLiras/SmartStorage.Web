using SmartStorage.Shared.Enum;
using SmartStorage_Shared.HypermediaSupport;
using SmartStorage_Shared.Model;

namespace SmartStorage.Shared.VO
{
    // Só sai da API, nunca entra: por isso o Parse vai da entidade para o VO,
    // ao contrário dos outros VOs, e deixa senha e refresh token para trás.
    public partial class UserAccountVO : ISupportHyperMedia
    {
        public long Id { get; set; }

        public string Username { get; set; }

        public string FullName { get; set; }

        public TipoUsuario Type { get; set; }

        public List<HyperMediaLink> Links { get; set; } = new List<HyperMediaLink>();

        public static UserAccountVO Parse(User origin)
        {
            if (origin == null)
                return null;

            return new UserAccountVO
            {
                Id = origin.Id,
                Username = origin.Username,
                FullName = origin.FullName,
                Type = origin.UseType
            };
        }

        public static List<UserAccountVO> ParseList(List<User> origin)
        {
            if (origin == null)
                return null;

            return origin.Select(item => Parse(item)).ToList();
        }
    }
}
