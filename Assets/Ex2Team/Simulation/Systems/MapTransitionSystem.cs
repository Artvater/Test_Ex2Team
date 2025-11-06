namespace Quantum.Ex2Team
{
    using Photon.Deterministic;
    using Quantum;

    public unsafe class MapTransitionSystem : SystemSignalsOnly, ISignalOnTriggerEnter3D
    {

        private bool _isWinnerExist;

        public void OnTriggerEnter3D(Frame frame, TriggerInfo3D info) {
            if (!frame.Has<FinishTrigger>(info.Entity))
                return;

            if (!frame.Has<PlayerLink>(info.Other))
                return;

            var triggerEntity = info.Entity;
            var playerEntity = info.Other;

            if (frame.Unsafe.TryGetPointer<PlayerInTrigger>(playerEntity, out var playerTrigger)) {
                playerTrigger->CurrentTrigger = triggerEntity;
            }

            CheckMapTransition(frame, playerEntity, triggerEntity);
        }

        private void CheckMapTransition(Frame frame, EntityRef playerEntity, EntityRef triggerEntity) {
            if (!frame.Unsafe.TryGetPointer<FinishTrigger>(triggerEntity, out var trigger))
                return;

            int playersInTrigger = CountPlayersInTrigger(frame, triggerEntity);
            int totalPlayers = CountTotalPlayers(frame);

            if (!frame.Unsafe.TryGetPointerSingleton<Ex2TeamGameplay>(out var gameplay))
                return;

            var player = frame.Unsafe.GetPointer<PlayerLink>(playerEntity)->Player;

            if (!_isWinnerExist) {
                gameplay->Winner = player;
                _isWinnerExist = true;
            }

            bool shouldTransition = trigger->RequireAllPlayers
                ? (playersInTrigger >= totalPlayers && totalPlayers > 0)
                : (playersInTrigger > 0);

            if (shouldTransition)
            {
                var targetMap = frame.FindAsset(trigger->TargetMap);

                if (targetMap != null)
                {
                    frame.Events.MapTransition(gameplay->Winner, trigger->TargetMap);
                    frame.Map = targetMap;

                    var spawnPosition = gameplay->SpawnPosition;
                    SetPlayersPosition(frame, spawnPosition);

                    _isWinnerExist = false;
                }
                else
                {
                    UnityEngine.Debug.Log("Target Map not found!");
                }
            }
        }

        private int CountPlayersInTrigger(Frame frame, EntityRef triggerEntity) {
            int count = 0;

            var players = frame.Filter<PlayerLink, PlayerInTrigger>();
            while (players.Next(out var entity, out var link, out var inTrigger)) {
                if (inTrigger.CurrentTrigger == triggerEntity)
                    count++;
            }

            return count;
        }

        private void SetPlayersPosition(Frame frame, FPVector3 spawnPosition) {
            var players = frame.Filter<PlayerLink, PlayerInTrigger>();
            while (players.Next(out var entity, out var link, out var inTrigger)) {
                if (frame.Unsafe.TryGetPointer<Transform3D>(entity, out var transform)) {
                    transform->Position = new FPVector3(spawnPosition.X, spawnPosition.Y, link.Player * spawnPosition.Z);
                    transform->Rotation = FPQuaternion.Identity;
                }
            }
        }

        private int CountTotalPlayers(Frame f) {
            int count = 0;
            var players = f.Filter<PlayerLink>();
            while (players.Next(out var entity, out var link))
            {
                count++;
            }

            return count;
        }
    }
}

