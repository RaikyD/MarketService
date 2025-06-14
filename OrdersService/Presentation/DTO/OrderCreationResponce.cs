using OrdersService.Domain.Entities;

namespace OrdersService.Presentation.DTO;

public sealed record OrderCreationResponce (
    Guid Id,
    Guid UserId,
    decimal Amount,
    string? Description
    );