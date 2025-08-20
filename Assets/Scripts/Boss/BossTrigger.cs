using Game.Scripts.Gameplay;
using System;
using Unity.VisualScripting;
using UnityEngine;

public class BossTrigger : MonoBehaviour
{
    public static Action OnEnter;
    public static Action OnExit;
    public enum TriggerState {
    
        None,
        Enter,
        Exit,
    }
    public TriggerState state;
    [SerializeField] private BossController controller;
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            var player = collision.GetComponent<PlayerController>();
            var effect = collision.GetComponent<PushBackEffect>() ?? collision.AddComponent<PushBackEffect>();
            switch (state)
            {
                case TriggerState.None:
                    controller.OnDetectPlayer(player);
                    break;
                case TriggerState.Enter:
                    effect.enabled = true;
                    //player.movement.StopPlayer();
                    player.movement.enabled = false;
                    OnEnter?.Invoke();
                    break;
                case TriggerState.Exit:
                    Debug.Log("exit");
                    OnExit?.Invoke();
                    player.movement.enabled = true;
                    //player.movement.UnStop();
                    effect.enabled = false;
                    break;

            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            Debug.Log("exit collider");

            switch (state)
            {
                case TriggerState.Enter:
                    break;
                case TriggerState.Exit:
                    state = TriggerState.Enter;
                    break;

            }
        }
    }
}
