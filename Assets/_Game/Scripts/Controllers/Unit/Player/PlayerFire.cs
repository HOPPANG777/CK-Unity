using UnityEngine;

public class PlayerFire : MonoBehaviour
{
    private IPlayerFire _receiver;

    public void Init(IPlayerFire receiver, Object logContext)
    {
        _receiver = receiver;
    }

    public bool IsConnected
    {
        get
        {
            if (_receiver == null)
            {
                return false;
            }

            // 인터페이스로 보관한 Unity 객체의 파괴 여부도 확인한다.
            if (_receiver is Object)
            {
                Object unityObject = (Object)_receiver;
                if (unityObject == null)
                {
                    return false;
                }
            }

            return true;
        }
    }

    public void SetReceiver(IPlayerFire receiver)
    {
        _receiver = receiver;
    }

    public bool TryFire(Vector2 worldPosition, Vector2 direction)
    {
        if (!IsConnected)
        {
            return false;
        }

        return _receiver.TryFire(worldPosition, direction);
    }
}
