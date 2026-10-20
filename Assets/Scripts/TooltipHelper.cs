using System;
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

    void Start()
    {
        nameLabel.text = toolName;
        descriptionLabel.text = toolDescription;
    }

    public void ShowTooltip()
    {
        if (tool.IsAtReturnPosition())
        {
            tooltipObject.SetActive(true);
        }
    }

    public void HideToolTip()
    {
        tooltipObject.SetActive(false);
    }
}
