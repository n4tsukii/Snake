using UnityEngine;

public class GameController : MonoBehaviour
{
    void Start()
    {
        StopGame();
    }
    public void StopGame()
    {
        Time.timeScale = 0;
    }

    public void PlayGame()
    {
        Time.timeScale = 1;
    }
}
