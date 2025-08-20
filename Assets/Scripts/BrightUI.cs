using UnityEngine;
using UnityEngine.UI; // nhớ import namespace UI

public class BrightUI : MonoBehaviour
{
    [Range(1f, 3f)] public float brightness = 1.5f;

    void Start()
    {
        Image img = GetComponent<Image>();
        if (img != null)
        {
            Color c = img.color;
            float r = Mathf.Clamp01(c.r * brightness);
            float g = Mathf.Clamp01(c.g * brightness);
            float b = Mathf.Clamp01(c.b * brightness);
            float a = Mathf.Clamp01(c.a); // Đảm bảo alpha không bị giảm
            img.color = new Color(r, g, b, a);
        }
    }
}
