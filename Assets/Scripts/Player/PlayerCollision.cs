using UnityEngine;

public class PlayerCollision : MonoBehaviour
{
    private Enemy enemy;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            enemy = collision.gameObject.GetComponent<Enemy>();
        }
    }
}
