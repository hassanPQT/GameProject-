using UnityEngine;

public class CutsceneTrigger : MonoBehaviour
{
    public CutsceneController cutsceneController;
    public Transform positionAfter;

    private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player")) return;

        triggered = true;
        // truyền enemy transform (bạn có thể set in inspector)
        Transform enemy = transform; // hoặc reference tới enemy transform
        cutsceneController.StartCutscene(enemy, positionAfter);
    }
}
