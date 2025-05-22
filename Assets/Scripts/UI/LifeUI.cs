using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LifeUI : MonoBehaviour
{
    [SerializeField] Color _deadLifeColor;
    PlayerManager _playerManager;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _playerManager = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerManager>();
    }

    public void SetMaxLife(byte pMaxLife)
    {
        GameObject vLifeUnit = transform.Find("LifeUnit").gameObject;
        for (int lCptLife = 0; lCptLife < pMaxLife; lCptLife++)
            Instantiate(vLifeUnit, transform);
        Destroy(vLifeUnit);
    }

    public void SetLifeNb(byte pLife)
    {
        for (int lCptChild = 0; lCptChild < transform.childCount; lCptChild++)
            if (lCptChild > pLife - 1) transform.GetChild(lCptChild).GetComponent<Image>().color = _deadLifeColor;
            else transform.GetChild(lCptChild).GetComponent<Image>().color = Color.white;
    }
}
