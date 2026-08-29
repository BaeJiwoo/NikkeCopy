namespace NikkeCopy.Domain.Players;

public sealed class Player
{
    public long Id { get; private set; }

    public string Name { get; private set; } = string.Empty;

    public DateTime CreatedAt { get; private set; }

    private Player()
    {

    }

    public Player(string name)
    {
        Name = name;
        CreatedAt = DateTime.UtcNow;
    }
}