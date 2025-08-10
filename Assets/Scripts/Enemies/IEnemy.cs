using Game.Scripts.Gameplay;

public interface IEnemy
{
    bool IsWin { get; set; }
    void OnDetectPlayer();
    void OnPlayerRequest(PlayerController playerController);
    void OnWinning();
    void OnPlayerMissed();

}