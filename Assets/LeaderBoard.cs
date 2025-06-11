using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class LeaderBoard : MonoBehaviour
{
    [SerializeField] GameObject _element;
    [SerializeField] Transform _elementLayout;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Dictionary<string, Save> vBoardSave = BoardManager.LoadBoardSave();
        vBoardSave = vBoardSave.OrderByDescending(x => x.Value.HighScore).ToDictionary(x => x.Key, x => x.Value); ;

        int vRank = 0;
        int vPlayerRank = 0;
        foreach (KeyValuePair<string, Save> lSave in vBoardSave)
        {
            vRank++;
            if (lSave.Value.Player == SaveManager._player) vPlayerRank = vRank;
            GameObject lElement = Instantiate(_element, _elementLayout);
            lElement.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>().text = lSave.Value.Player;
            lElement.transform.GetChild(1).GetChild(0).GetComponent<TextMeshProUGUI>().text = lSave.Value.HighScore.ToString();
            lElement.transform.GetChild(2).GetChild(0).GetComponent<TextMeshProUGUI>().text = vRank.ToString();
        }

        transform.Find("PlayerRank").GetChild(0).GetComponent<TextMeshProUGUI>().text = "Your rank is : " + vPlayerRank.ToString();
    }
}
