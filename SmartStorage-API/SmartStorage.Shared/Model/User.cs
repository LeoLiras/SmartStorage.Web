using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SmartStorage_Shared.Model;

[Table("users", Schema = "dbo")]
public class User
{
    [Column("id")]
    [Key]
    public long Id { get; set; }

    [Column("user_name")]
    [Required]
    [StringLength(50)]
    public string Username { get; set; } = string.Empty;

    [Column("full_name")]
    [Required]
    [StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    [Column("password")]
    [Required]
    [StringLength(130)]
    public string Password { get; set; } = string.Empty;

    [Column("refresh_token")]
    [StringLength(500)]
    public string? RefreshToken { get; set; }

    [Column("refresh_token_expiry_time")]
    public DateTime? RefreshTokenExpiryTime { get; set; }
}
