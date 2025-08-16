using System;
using UnityEngine;

public static class CollisionHandler
{
    public static void HandleCollisionByLayer(
        Collision2D collision,
        LayerMask playerLayer,
        LayerMask duckLayer,
        Action<PlayerController> onPlayerCollision,
        Action<DuckController> onDuckCollision)
    {

        int otherLayer = collision.gameObject.layer;
        int otherMask = 1 << otherLayer;


        if ((playerLayer.value & otherMask) != 0)
        {
            var player = collision.gameObject.GetComponent<PlayerController>();
            if (player != null)
            {
                onPlayerCollision?.Invoke(player);
                return;
            }
        }

        if ((duckLayer.value & otherMask) != 0)
        {
            var duck = collision.gameObject.GetComponent<DuckController>();
            if (duck != null)
            {
                onDuckCollision?.Invoke(duck);
                return;
            }
        }
    }
}
