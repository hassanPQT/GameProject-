using UnityEngine;

namespace Game.Scripts.Gameplay
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class PlayerController : MonoBehaviour, IListener
    {
        
        public PlayerDetection detection;
        public PlayerMovement movement;
     

        //  private CameraFollowObject _cameraFollowObject;

        private void Start()
        {
            movement = GetComponent<PlayerMovement>();
            detection = GetComponent<PlayerDetection>();
            GAME_STAT.PLAYER_SPEED = movement._moveSpeed;  
            movement._canRun = false;
        }
       


        public void Playing()
        {
        }

        public void Pause()
        {
            movement.StopPlayer();
        }

        public void GameWin()
        {
        }

        public void GameLose()
        {
        }

    }


}
