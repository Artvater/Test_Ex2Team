namespace Quantum.Ex2Team
{
    using Photon.Deterministic;
    using UnityEngine.Scripting;

    [Preserve]
    public unsafe class CubeMassControlSystem : SystemMainThreadFilter<CubeMassControlSystem.Filter>
    {

        public override void Update(Frame frame, ref Filter filter) {
            var nearCube = filter.InteractionState->CubeEntity;
            if (nearCube == EntityRef.None)
                return;


            var input = frame.GetPlayerInput(filter.Link->Player);

            if (input->MassAdjustment <= FP._0)
                return;


            if (frame.Unsafe.TryGetPointer<PushableObject>(nearCube, out var pushable) &&
                frame.Unsafe.TryGetPointer<PhysicsBody3D>(nearCube, out var body)) {

                FP newMass = FPMath.Clamp(input->MassAdjustment, pushable->MinMass, pushable->MaxMass);

                pushable->CurrentMass = newMass;
                body->Mass = newMass;
            }
        }

        public struct Filter
        {
            public EntityRef Entity;
            public PlayerLink* Link;
            public PlayerInteractionState* InteractionState;
        }
    }
}
