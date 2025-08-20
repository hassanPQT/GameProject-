using UnityEngine;

public class CameraTrigger : MonoBehaviour
{
    [SerializeField] BossCameraController controller;
    private void OnEnable()
    {
        controller = FindFirstObjectByType<BossCameraController>();
    }
    public bool IsOnce = false;
    public BossCameraController.State state;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            controller.ChangeState(state);

            if (IsOnce)
            {
                gameObject.GetComponent<Collider2D>().enabled = false;
            }
        }
    }
}
