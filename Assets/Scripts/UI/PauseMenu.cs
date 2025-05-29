using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public void BackToMenu()
    {
        PartieManager.Instance.SaveRunData();
        SceneManager.LoadScene("Menu");
    }

    void OnEnable()
    {
        Time.timeScale = 0;
    }
    void OnDisable()
    {
        Time.timeScale = 1;
    }
}
