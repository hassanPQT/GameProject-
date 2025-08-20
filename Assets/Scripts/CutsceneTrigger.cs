using Game.Scripts.Gameplay;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CutsceneTrigger : MonoBehaviour
{
    public CutsceneController cutsceneController;

    [SerializeField] private bool triggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (triggered) return;
        if (!other.CompareTag("Player"))
            return;

        triggered = true;
        // truyền enemy transform (bạn có thể set in inspector)
        cutsceneController.StartCutscene();
    }
}
