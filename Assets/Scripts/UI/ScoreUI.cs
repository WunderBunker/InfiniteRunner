using System;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

public class ScoreUI : MonoBehaviour
{
    [SerializeField] float _ponctualFadingSpeed = 1;
    [SerializeField] float _ponctualFallingSpeed = 500;
    [SerializeField] GameObject _ponctualScore;
    PlayerManager _playerManager;

    List<PonctualScore> _ponctualsList = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _playerManager = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerManager>();
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<TextMeshProUGUI>().text = "Score : " + _playerManager.Score.ToString("0");

        foreach (PonctualScore lScore in _ponctualsList)
        {
            lScore.TextMesh.color = new Color(lScore.TextMesh.color.r, lScore.TextMesh.color.g, lScore.TextMesh.color.b, lScore.TextMesh.color.a - Time.deltaTime * lScore.FadingSpeed);
            lScore.ScoreTransform.position += Vector3.down * Time.deltaTime * lScore.FallingSpeed;
        }

        for (int lcptScore = _ponctualsList.Count - 1; lcptScore >= 0; lcptScore--)
            if (_ponctualsList[lcptScore].TextMesh.color.a <= 0) _ponctualsList.RemoveAt(lcptScore);

    }

    public void AddPonctualScore(int pNb)
    {
        float vValueCoef = math.min((float)pNb/500 , 1);

        GameObject vNewScoreObj = Instantiate(_ponctualScore, transform);
        PonctualScore vNewScore = new() { TextMesh = vNewScoreObj.GetComponent<TextMeshProUGUI>(), ScoreTransform = vNewScoreObj.transform };

        vNewScore.TextMesh.text = "+" + pNb.ToString();
        vNewScore.TextMesh.color = new Color(
            Tools.OutQuint(vValueCoef) * (1 - vNewScore.TextMesh.color.r) + vNewScore.TextMesh.color.r,
            Tools.OutQuint(1 - vValueCoef) * (1 - vNewScore.TextMesh.color.g) + vNewScore.TextMesh.color.g,
            vNewScore.TextMesh.color.b, 1);
        vNewScore.FadingSpeed = math.lerp(_ponctualFadingSpeed, _ponctualFadingSpeed / 4, vValueCoef);
        vNewScore.FallingSpeed = _ponctualFallingSpeed;

        _ponctualsList.Add(vNewScore);
    }
}

public struct PonctualScore
{
    public TextMeshProUGUI TextMesh;
    public Transform ScoreTransform;
    public float FadingSpeed;
    public float FallingSpeed;
}
