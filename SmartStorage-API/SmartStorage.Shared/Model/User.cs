using SmartStorage.Shared.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartStorage_Shared.Model;

[Table("User", Schema = "dbo")]
public class User : BaseEntity
{
    [Column("UseUsername")]
    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [Column("UseFullName")]
    [Required]
    [StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Column("UsePassword")]
    [Required]
    [StringLength(130)]
    public string Password { get; set; } = string.Empty;

    [Column("UseRefreshToken")]
    [StringLength(500)]
    public string? RefreshToken { get; set; }

    [Column("UseRefreshTokenExpiryTime")]
    public DateTime? RefreshTokenExpiryTime { get; set; }

    [Column("UseType")]
    [Required]
    public TipoUsuario UseType { get; set; }
}
