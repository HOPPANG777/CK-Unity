using UnityEngine;

public interface IPlayerInput
{
    Vector2 Move { get; }

    bool StartListening();

    void StopListening();

    bool TryConsumeFireRequest();
}
