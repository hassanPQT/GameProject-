using DG.Tweening;
using Game.Scripts.Gameplay;
using System.Collections;
using UnityEngine;

public class BossController : MonoBehaviour
{
    private static readonly int IsAttack = Animator.StringToHash("isAttack");
    private static readonly int IsDie = Animator.StringToHash("isDie");


    private Animator animator;
    [SerializeField] private Transform bossEnterTrigger;
    [SerializeField] private Transform bossTrigger;
    [SerializeField] private Transform position;
    [SerializeField] private BossAI bossAI;
    [SerializeField] private float pushLeght = 20f;
    [SerializeField] private float pushTime = 3f;

    private void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void OnDetectPlayer(PlayerController playerController)
    {
        StartCoroutine(DetectProcess(playerController));
    }

    private IEnumerator DetectProcess(PlayerController playerController)
    {
        playerController.movement.StopMoving();
        animator.SetBool(IsDie, true);
        playerController.movement.StopPlayer();
        playerController.movement.enabled = false;
        yield return CutScence();
        bossEnterTrigger.gameObject.SetActive(true);

        bossAI.UpdateUI(0);
        yield return playerController.transform.DOMoveX(playerController.transform.position.x - pushLeght, pushTime);
        


        bossTrigger.gameObject.SetActive(true);
        playerController.movement.enabled = true;
        playerController.movement.UnStop();
    }
    private IEnumerator CutScence()
    {
        var sequence = DOTween.Sequence();
        sequence.Append( transform.DOMove(position.position + Vector3.up * 10f, 1).SetEase(Ease.Linear));
        sequence.Append( transform.DOMove(position.position, 0.2f).SetEase(Ease.Linear));
        yield return sequence.WaitForCompletion();
        yield return new WaitForSeconds(2f);
    }
}
