using System.ComponentModel.DataAnnotations.Schema;

namespace SmartStorage_API.Authentication.Repositories.GenericRepository
{
    public class BaseEntity
    {
        [Column("id")]
        public long Id { get; set; }
    }
}
