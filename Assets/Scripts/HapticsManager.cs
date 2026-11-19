using System.Collections;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Inputs.Haptics;

/// <summary>
/// Central place for every haptic "pattern" in the piano regulation app.
/// Designers tune the values in the Inspector; programmers just call
/// HapticsManager.Instance.PlayCorrect() (etc.) from gameplay code.
///
/// Setup:
/// 1. Put this on an empty GameObject in the scene (e.g. "Haptics").
/// 2. Drag the Haptic Impulse Player from the Left Controller and the
///    Right Controller (under XR Origin) into the two slots below.
/// 3. Tune each pattern while testing in the headset.
/// </summary>
public class HapticsManager : MonoBehaviour
{
    public static HapticsManager Instance { get; private set; }

    public enum Hand { Left, Right, Both }

    [System.Serializable]
    public struct Pulse
    {
        [Range(0f, 1f)] public float amplitude;   // strength
        [Min(0f)] public float duration;          // seconds
        [Min(0f)] public float frequency;         // Hz, 0 = device default (OpenXR only)

        public Pulse(float a, float d, float f = 0f) { amplitude = a; duration = d; frequency = f; }
    }

    [Header("Controllers")]
    [SerializeField] HapticImpulsePlayer leftHand;
    [SerializeField] HapticImpulsePlayer rightHand;
    [Tooltip("Hand used by the no-argument methods (handy for Inspector events).")]
    [SerializeField] Hand defaultHand = Hand.Right;

    [Header("UI / Quiz patterns")]
    [SerializeField] Pulse hover     = new Pulse(0.10f, 0.03f);
    [SerializeField] Pulse select    = new Pulse(0.40f, 0.06f);
    [Tooltip("Played twice with a short gap: a light 'da-dum'.")]
    [SerializeField] Pulse correct   = new Pulse(0.50f, 0.06f);
    [SerializeField] float correctGap = 0.08f;
    [SerializeField] Pulse incorrect = new Pulse(0.70f, 0.25f, 60f);

    [Header("Piano interaction patterns")]
    [Tooltip("Tiny click, e.g. each increment of turning a regulating screw.")]
    [SerializeField] Pulse tick       = new Pulse(0.25f, 0.015f);
    [Tooltip("Firm bump, e.g. a key hitting bottom / a part reaching its stop.")]
    [SerializeField] Pulse bottomOut  = new Pulse(0.60f, 0.04f);

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    // ---- No-argument versions: can be wired up in the Inspector (UnityEvents) ----
    public void PlayHover()     => Play(hover, defaultHand);
    public void PlaySelect()    => Play(select, defaultHand);
    public void PlayCorrect()   => PlayCorrect(defaultHand);
    public void PlayIncorrect() => Play(incorrect, defaultHand);
    public void PlayTick()      => Play(tick, defaultHand);
    public void PlayBottomOut() => Play(bottomOut, defaultHand);

    // ---- Versions for programmers who know which hand did the action ----
    public void PlayHover(Hand h)     => Play(hover, h);
    public void PlaySelect(Hand h)    => Play(select, h);
    public void PlayIncorrect(Hand h) => Play(incorrect, h);
    public void PlayTick(Hand h)      => Play(tick, h);
    public void PlayBottomOut(Hand h) => Play(bottomOut, h);

    public void PlayCorrect(Hand h) => StartCoroutine(DoublePulse(correct, correctGap, h));

    /// <summary>Play any custom pulse — useful for quick experiments.</summary>
    public void Play(Pulse p, Hand h)
    {
        if (h != Hand.Right) Send(leftHand, p);
        if (h != Hand.Left)  Send(rightHand, p);
    }

    IEnumerator DoublePulse(Pulse p, float gap, Hand h)
    {
        Play(p, h);
        yield return new WaitForSeconds(p.duration + gap);
        Play(p, h);
    }

    static void Send(HapticImpulsePlayer player, Pulse p)
    {
        if (player == null) return;
        player.SendHapticImpulse(p.amplitude, p.duration, p.frequency);
    }
}
