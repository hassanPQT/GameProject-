using Cinemachine;
using Unity.Jobs;
using UnityEngine;
using UnityEngine.UI;

public class MoodBarController : MonoBehaviour
{
    [SerializeField] private Slider moodSlider;
    [SerializeField, Range(0f, 1f)] private float startValue = 0.5f;
    [SerializeField] private Sprite[] moodFillImage;
    [SerializeField] private Image ChangeMoodImage;

    // speed modifiers, v.v. có thể bind từ đây
    public float MovementSpeedModifier { get; private set; } = 1f;
    public float SongWheelTimeModifier { get; private set; } = 1f;

    private float _moodValue;

    private void Awake()
    {
        _moodValue = Mathf.Clamp01(startValue);
        UpdateUI();
        ApplyEffects();
    }

    /// <summary> Tăng mood thêm delta (0→1) </summary>
    public void IncreaseMood(float delta)
    {
        SetMood(_moodValue + delta);
    }

    /// <summary> Giảm mood đi delta (0→1) </summary>
    public void DecreaseMood(float delta)
    {
        SetMood(_moodValue - delta);
    }

    private void SetMood(float newValue)
    {
        _moodValue = Mathf.Clamp01(newValue);
        UpdateUI();
        ApplyEffects();

        // Clamp mood value first
        _moodValue = Mathf.Clamp01(_moodValue);

        // Handle special cases
        if (Mathf.Approximately(_moodValue, 0f))
        {
            ChangeMoodImage.sprite = moodFillImage[moodFillImage.Length - 1]; // 0% mood
            OnMoodMin();
            Debug.LogWarning("Mood value cannot be negative, resetting to 0.");
        }
        else if (Mathf.Approximately(_moodValue, 0.9f) || Mathf.Approximately(_moodValue, 1f))
        {
            ChangeMoodImage.sprite = moodFillImage[0]; // 100% mood
            OnMoodMax();
            Debug.LogWarning("Mood value cannot exceed 1, resetting to 1.");
        }
        else
        {
            // Select sprite based on ranges
            if (_moodValue <= 0.3f)
                ChangeMoodImage.sprite = moodFillImage[moodFillImage.Length - 1]; // 30% mood
            else if (Mathf.Approximately(_moodValue, 0.5f))
                ChangeMoodImage.sprite = moodFillImage[2]; // 50% mood
            else if (_moodValue < 0.5f)
                ChangeMoodImage.sprite = moodFillImage[3]; // <50% mood
            else if (_moodValue <= 0.75f)
                ChangeMoodImage.sprite = moodFillImage[1]; // 70% mood
            else // 0.75 < mood <= 0.9
                ChangeMoodImage.sprite = moodFillImage[0]; // 90% mood
        }
    }


    private void UpdateUI()
{
    if (moodSlider != null)
        moodSlider.value = _moodValue;
}

private void ApplyEffects()
{
    // Ví dụ: khi mood cao: tăng speed, tăng thời gian chọn
    MovementSpeedModifier = 1f + _moodValue * 0.5f;        // max +50% speed
    SongWheelTimeModifier = 1f + _moodValue * 0.5f;         // max +50% time
                                                            // Bạn có thể broadcast event hoặc gán trực tiếp cho player / UI
}

private void OnMoodMax()
{
    Debug.Log("Mood full: unlock extra Song Wheel");
    // gọi unlock skill, thêm slice mới...
}

private void OnMoodMin()
{
    Debug.Log("Mood empty: Rabbit state, disable Song Wheel");
    // vắng quyền dùng song wheel, respawn player...
}
}
