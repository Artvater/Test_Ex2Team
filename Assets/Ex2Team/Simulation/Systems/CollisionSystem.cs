namespace Quantum.Ex2Team
{
    using Photon.Deterministic;

    public unsafe class CollisionSystem : SystemSignalsOnly, ISignalOnCollisionEnter3D, ISignalOnCollision3D, ISignalOnCollisionExit3D
    {
        private static readonly FP PushDotThreshold = FP._0_75;
        private static readonly FP DirectionEpsilon = FP._0_01;

        private EntityRef _localPlayerEntity;

        public void OnCollision3D(Frame f, CollisionInfo3D info) {
            f.Events.OnStartPushing(info.Entity, info.Other, IsObjectInFront(f, info.Entity, info.Other));
        }

        public void OnCollisionEnter3D(Frame f, CollisionInfo3D info) {
            if (f.Unsafe.TryGetPointer<PlayerInteractionState>(info.Entity, out var interactionState)) {
                interactionState->CubeEntity = info.Other;
            }
        }

        public void OnCollisionExit3D(Frame f, ExitInfo3D info) {
            f.Events.OnEndPushing(info.Entity);

            if (f.Unsafe.TryGetPointer<PlayerInteractionState>(info.Entity, out var interactionState)) {
                interactionState->CubeEntity = EntityRef.None;
            }
        }

        private bool IsObjectInFront(Frame f, EntityRef playerEntity, EntityRef otherEntity) {
            if (!f.Unsafe.TryGetPointer<Transform3D>(playerEntity, out var playerTransform))
                return false;

            if (!f.Unsafe.TryGetPointer<Transform3D>(otherEntity, out var otherTransform))
                return false;

            FPVector3 directionToOther = otherTransform->Position - playerTransform->Position;
            directionToOther.Y = 0;

            if (directionToOther.SqrMagnitude < DirectionEpsilon)
                return false;

            directionToOther = directionToOther.Normalized;

            FPVector3 playerForward = playerTransform->Forward;
            playerForward.Y = 0;
            playerForward = playerForward.Normalized;

            FP dotProduct = FPVector3.Dot(playerForward, directionToOther);

            return dotProduct >= PushDotThreshold;
        }
    }
}
