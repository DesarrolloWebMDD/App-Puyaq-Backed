namespace Puyaq.CrossCutting.Time;

public interface IClock
{
    DateTimeOffset UtcNow { get; }
}
