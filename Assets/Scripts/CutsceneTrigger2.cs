using UnityEngine;

public class CutsceneTrigger2 : MonoBehaviour
{
    public CutsceneController cutsceneController;

    [SerializeField] private bool triggered2 = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered2) return;
        if (!other.CompareTag("Player"))
            return;

        Debug.Log("Cutscene triggered for MapDesign 2");
        triggered2 = true;
        cutsceneController.StartCutscene2();
    }
}
