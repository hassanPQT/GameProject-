using DG.Tweening;
using Game.Scripts.Gameplay;
using System.Collections;
using UnityEngine;

public class BossController : MonoBehaviour
{
    [SerializeField] private Transform camBossToPlayer;
    [SerializeField] private Transform camPlayerToBoss;
    [SerializeField] private Transform bossTrigger;
    [SerializeField] private Transform position;
    [SerializeField] private SpriteRenderer model;
    [SerializeField] private BossAI bossAI;

    private void Start()
    {
        model.enabled = false;
    }

    public void OnDetectPlayer(PlayerController playerController)
    {
        StartCoroutine(DetectProcess(playerController));
    }

    private IEnumerator DetectProcess(PlayerController playerController)
    {
        model.enabled = true;
        playerController.movement.StopPlayer();
        playerController.movement.enabled = false;
        yield return CutScence();
        camBossToPlayer.gameObject.SetActive(true);
        camPlayerToBoss.gameObject.SetActive(true);

        bossAI.UpdateUI(0);
        var pushback = playerController.transform.DOMoveX(playerController.transform.position.x - 40f, 1f);
        yield return pushback.WaitForCompletion();
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
