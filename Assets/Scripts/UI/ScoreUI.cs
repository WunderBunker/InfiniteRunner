using TMPro;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    [SerializeField] float _ponctualFadingSpeed = 1;
    PlayerManager _playerManager;
    TextMeshProUGUI _ponctualPointText;
    bool _isFadingPonctual;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _playerManager = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerManager>();
        _ponctualPointText = transform.Find("PonctualPoint").gameObject.GetComponent<TextMeshProUGUI>();
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<TextMeshProUGUI>().text = "Score : " + _playerManager.Score.ToString("0");
        if (_isFadingPonctual)
        {
            _ponctualPointText.color = new Color(_ponctualPointText.color.r, _ponctualPointText.color.g, _ponctualPointText.color.b, _ponctualPointText.color.a - Time.deltaTime * _ponctualFadingSpeed);
            if(_ponctualPointText.color.a<=0)
                _isFadingPonctual = false;
        }
    }

    public void AddPonctualScore(int pNb)
    {
        _ponctualPointText.text = "+" + pNb.ToString();
        _ponctualPointText.color = new Color(_ponctualPointText.color.r, _ponctualPointText.color.g, _ponctualPointText.color.b, 1);
        _isFadingPonctual = true;
    }
}

