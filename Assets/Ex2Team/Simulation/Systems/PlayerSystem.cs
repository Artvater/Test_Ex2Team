namespace Quantum.Ex2Team
{
    using Photon.Deterministic;
    using UnityEngine.Scripting;

    [Preserve]
    public unsafe class PlayerSystem : SystemSignalsOnly, ISignalOnPlayerAdded, ISignalOnPlayerDisconnected
    {
        public void OnPlayerAdded(Frame frame, PlayerRef player, bool firstTime) {
            var runtimePlayer = frame.GetPlayerData(player);
            var entity = frame.Create(runtimePlayer.PlayerAvatar);

            frame.Set(entity, new PlayerLink { Player = player });

            if (frame.Unsafe.TryGetPointer<Transform3D>(entity, out var transform)) {

                if (!frame.Unsafe.TryGetPointerSingleton<Ex2TeamGameplay>(out var gameplay))
                    return;
               
                var startPosition = gameplay->SpawnPosition;

                transform->Position = new FPVector3(startPosition.X, startPosition.Y, player * startPosition.Z);
                transform->Rotation = FPQuaternion.Identity;
            }
        }

        public void OnPlayerDisconnected(Frame frame, PlayerRef player) {
            foreach (var pair in frame.GetComponentIterator<PlayerLink>()) {
                if (pair.Component.Player == player) {
                    frame.Destroy(pair.Entity);
                }
            }
        }
    }
}
