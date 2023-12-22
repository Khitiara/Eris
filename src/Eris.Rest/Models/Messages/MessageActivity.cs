namespace Eris.Rest.Models.Messages;

public readonly struct MessageActivity
{
    public required MessageActivityType Type { get; init; }
    public string? PartyId { get; init; }
}