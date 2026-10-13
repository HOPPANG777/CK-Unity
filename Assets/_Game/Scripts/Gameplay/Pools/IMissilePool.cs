public interface IMissilePool : IPlayerFire
{
    int ActiveCount { get; }
    void StopAll();
}
