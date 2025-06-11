using UnityEngine;

public class PauseMenu : MonoBehaviour
{
    public void BackToMenu()
    {
        PartieManager.Instance.SaveRunData();
        StartCoroutine(transform.parent.GetComponent<MainCanvas>().LoadScene("Menu"));
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
