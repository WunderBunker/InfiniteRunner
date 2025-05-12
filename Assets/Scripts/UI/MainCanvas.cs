using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainCanvas : MonoBehaviour
{
    GameObject _pauseMenu;
    GameObject _endMenu;
    bool _isOnPause = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _pauseMenu = transform.Find("PauseMenu").gameObject;
        _pauseMenu.SetActive(false);
        _endMenu = transform.Find("EndMenu").gameObject;
        _endMenu.SetActive(false);
    }

    public void LaunchPauseMenu()
    {
        _isOnPause = !_isOnPause;
        _pauseMenu.SetActive(_isOnPause);

        Time.timeScale = _isOnPause ? 0 : 1;
    }

    public void LaunchEndMenu()
    {
        _endMenu.SetActive(true);
        _endMenu.transform.Find("Score").Find("Value").GetComponent<TextMeshProUGUI>().text
            = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerManager>().CollectedOboles.ToString();
        Time.timeScale = 0;
    }

    public void OnRetryButton()
    {
        SaveManager.AddOboles(GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerManager>().CollectedOboles);
        SceneManager.LoadScene("RunScene");
        Time.timeScale = 1;
    }

    public void OnBackToMenuButton()
    {
        SaveManager.AddOboles(GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerManager>().CollectedOboles);
        SaveManager.SaveSave();
        SceneManager.LoadScene("Menu");
        Time.timeScale = 1;
    }
}
