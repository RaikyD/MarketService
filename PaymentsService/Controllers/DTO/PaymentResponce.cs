namespace PaymentsService.Controllers.DTO;

public sealed record PaymentResponce(
    Guid UserId,
    decimal Amount
    );