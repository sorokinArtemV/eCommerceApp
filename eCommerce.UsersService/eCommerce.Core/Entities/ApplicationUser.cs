namespace eCommerce.Core.Entities;

/// <summary>
/// Defined Application User which acts as an entity model
/// </summary>
public sealed class ApplicationUser
{
    public Guid UserId { get; set; }
    public string? Email { get; set; }
    public string? Password { get; set; }
    public string? PersonName { get; set; }
    public string? Gender { get; set; }
}