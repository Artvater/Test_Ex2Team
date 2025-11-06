namespace Quantum.Ex2Team
{
    using System.Collections;
    using Photon.Deterministic;
    using UnityEngine;
    using UnityEngine.UI;
    using TMPro;

    public unsafe class GameplayUI : MonoBehaviour
    {
        [Header("UI Elements")] 
        [SerializeField] private Slider massSlider;
        [SerializeField] private TextMeshProUGUI massValueText;
        [SerializeField] private TextMeshProUGUI minMassValueText;
        [SerializeField] private TextMeshProUGUI maxMassValueText;
        [SerializeField] private TextMeshProUGUI levelFinishText;

        [Header("Reference")] 
        [SerializeField] private QuantumDebugInput inputProvider;
        [SerializeField] private CanvasGroup massControlCanvasGroup;
        [SerializeField] private CanvasGroup finishCanvasGroup;

        [Header("Settings")]
        [SerializeField] private float finishDisplayDuration = 2.5f;
        [SerializeField] private float fadeSpeed = 3f;

        private QuantumGame _game;
        private EntityRef _localPlayerEntity;
        private EntityRef _currentCube;
        private bool _isVisible;
        private bool _isUpdatingSlider;
        private float _pendingMassValue;
        private bool _hasPendingValue;
        private bool _isGameReady;
        private bool _isUIInitialized;
        private PlayerRef _cachedLocalPlayer = PlayerRef.None;

        private const float HideCGAlpha = 0f;
        private const float showCGAlpha = 1f;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);

            if (massControlCanvasGroup != null)
                massControlCanvasGroup.alpha = HideCGAlpha;

            if (finishCanvasGroup != null)
                finishCanvasGroup.alpha = HideCGAlpha;

            if (massSlider != null)
                massSlider.onValueChanged.AddListener(OnMassSliderChanged);

#if UNITY_STANDALONE
            SetupSliderDesktop();
#endif
        }

        private void OnEnable()
        {
            QuantumEvent.Subscribe(listener: this, handler: (EventOnStartPushing e) => ShowUI(e));
            QuantumEvent.Subscribe(listener: this, handler: (EventOnEndPushing e) => HideUI(e));
            QuantumEvent.Subscribe(listener: this, handler: (EventMapTransition e) => OnMapTransitionEvent(e));

            QuantumCallback.Subscribe(this, (CallbackGameStarted callback) => OnQuantumGameStarted(callback));
            QuantumCallback.Subscribe(this, (CallbackGameDestroyed callback) => OnQuantumGameDestroyed(callback));
        }

        private void OnDisable()
        {
            QuantumEvent.UnsubscribeListener(this);
            QuantumCallback.UnsubscribeListener(this);
        }

        private void Start()
        {
            _game = QuantumRunner.Default?.Game;
            _isGameReady = _game != null;
        }

        private void OnDestroy() {
            if (massSlider != null)
                massSlider.onValueChanged.RemoveListener(OnMassSliderChanged);
        }

        private void Update()
        {
            if (_hasPendingValue && inputProvider != null)
            {
                inputProvider.SetMassValue(_pendingMassValue);
                _hasPendingValue = false;
            }

            if (!_isGameReady || _game == null)
                return;

            var frame = _game.Frames.Predicted;
            if (frame == null)
                return;

            if (_localPlayerEntity == EntityRef.None) {
                _localPlayerEntity = FindLocalPlayerEntity(frame);
            }

            if (_isVisible && _currentCube != EntityRef.None) {
                SyncSliderWithQuantum(frame);
            }
        }

        private void OnQuantumGameStarted(CallbackGameStarted callback)
        {
            _game = callback.Game;
            _isGameReady = true;

            _localPlayerEntity = EntityRef.None;
            _cachedLocalPlayer = PlayerRef.None;
        }

        private void OnQuantumGameDestroyed(CallbackGameDestroyed callback) {
            _game = null;
            _isGameReady = false;
            _localPlayerEntity = EntityRef.None;
            _cachedLocalPlayer = PlayerRef.None;
        }

        private void OnMapTransitionEvent(EventMapTransition e)
        {
            if (_game == null)
                return;

            var frame = _game.Frames.Predicted;
            if (frame == null)
                return;

            var winnerData = frame.GetPlayerData(e.Winner);

            var targetMap = frame.FindAsset(e.TargetMap);
            if (targetMap == null) {
                Debug.LogError($"Target map not found: {e.TargetMap}");
                return;
            }

            if (finishCanvasGroup != null && levelFinishText != null)
            {
                finishCanvasGroup.alpha = showCGAlpha;
                levelFinishText.text =
                    $"We have a winner!\n<color=green>{winnerData.PlayerNickname}</color>\n Next level - <color=yellow>{targetMap.Scene}</color>";

                StartCoroutine(ShowFinishMessage());
            }
        }

        private IEnumerator ShowFinishMessage() {
            if (finishCanvasGroup == null)
                yield break;

            float elapsed = 0f;
            finishCanvasGroup.alpha = showCGAlpha;

            yield return new WaitForSeconds(finishDisplayDuration); 

            while (elapsed < 1f / fadeSpeed) {
                elapsed += Time.deltaTime;
                finishCanvasGroup.alpha = Mathf.Lerp(showCGAlpha, HideCGAlpha, elapsed * fadeSpeed);
                yield return null;
            }
            finishCanvasGroup.alpha = HideCGAlpha;
        }

        private void SetupSliderDesktop()
        {
            if (massSlider == null)
                return;

            var nav = massSlider.navigation;
            nav.mode = Navigation.Mode.None;
            massSlider.navigation = nav;
        }

        private void ShowUI(EventOnStartPushing e)
        {
            if (!_isGameReady || _game == null) 
                return;

            if (!IsLocalPlayer(e.Entity))
                return;

            if (_isVisible) 
                return;

            var frame = _game.Frames.Predicted;
            if (frame == null) return;

            _isVisible = true;
            _currentCube = e.OtherObject;

            if (frame.Unsafe.TryGetPointer<PushableObject>(e.OtherObject, out var pushable))
            {
                _isUpdatingSlider = true;

                if (!_isUIInitialized)
                {
                    InitializeUI(pushable);
                }

                massSlider.value = pushable->CurrentMass.AsFloat;
                UpdateMassText(pushable->CurrentMass.AsFloat);

                _isUpdatingSlider = false;
            }

            if (massControlCanvasGroup != null)
            {
                massControlCanvasGroup.alpha = showCGAlpha;
            }
        }

        private void InitializeUI(PushableObject* pushable)
        {
            if (_isUIInitialized)
                return;

            _isUIInitialized = true;

            if (massSlider != null)
            {
                massSlider.minValue = pushable->MinMass.AsFloat;
                massSlider.maxValue = pushable->MaxMass.AsFloat;
            }

            if (minMassValueText != null)
                minMassValueText.text = pushable->MinMass.AsFloat.ToString("F0");
            
            if (maxMassValueText != null)
                maxMassValueText.text = pushable->MaxMass.AsFloat.ToString("F0");
        }

        private void HideUI(EventOnEndPushing e) {
            if (!IsLocalPlayer(e.Entity))
                return;

            _isVisible = false;
            _hasPendingValue = false;
            _currentCube = EntityRef.None;

            if (massControlCanvasGroup != null)
                massControlCanvasGroup.alpha = HideCGAlpha;
        }

        private void SyncSliderWithQuantum(Frame frame)
        {
            if (_isUpdatingSlider || massSlider == null)
                return;

            if (!frame.Unsafe.TryGetPointer<PushableObject>(_currentCube, out var pushable))
                return;
            
            float quantumMass = pushable->CurrentMass.AsFloat;

            if (Mathf.Abs(massSlider.value - quantumMass) > 1f) {
                _isUpdatingSlider = true;
                massSlider.value = quantumMass;
                UpdateMassText(quantumMass);
                _isUpdatingSlider = false;
            }
        }

        private void OnMassSliderChanged(float value) {
            if (_isUpdatingSlider)
                return;

            if (!_isGameReady || _game == null || _currentCube == EntityRef.None)
                return;

            UpdateMassText(value);
            SendMassChangeCommand(value);

            _pendingMassValue = value;
            _hasPendingValue = true;
        }

        private void SendMassChangeCommand(float newMass) {
            var command = new SetCubeMassCommand
            {
                CubeEntity = _currentCube,
                NewMass = FP.FromFloat_UNSAFE(newMass)
            };

            _game.SendCommand(command);
        }

        private void UpdateMassText(float mass)
        {
            if (massValueText != null)
                massValueText.text = $"{mass:F1}";
        }

        private bool IsLocalPlayer(EntityRef playerEntity) {
            if (playerEntity == EntityRef.None)
                return false;

            if (_localPlayerEntity == EntityRef.None) {
                var frame = _game?.Frames.Predicted;
                if (frame != null)
                {
                    _localPlayerEntity = FindLocalPlayerEntity(frame);
                }
            }

            return playerEntity == _localPlayerEntity;
        }

        private EntityRef FindLocalPlayerEntity(Frame frame)
        {
            if (_game == null)
                return EntityRef.None;

            var localPlayers = _game.GetLocalPlayers();
            if (localPlayers == null || localPlayers.Count == 0)
                return EntityRef.None;

            if (_cachedLocalPlayer == PlayerRef.None) {
                _cachedLocalPlayer = localPlayers[0];
            }

            var players = frame.Filter<PlayerLink>();
            while (players.Next(out var entity, out var link))
            {
                if (link.Player == _cachedLocalPlayer)
                    return entity;
            }

            return EntityRef.None;
        }
    }
}
