using UnityEngine;

public class MainCanvas : MonoBehaviour
{
    GameObject _pauseMenu;
    bool _isOnPause = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _pauseMenu = transform.Find("PauseMenu").gameObject;
        _pauseMenu.SetActive(false);
    }

    public void OnPauseButton()
    {
        _isOnPause = !_isOnPause;
        _pauseMenu.SetActive(_isOnPause);
    }
}
