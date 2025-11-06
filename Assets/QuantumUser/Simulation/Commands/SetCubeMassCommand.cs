namespace Quantum
{
    using Photon.Deterministic;

    public unsafe class SetCubeMassCommand : DeterministicCommand
    {

        public EntityRef CubeEntity;
        public FP NewMass;

        public override void Serialize(BitStream stream) 
        {
            stream.Serialize(ref CubeEntity.Index);
            stream.Serialize(ref CubeEntity.Version);
            stream.Serialize(ref NewMass.RawValue);
        }

        public void Execute(Frame f)
        {
            UnityEngine.Debug.Log("commmmmmmmaaqaaaaand");
            if (f.Unsafe.TryGetPointer<PushableObject>(CubeEntity, out var pushable) &&
                f.Unsafe.TryGetPointer<PhysicsBody3D>(CubeEntity, out var body)) {
                FP clampedMass = FPMath.Clamp(NewMass, pushable->MinMass, pushable->MaxMass);

                pushable->CurrentMass = clampedMass;
                body->Mass = clampedMass;
            }
        }
    }
}
