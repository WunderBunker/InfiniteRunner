using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    void Start()
    {
        Save vSave = SaveManager.LoadSave();

        transform.Find("ObolesCount").GetComponent<TextMeshProUGUI>().text = "Oboles : " + vSave.Oboles.ToString();
        transform.Find("HighScore").GetComponent<TextMeshProUGUI>().text = "High Score : " + vSave.HighScore.ToString();
    }

    void OnApplicationQuit()
    {
        SaveManager.SaveSave();
    }

    public void PlayButtonClick()
    {
        SaveManager.SaveSave();
        StartCoroutine(WaitForClickSoundAndLoad());
    }

    IEnumerator WaitForClickSoundAndLoad()
    {
        AudioManager.Instance.PlayClickSound();
        yield return new WaitForSeconds(0.25f);
        SceneManager.LoadScene("RunScene");
    }
}
