namespace BusinessLogicLayer.DTO;

public record UserDto(
    Guid userID,
    string Email,
    string PersonName,
    string Gender
    );
