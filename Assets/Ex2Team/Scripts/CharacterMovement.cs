namespace Quantum.TestEx2Team
{
    using Photon.Deterministic;
    
    public unsafe class CharacterMovementSystem : SystemMainThreadFilter<CharacterMovementSystem.Filter>
    {

        public struct Filter
        {
            public EntityRef Entity;
            public CharacterController3D* CharacterController;
            public Transform3D* Transform;
        }

        public override void Update(Frame f, ref Filter filter)
        {
            filter.CharacterController->Move(f, filter.Entity, FPVector3.Forward);
        }
    }
}
