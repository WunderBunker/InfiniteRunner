using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public void BackToMenu()
    {
        SaveManager.AddOboles(GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerManager>().CollectedOboles);
        SaveManager.SaveSave();
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
