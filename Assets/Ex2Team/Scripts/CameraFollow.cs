namespace Quantum.Ex2Team
{
    using UnityEngine;
    using Quantum;

    public class CameraFollow : QuantumEntityViewComponent<CustomViewContext>
    {
        public Vector3 Offset;
        private bool _local;

        public override void OnActivate(Frame frame)
        {
            var link = frame.Get<PlayerLink>(EntityRef);
            _local = Game.PlayerIsLocal(link.Player);
        }

        public override void OnUpdateView()
        {
            if (_local == false) return;

            ViewContext.MyCamera.transform.position = transform.position + Offset;
            ViewContext.MyCamera.transform.rotation = transform.rotation;
            ViewContext.MyCamera.transform.LookAt(transform);
        }
    }
}