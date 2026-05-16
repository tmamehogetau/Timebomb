using UnityEngine;

namespace Rounds2.Player
{
    public static class PlayerRoundReset
    {
        public static void Apply(Transform transform, Rigidbody2D body, Vector3 position, Quaternion rotation)
        {
            if (body != null)
            {
                body.linearVelocity = Vector2.zero;
                body.angularVelocity = 0f;
                body.position = position;
                body.rotation = rotation.eulerAngles.z;
                return;
            }

            transform.SetPositionAndRotation(position, rotation);
        }
    }
}
