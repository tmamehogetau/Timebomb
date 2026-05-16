using FishNet.Object.Prediction;
using UnityEngine;

namespace Rounds2.Player
{
    public struct PlayerReplicateData : IReplicateData
    {
        public Vector2 Move;
        public Vector2 Aim;
        public bool Fire;
        private uint tick;

        public PlayerReplicateData(Vector2 move, Vector2 aim, bool fire)
        {
            Move = move;
            Aim = aim;
            Fire = fire;
            tick = 0;
        }

        public void Dispose() { }
        public readonly uint GetTick() => tick;
        public void SetTick(uint value) => tick = value;
    }

    public struct PlayerReconcileData : IReconcileData
    {
        public PredictionRigidbody2D Body;
        public Vector2 Aim;
        public Vector2 MoveVelocity;
        private uint tick;

        public PlayerReconcileData(PredictionRigidbody2D body, Vector2 aim, Vector2 moveVelocity)
        {
            Body = body;
            Aim = aim;
            MoveVelocity = moveVelocity;
            tick = 0;
        }

        public void Dispose() { }
        public readonly uint GetTick() => tick;
        public void SetTick(uint value) => tick = value;
    }
}
