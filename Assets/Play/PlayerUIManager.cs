using UnityEngine;

public class PlayerUIManager : MonoBehaviour
{
    public GameObject menu, play;

    void Awake()
    {
        SwitchToMenu();
    }

    public void SwitchToMenu()
    {
        menu.SetActive(true);
        play.SetActive(false);
    }

    public void SwitchToPlayer()
    {
        play.SetActive(true);
        menu.SetActive(false);
    }
}
