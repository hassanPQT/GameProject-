using UnityEngine;

public class BrightSprite : MonoBehaviour
{
    [Range(1f, 3f)] public float brightness = 1.5f; // hệ số sáng (1 = giữ nguyên)

    void Start()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color c = sr.color;
            // Tăng RGB lên theo hệ số, nhưng không vượt quá 1.0
            float r = Mathf.Min(c.r * brightness, 1f);
            float g = Mathf.Min(c.g * brightness, 1f);
            float b = Mathf.Min(c.b * brightness, 1f);
            sr.color = new Color(r, g, b, c.a);
        }
    }
}
