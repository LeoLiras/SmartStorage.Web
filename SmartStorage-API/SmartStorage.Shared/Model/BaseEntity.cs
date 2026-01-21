using System.ComponentModel.DataAnnotations.Schema;

namespace SmartStorage_Shared.Model
{
    public class BaseEntity
    {
        [Column("id")]
        public long Id { get; set; }
    }
}
