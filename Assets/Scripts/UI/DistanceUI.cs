using TMPro;
using UnityEngine;

public class DistanceUI : MonoBehaviour
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
        GetComponent<TextMeshProUGUI>().text = "Distance : " + _playerManager.TotalDistance.ToString("0.0");
    }
}
