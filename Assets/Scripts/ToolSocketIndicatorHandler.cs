using System;
using UnityEngine;

public class ToolSocketIndicatorHandler : MonoBehaviour
{
    public enum HoldCheck
    {
        Tool, Caliper
    }

    [SerializeField] private PlayerState playerState;
    [SerializeField] private HoldCheck holdCheck = HoldCheck.Tool;

    [Header("Tool Indicators")]
    [SerializeField] Material indicatorMaterial;
    [SerializeField] float indicatorFadeDuration = 0.25f;
    [SerializeField, Range(0f, 1f)] float indicatorMaxAlpha = 1f;

    static readonly int BaseColorId = Shader.PropertyToID("_BaseColor"); // URP/HDRP
    static readonly int ColorId = Shader.PropertyToID("_Color");         // Built-in

    int colorPropertyId;
    float currentAlpha;
    bool indicatorsVisible;

    void Awake()
    {
        colorPropertyId = indicatorMaterial.HasProperty(BaseColorId) ? BaseColorId : ColorId;

        currentAlpha = 0f;
        SetAlpha(currentAlpha);

        indicatorsVisible = true; // force the first call to actually apply
        SetIndicatorsVisible(false);
    }

    void Update()
    {
        bool isHolding = IsHolding();

        // Turn children on as soon as the player grabs a tool,
        // so there is something to fade in.
        if (isHolding && !indicatorsVisible)
            SetIndicatorsVisible(true);

        float targetAlpha = isHolding ? indicatorMaxAlpha : 0f;

        if (!Mathf.Approximately(currentAlpha, targetAlpha))
        {
            float fadeSpeed = indicatorFadeDuration > 0f
                ? indicatorMaxAlpha / indicatorFadeDuration
                : float.MaxValue;

            currentAlpha = Mathf.MoveTowards(currentAlpha, targetAlpha, fadeSpeed * Time.deltaTime);
            SetAlpha(currentAlpha);
        }

        // Turn children off only once the fade-out has fully finished.
        if (!isHolding && indicatorsVisible && currentAlpha <= 0f)
            SetIndicatorsVisible(false);
    }

    void OnDisable()
    {
        // Avoid leaving the shared material asset modified after leaving play mode.
        SetAlpha(0.25f);
    }

    bool IsHolding()
    {
        return holdCheck switch
        {
            HoldCheck.Tool => playerState.IsHoldingTool(),
            HoldCheck.Caliper => playerState.IsHoldingCaliper(),
            _ => throw new ArgumentOutOfRangeException()
        };
    }

    void SetIndicatorsVisible(bool visible)
    {
        if (indicatorsVisible == visible)
            return;

        indicatorsVisible = visible;

        for (int i = 0; i < transform.childCount; i++)
        {
            GameObject obj = transform.GetChild(i).gameObject;
            obj.GetComponent<CustomSocket>().SetIndicatorSphereVisible(visible);
        }
    }

    void SetAlpha(float alpha)
    {
        Color c = indicatorMaterial.GetColor(colorPropertyId);
        c.a = alpha;
        indicatorMaterial.SetColor(colorPropertyId, c);
    }
}