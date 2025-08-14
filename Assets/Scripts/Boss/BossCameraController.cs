using Cinemachine;
using UnityEngine;

public class BossCameraController : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera _camNormal;
    [SerializeField] CinemachineVirtualCamera _camPreboss;
    [SerializeField] CinemachineVirtualCamera _camBoss;
    [SerializeField] CinemachineVirtualCamera _camFollowBoss;
    public enum State
    {
        Normal,
        Preboss,
        Cutscence,
        Boss,
    }

    private void ResetAll()
    {
        _camBoss.Priority = 0;
        _camNormal.Priority = 0;
        _camPreboss.Priority = 0;
        _camFollowBoss.Priority = 0;
    }

    public void ChangeState(State state)
    {
        ResetAll();

        switch (state)
        {
            case State.Normal:
                _camNormal.Priority = 10;
                break;
            case State.Preboss:
                _camPreboss.Priority = 10;
                break ;
            case State.Cutscence:
                _camFollowBoss.Priority = 10;
                break ;
            case State.Boss:
                _camBoss.Priority = 10;
                break ;
        }
    }
}
