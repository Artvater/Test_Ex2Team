namespace Quantum.Ex2Team
{
    using Photon.Deterministic;

    public unsafe class MovementSystem : SystemMainThreadFilter<MovementSystem.Filter>
    {

        private const int FallingEdgeValue = -7;
        private const int RotationSpeed = 10;
        private static readonly FP DirectionEpsilon = FP._0_01;
        private static readonly FP MagnitudeEpsilon = FP._0_10;

        public override void Update(Frame frame, ref Filter filter)
        {
            var player = filter.Link->Player;
            var input = frame.GetPlayerInput(player);

            var direction = input->Direction;

            if (direction.Magnitude > 1) {
                direction = direction.Normalized;
            }

            var movementVector = direction.XOY;

            if (input->Jump.WasPressed) {
                filter.KCC->Jump(frame);
            }

            filter.KCC->Move(frame, filter.Entity, movementVector);

            if (direction.Magnitude > MagnitudeEpsilon) {
                RotateTowardsMovement(frame, filter.Transform, movementVector);
            }

            if (filter.Transform->Position.Y < FallingEdgeValue) {
                filter.Transform->Position = GetStartSpawnPosition(frame, player);
                filter.Transform->Rotation = FPQuaternion.Identity;
            }
        }

        FPVector3 GetStartSpawnPosition(Frame frame, PlayerRef player) {
            if (!frame.Unsafe.TryGetPointerSingleton<Ex2TeamGameplay>(out var gameplay))
                return FPVector3.One;

            var startPosition = gameplay->SpawnPosition;

            return new FPVector3(startPosition.X, startPosition.Y, player * startPosition.Z);
        }

        private void RotateTowardsMovement(Frame frame, Transform3D* transform, FPVector3 movementDirection)
        {
            if (movementDirection.SqrMagnitude < DirectionEpsilon)
                return;

            var targetRotation = FPQuaternion.LookRotation(movementDirection.Normalized, FPVector3.Up);

            transform->Rotation = FPQuaternion.Slerp(
                transform->Rotation,
                targetRotation,
                frame.DeltaTime * RotationSpeed
            );
        }

        public struct Filter
        {
            public EntityRef Entity;
            public Transform3D* Transform;
            public CharacterController3D* KCC;
            public PlayerLink* Link;
        }
    }
}
