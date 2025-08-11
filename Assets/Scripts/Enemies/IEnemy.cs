using Game.Scripts.Gameplay;

public interface IEnemy
{
    bool IsMoving { get; set; }
    bool IsWin { get; set; }

    void OnDetectPlayer(PlayerController playerController);
    void OnPlayerRequest(PlayerController playerController);
    void OnWinning();
    void OnPlayerMissed();

}