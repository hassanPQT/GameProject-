using UnityEngine;

public class SliceView : MonoBehaviour
{
    public void HighlightSlice()
    {
        //GetComponent<RectTransform>().sizeDelta = _sliceSize[slice] + new Vector2(10, 10);
        transform.GetChild(0).gameObject.SetActive(true);
        transform.GetChild(1).gameObject.SetActive(true);
    }
    public void UnHighlight()
    {
        transform.GetChild(0).gameObject.SetActive(false);
        transform.GetChild(1).gameObject.SetActive(false);
    }
}
