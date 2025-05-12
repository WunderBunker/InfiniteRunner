using TMPro;
using UnityEngine;

public class ObolesUI : MonoBehaviour
{
    PlayerManager _playerManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _playerManager = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerManager>();
    }

    // Update is called once per frame
    void Update()
    {
        GetComponent<TextMeshProUGUI>().text = "Oboles : " + _playerManager.CollectedOboles.ToString();
    }
}
