namespace Quantum {
  using Photon.Deterministic;
  using UnityEngine;

  /// <summary>
  /// A Unity script that creates empty input for any Quantum game.
  /// </summary>
  public class QuantumDebugInput : MonoBehaviour {

    private void Awake() {
      DontDestroyOnLoad(gameObject);
    }

    private void OnEnable() {
      QuantumCallback.Subscribe(this, (CallbackPollInput callback) => PollInput(callback));
    }

    private void OnDisable() {
      QuantumCallback.UnsubscribeListener(this);
    }

    private float _pendingMassValue = -1f;

    /// <summary>
    /// Set an empty input when polled by the simulation.
    /// </summary>
    /// <param name="callback"></param>
    public void PollInput(CallbackPollInput callback) {
#if DEBUG
      if (callback.IsInputSet) {
        Debug.LogWarning($"{nameof(QuantumDebugInput)}.{nameof(PollInput)}: Input was already set by another user script, unsubscribing from the poll input callback. Please delete this component.", this);
        QuantumCallback.UnsubscribeListener(this);
        return;
      }
#endif

      Quantum.Input i = new Quantum.Input();

      float x = UnityEngine.Input.GetAxis("Horizontal");
      float y = UnityEngine.Input.GetAxis("Vertical");

      i.Direction = new FPVector2(-y.ToFP(), x.ToFP());
      i.Jump = UnityEngine.Input.GetButton("Jump");

      if (_pendingMassValue >= 0) { 
        i.MassAdjustment = FP.FromFloat_UNSAFE(_pendingMassValue); 
        _pendingMassValue = -1f; 
      } else {
        i.MassAdjustment = FP._0;
      }

      callback.SetInput(i, DeterministicInputFlags.Repeatable);
    }

    public void SetMassValue(float mass) {
      _pendingMassValue = mass;
    }
  }
}
