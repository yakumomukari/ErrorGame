using UnityEngine;

public interface IExplosionReceiver
{
    void ReceiveExplosion(Vector2 explosionOrigin, float explosionRadius);
}
