using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Shop : MonoBehaviour
{
    [SerializeField] ShoppingList _shoppingList;
    [SerializeField] GameObject _itemUI;
    [SerializeField] Transform _itemLayout;
    [SerializeField] AudioClip _soundScape;

    int _soundscapeToken;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transform.Find("Oboles").GetComponent<TextMeshProUGUI>().text = "Oboles : " + SaveManager.GetSave().Oboles;
        _soundscapeToken = AudioManager.Instance.PlayKeepSound(_soundScape, 1);

        foreach (ShopItemStruct lItem in _shoppingList.ItemsList)
        {
            var lNewItem = Instantiate(_itemUI, GetLayout(lItem));
            lNewItem.GetComponent<ShopItem>().ItemStruct = lItem;
            lNewItem.GetComponent<ShopItem>().Shop = this;
        }
    }

    public void Quit()
    {
        SaveManager.SaveSave();
        StartCoroutine(LoadMenu());
    }

    public void MajShop()
    {
        transform.Find("Oboles").GetComponent<TextMeshProUGUI>().text = "Oboles : " + SaveManager.GetSave().Oboles;
        for (int lCptChild = 0; lCptChild < _itemLayout.childCount; lCptChild++)
            for (int lCptChild2 = 0; lCptChild2 < _itemLayout.GetChild(lCptChild).childCount; lCptChild2++)
                if (_itemLayout.GetChild(lCptChild).GetChild(lCptChild2).name != "Text")
                    _itemLayout.GetChild(lCptChild).GetChild(lCptChild2).GetComponent<ShopItem>().InitItemInfo();
    }

    Transform GetLayout(ShopItemStruct pItem)
    {
        switch (pItem.ItemType)
        {
            case ShopItemType.Material:
                switch (pItem.ItemSubType)
                {
                    case ShopItemSubType.Plank:
                        return _itemLayout.Find("Plank");
                    case ShopItemSubType.Sail:
                        return _itemLayout.Find("Sail");
                }
                return _itemLayout;
            default:
                return _itemLayout;
        }
    }

    IEnumerator LoadMenu()
    {
        AudioManager.Instance.PlayClickSound();
        yield return transform.Find("FadeInBlack").GetComponent<FadeInBlack>().FadeIn();
        SceneManager.LoadScene("Menu");
    }

    void OnDestroy()
    {
        AudioManager.Instance.StopKeepSound(_soundscapeToken);
    }
}

[Serializable]
public struct ShopItemStruct
{
    public int Cost;
    public ShopItemType ItemType;
    public ShopItemSubType ItemSubType;
    public string ItemId;
    public Sprite Sprite;

}
