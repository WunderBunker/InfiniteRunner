using TMPro;
using UnityEngine;

public class Cours_Global : MonoBehaviour
{
    public static Cours_Global Instance;
    int _score = 0;
    [SerializeField] TextMeshProUGUI _scoreUI;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        Instance = this;
    }

    public void AddScore(int pValue)
    {
        _score += pValue;
        _scoreUI.text = "Score : " + _score.ToString();
    }
}
