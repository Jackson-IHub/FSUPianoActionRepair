using System.Collections;
using TMPro;
using UnityEngine;

public class TooltipHelper : MonoBehaviour
{
    [SerializeField] private string toolName;

    [TextArea(3, 10)]
    [SerializeField] private string toolDescription;

    [SerializeField] private Tool tool;

    [SerializeField] private GameObject tooltipObject;
    [SerializeField] private Canvas tooltipCanvas;
    [SerializeField] private TextMeshProUGUI nameLabel;
    [SerializeField] private TextMeshProUGUI descriptionLabel;

    [Header("Fade")]
    [SerializeField] private float fadeInDuration = 0.25f;
    [SerializeField] private float fadeOutDuration = 0.25f;

    private CanvasGroup canvasGroup;
    private Coroutine fadeRoutine;

    void Awake()
    {
        // Reuse a CanvasGroup on the canvas if there is one, otherwise add it.
        canvasGroup = tooltipCanvas.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = tooltipCanvas.gameObject.AddComponent<CanvasGroup>();
        }
    }

    void Start()
    {
        nameLabel.text = toolName;
        descriptionLabel.text = toolDescription;

        // Start fully hidden.
        canvasGroup.alpha = 0f;
        tooltipObject.SetActive(false);
    }

    public void ShowTooltip()
    {
        if (tool.IsAtReturnPosition())
        {
            tooltipObject.SetActive(true);
            StartFade(1f, fadeInDuration, false);
        }
    }

    public void HideToolTip()
    {
        // Nothing to fade if it's already hidden.
        if (!tooltipObject.activeSelf) return;

        StartFade(0f, fadeOutDuration, true);
    }

    private void StartFade(float targetAlpha, float duration, bool deactivateWhenDone)
    {
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
        }
        fadeRoutine = StartCoroutine(FadeRoutine(targetAlpha, duration, deactivateWhenDone));
    }

    private IEnumerator FadeRoutine(float targetAlpha, float duration, bool deactivateWhenDone)
    {
        float startAlpha = canvasGroup.alpha;

        // Scale duration by remaining distance so interrupted fades stay smooth.
        float effectiveDuration = duration * Mathf.Abs(targetAlpha - startAlpha);
        float elapsed = 0f;

        while (elapsed < effectiveDuration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.SmoothStep(0f, 1f, elapsed / effectiveDuration);
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, t);
            yield return null;
        }

        canvasGroup.alpha = targetAlpha;

        if (deactivateWhenDone)
        {
            tooltipObject.SetActive(false);
        }

        fadeRoutine = null;
    }

    void OnDisable()
    {
        // If this helper is disabled mid-fade, don't leave the tooltip half visible.
        if (fadeRoutine != null)
        {
            StopCoroutine(fadeRoutine);
            fadeRoutine = null;
        }
    }
}