using Game.Scripts.Gameplay;
using System;
using Unity.VisualScripting;
using UnityEngine;

public class BossTrigger : MonoBehaviour
{
    public static Action<PlayerController, PushBackEffect> OnEnter;
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
            var effect = player.GetComponent<PushBackEffect>();
            switch (state)
            {
                case TriggerState.None:
                    controller.OnDetectPlayer(player);
                    break;
                case TriggerState.Enter:
                    if (effect == null)
                    {
                        effect = player.AddComponent<PushBackEffect>();
                    }
                    effect.enabled = true;
                    OnEnter?.Invoke(player, effect);
                    break;
                case TriggerState.Exit:
                    if (effect == null)
                    {
                        effect = player.AddComponent<PushBackEffect>();
                    }
                    OnExit?.Invoke();
                    effect.enabled = false;
                    break;

            }
        }
    }
}
