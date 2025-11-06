namespace Quantum.Ex2Team
{
    using UnityEngine;
    using Quantum;

    public unsafe class AnimationView : QuantumEntityViewComponent<CustomViewContext>
    {
        private Animator _anim;
        private bool _onPushingContinue;

        private const float MotionSpeed = 1.2f;
        private readonly string PushParameterName = "IsPushing";
        private readonly string SpeedParameterName = "Speed";
        private readonly string MotionSpeedParameterName = "MotionSpeed";
        private readonly string GroundedParameterName = "Grounded";
        private readonly string FallParameterName = "FreeFall";

        public override void OnInitialize()
        {
            _anim = GetComponentInChildren<Animator>();
            QuantumEvent.Subscribe(listener: this, handler: (EventOnStartPushing e) => HandleStartPushingEvent(e));
            QuantumEvent.Subscribe(listener: this, handler: (EventOnEndPushing e) => HandleEndPushingEvent(e));
        }

        void HandleStartPushingEvent(EventOnStartPushing e)
        {
            if (e.Entity != EntityRef)
                return;

            _onPushingContinue = e.IsObjectInFront;
        }

        void HandleEndPushingEvent(EventOnEndPushing e)
        {
            _onPushingContinue = false;
        }

        public override void OnUpdateView()
        {
            var kcc = PredictedFrame.Get<CharacterController3D>(EntityRef);
            var speed = kcc.Velocity.Magnitude.AsFloat;

            _anim.SetBool(GroundedParameterName, kcc.Grounded);

            if (speed > 0 && kcc.Grounded)
            {
                _anim.SetFloat(SpeedParameterName, speed);
                _anim.SetFloat(MotionSpeedParameterName, MotionSpeed);
                _anim.SetBool(FallParameterName, false);
            }
            else if (!kcc.Grounded)
            {
                _anim.SetBool(FallParameterName, true);
                _onPushingContinue = false;
            }
            else
            {
                _anim.SetFloat(SpeedParameterName, 0f);
                _anim.SetBool(FallParameterName, false);
            }

            _anim.SetBool(PushParameterName, _onPushingContinue && speed > 0);

        }
    }
}
