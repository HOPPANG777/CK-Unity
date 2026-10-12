public interface IScreenWorldBounds
{
    bool IsValid { get; }

    bool TryGetBounds(out ScreenWorldBoundsArea bounds);
}
