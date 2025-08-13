using Game.Scripts.Gameplay;
using System.Threading.Tasks;
using UnityEngine;

public class BossAI : MonoBehaviour
{
   [SerializeField]  private ParticleSystem _dirVFX;
    private void Awake()
    {
        _dirVFX.gameObject.SetActive(false);
    }
    private bool _isPlaying;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private readonly Vector2[] attackDir = new Vector2[] {
        Vector2.left, 
        Vector2.left + Vector2.up,
        Vector2.left + Vector2.down,
    };
    private Vector2 _attackDir;
    public async void StartGameLoop(PlayerController playerController, PushBackEffect backEffect)
    {
        _isPlaying = true;
        _dirVFX.gameObject.SetActive(true);
        while (_isPlaying)
        {
            await PushBackPlayer(playerController, GetRandomAttackDir(), backEffect);
        }
        backEffect.enabled = false;
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        _isPlaying = false;
    }
    private Vector2 GetRandomAttackDir()
    {
        _attackDir = attackDir[Random.Range(0, attackDir.Length)];
        return _attackDir;
    }

    private async Task PushBackPlayer(PlayerController player, Vector2 dir, PushBackEffect backEffect)
    {
        // show animation background
        if (dir == Vector2.left)
        {
            _dirVFX.transform.rotation = Quaternion.Euler(0, -90, 0);
        }
        if (dir == Vector2.left + Vector2.up)
        {
            _dirVFX.transform.rotation = Quaternion.Euler(-45, -90, 0);
        }
        if (dir == Vector2.left + Vector2.down)
        {
            _dirVFX.transform.rotation = Quaternion.Euler(45, -90, 0);

        }
        backEffect.BossDirection = dir;

        await Task.Delay(3000);
    }
}
