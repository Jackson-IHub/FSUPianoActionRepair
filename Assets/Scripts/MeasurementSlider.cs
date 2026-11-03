using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MeasurementSlider : MonoBehaviour
{
    [SerializeField] private Slider slider;
    [SerializeField] private TextMeshProUGUI valueLabel;
    [SerializeField] private Button decreaseButton;
    [SerializeField] private Button increaseButton;

    private float increment = 0.1f;

    private float sliderLeftX;
    private float sliderRightX;

    public float getValue()
    {
        // Return the value of the slider rounded to the nearest increment
        return Mathf.Round(slider.value / increment) * increment;
    }

    public void setBounds(float lower, float upper, float increment)
    {
        slider.minValue = lower;
        slider.maxValue = upper;
        this.increment = increment;

        slider.value = (lower + upper) / 2f; // Set to midpoint

        OnSliderValueChanged(slider.value);
    }

    void Start()
    {
        slider.onValueChanged.AddListener(OnSliderValueChanged);
        decreaseButton.onClick.AddListener(OnDecrease);
        increaseButton.onClick.AddListener(OnIncrease);

        RectTransform sliderRect = slider.handleRect.parent as RectTransform;
        Vector3[] corners = new Vector3[4];
        sliderRect.GetWorldCorners(corners);

        sliderLeftX = corners[0].x;
        sliderRightX = corners[2].x;

        OnSliderValueChanged(slider.value);
    }

    void OnDecrease()
    {
        slider.value = Mathf.Max(slider.minValue, getValue() - increment);
        OnSliderValueChanged(slider.value);
    }

    void OnIncrease()
    {
        slider.value = Mathf.Max(slider.minValue, getValue() + increment);
        OnSliderValueChanged(slider.value);
    }

    void UpdateLabelPosition()
    {
        float t = Mathf.InverseLerp(slider.minValue, slider.maxValue, slider.value);
        Vector3 newPos = valueLabel.rectTransform.position;
        newPos.x = Mathf.Lerp(sliderLeftX, sliderRightX, t);
        valueLabel.rectTransform.position = newPos;
    }

    void OnSliderValueChanged(float _value)
    {
        // Update label text
        valueLabel.text = getValue().ToString() + "mm";

        UpdateLabelPosition();
    }
}
