using UnityEngine;

public class WallTrigger : MonoBehaviour
{
    private void Awake()
    {
            GetComponent<Collider2D>().isTrigger = true;
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            GetComponent<Collider2D>().isTrigger = false;
        }
    }

    public void Exit()
    {
        Application.Quit();
    }
}
