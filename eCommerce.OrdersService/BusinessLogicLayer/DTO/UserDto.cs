namespace BusinessLogicLayer.DTO;

public record UserDto(
    Guid UserID,
    string Email,
    string PersonName,
    string Gender
);