using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    void Start()
    {
        Save vSave = SaveManager.LoadSave();

        transform.Find("ObolesCount").GetComponent<TextMeshProUGUI>().text = "Oboles : " + vSave.Oboles.ToString();
    }

    void OnApplicationQuit()
    {
        SaveManager.SaveSave();
    }

    public void PlayButtonClick()
    {
        SceneManager.LoadScene("RunScene");
        SaveManager.SaveSave();
    }
}
