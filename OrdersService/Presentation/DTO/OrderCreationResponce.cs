using OrdersService.Domain.Entities;

namespace OrdersService.Presentation.DTO;

public record OrderCreationResponce(
    Guid Id,
    Guid UserId,
    decimal Amount,
    string? Description
    );