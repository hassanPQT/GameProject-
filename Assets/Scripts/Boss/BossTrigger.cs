using Game.Scripts.Gameplay;
using UnityEngine;

public class BossTrigger : MonoBehaviour
{
    [SerializeField] private BossController controller;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            controller.OnDetectPlayer(collision.GetComponent<PlayerController>());
            gameObject.SetActive(false);
        }
    }
}
