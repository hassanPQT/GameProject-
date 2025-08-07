using System;

public interface IListener 
{
    void Playing();
    void Pause();
    void EnemySing();
    void PlayerSing();
    void WinSing();
    void LoseSing();
}
