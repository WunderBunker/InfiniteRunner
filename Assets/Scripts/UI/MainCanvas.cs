using System.Collections;
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

    public void OnButtonClose()
    {
        LaunchPauseMenu();
        AudioManager.Instance.PlayClickSound();
    }

    public void OnRetryButton()
    {
        PartieManager.Instance.SaveRunData();

        SceneManager.LoadScene("RunScene");
        Time.timeScale = 1;

        AudioManager.Instance.PlayClickSound();
    }

    public void OnBackToMenuButton()
    {
        PartieManager.Instance.SaveRunData();
        Time.timeScale = 1;
        StartCoroutine(WaitForClickSoundAndLoad());
    }

    IEnumerator WaitForClickSoundAndLoad()
    {
        AudioManager.Instance.PlayClickSound();
        yield return new WaitForSeconds(0.25f);
        SceneManager.LoadScene("Menu");
    }
}
