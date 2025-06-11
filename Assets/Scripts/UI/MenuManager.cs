using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [SerializeField] AudioClip _soundScape;
    int _soundscapeToken;
    GameObject _tutorialPanel;
    GameObject _leaderBoardPanel;

    void Start()
    {
        Save vSave = SaveManager.LoadSave();

        transform.Find("ObolesCount").GetComponent<TextMeshProUGUI>().text = "Oboles : " + vSave.Oboles.ToString();
        transform.Find("HighScore").GetComponent<TextMeshProUGUI>().text = "High Score : " + vSave.HighScore.ToString();
        _tutorialPanel = transform.Find("Tutorial").gameObject;
        _leaderBoardPanel = transform.Find("LeaderBoard").gameObject;

        //Si premier pas encore de score d'enregitrer on affiche le tuto
        if (vSave.HighScore == 0) LaunchTutorialPanel(true);

        _soundscapeToken = AudioManager.Instance.PlayKeepSound(_soundScape, 1);
    }

    void OnApplicationQuit()
    {
        SaveManager.SaveSave();
    }

    public void PlayButtonClick()
    {
        SaveManager.SaveSave();
        PlayClickSound();
        StartCoroutine(LoadOtherScene("RunScene"));
    }

    IEnumerator LoadOtherScene(string pScene)
    {
        yield return transform.Find("FadeInBlack").GetComponent<FadeInBlack>().FadeIn();
        SceneManager.LoadScene(pScene);
    }


    public void LaunchTutorialPanel(bool pActive) => _tutorialPanel.SetActive(pActive);

    public void LaunchLeaderBoardPanel(bool pActive) => _leaderBoardPanel.SetActive(pActive);


    public void LaunchShopScene() => StartCoroutine(LoadOtherScene("Shop"));

    public void PlayClickSound() => AudioManager.Instance.PlayClickSound();

    void OnDestroy()
    {
        AudioManager.Instance.StopKeepSound(_soundscapeToken);
    }

}
