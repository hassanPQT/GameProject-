using Cinemachine;
using Game.Scripts.Gameplay;
using UnityEngine;

public partial class BossCameraController : MonoBehaviour
{
    [SerializeField] CinemachineVirtualCamera _camNormal;
    [SerializeField] CinemachineVirtualCamera _camPreboss;
    [SerializeField] CinemachineVirtualCamera _camBoss;
    [SerializeField] CinemachineVirtualCamera _camFollowBoss;

    [SerializeField] CinemachineTargetGroup _preTargetGroup;
    [SerializeField] CinemachineTargetGroup _bossTargetGroup;

    [SerializeField] Transform _preTransform;
    [SerializeField] float _preWeight = 1f;
    [SerializeField] float _preRadius = 1f;
    [SerializeField] private Transform _player;
    [SerializeField] float _playerWeight = 1f;
    [SerializeField] float _playerRadius = 1f;
    [SerializeField] private Transform _boss;
    [SerializeField] float _bossWeight = 1f;
    [SerializeField] float _bossRadius = 1f;
    private void Awake()
    {
        _player = FindFirstObjectByType<PlayerController>().transform;
        _boss = FindFirstObjectByType<BossController>().transform;

        if (_player == null || _boss == null) return;
        _camNormal.Follow = _player;
        _camNormal.LookAt = _player;
        _preTargetGroup.AddMember(_preTransform,_preWeight, _preRadius);
        _preTargetGroup.AddMember(_player,_playerWeight, _playerRadius);
        _bossTargetGroup.AddMember(_boss,_bossWeight, _bossRadius);
        _bossTargetGroup.AddMember(_player,_playerWeight, _playerRadius);
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
