using Game.Scripts.Gameplay;
using Unity.VisualScripting;
using UnityEngine;

public class BossController : MonoBehaviour
{
    [SerializeField] private SpriteRenderer model;
    [SerializeField] private BossAI bossAI;

    private void Start()
    {
        model.enabled = false;
    }

    public void OnDetectPlayer(PlayerController playerController)
    {
        // cutscene or animation some thong else
        
    

        /// 
        model.enabled = true;
        // stop player va them effect 
        playerController.movement.StopPlayer();
        playerController.movement.enabled = false;
        var backEffect = playerController.AddComponent<PushBackEffect>();

        // push player back

        // play game loop
        bossAI.StartGameLoop(playerController, backEffect);

    }
}
