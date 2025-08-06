using UnityEngine;
using UnityEngine.UI;

public class EmotionalBar : MonoBehaviour
{
    public Slider slider;
    public Image fill;
    public Gradient gradient; // Gradient for the fill color

    [Header("Emotion Point")]
    public int maxEmotionPoint = 100;
    public int startEmotionPoint = 50;
    public int currentEmotionPoint;
    public EmotionalBar emotionalBar;
    public int emotionRewardPoint = 10;

    private void Start()
    {
        currentEmotionPoint = startEmotionPoint; // Initialize current emotion point
    }
    public void SetMaxEmotion()
    {
        slider.maxValue = maxEmotionPoint;
        slider.value = startEmotionPoint; // Set the initial value to max

        fill.color = gradient.Evaluate(1f); // Set the fill color to the end of the gradient
    }

    public void SetEmotion()
    {
       slider.value = currentEmotionPoint;

        fill.color = gradient.Evaluate(slider.normalizedValue);
    }

    void TakeRedeem()
    {
        if (GameManager.Instance.IsWin)
        {
            currentEmotionPoint += 10;         
        }
        else 
        {
            currentEmotionPoint -= 10;         
        }
    }
}
