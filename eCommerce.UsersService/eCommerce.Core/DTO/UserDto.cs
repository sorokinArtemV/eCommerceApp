namespace eCommerce.Core.DTO;

public record UserDto(
    Guid UserId,
    string? Email,
    string? PersonName,
    string Gender
);