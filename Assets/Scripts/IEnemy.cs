public interface IEnemy
{
    bool IsWin { get; set; }
    void OnDetectPlayer();
    void OnPlayerRequest();
    void OnWinning();
    void OnPlayerMissed();

}