using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopItem : MonoBehaviour
{
    public ShopItemStruct ItemStruct;
    public Shop Shop;

    Save _save;
    bool _isGot;
    [SerializeField]bool _isEquipped;
    SkinManager _boatSkinManager;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _save = SaveManager.GetSave();
        _isGot = _save.ShopSave.GottenItemsIdList.Contains(ItemStruct.ItemId);
        _boatSkinManager = FindAnyObjectByType<SkinManager>();
        transform.Find("Image").GetComponent<Image>().sprite = ItemStruct.Sprite;
        InitItemInfo();
    }

    public void InitItemInfo()
    {
        if (_isGot)
        {
            _isEquipped = IsEquipped();
            if (_isEquipped)
            {
                transform.Find("Cost").GetComponent<TextMeshProUGUI>().text = "";
                transform.Find("BuyButton").Find("Text").GetComponent<TextMeshProUGUI>().text = "Equipped";
                transform.Find("BuyButton").GetComponent<Button>().interactable = false;
                transform.Find("BuyButton").GetComponent<Image>().color = Color.yellow;
            }
            else
            {
                transform.Find("Cost").GetComponent<TextMeshProUGUI>().text = "";
                transform.Find("BuyButton").Find("Text").GetComponent<TextMeshProUGUI>().text = "Equip";
                transform.Find("BuyButton").GetComponent<Button>().interactable = true;
                transform.Find("BuyButton").GetComponent<Image>().color = Color.white;
            }
        }
        else
        {
            _isEquipped = false;
            transform.Find("Cost").GetComponent<TextMeshProUGUI>().text = "Cost : " + ItemStruct.Cost.ToString();
            transform.Find("BuyButton").Find("Text").GetComponent<TextMeshProUGUI>().text = "Buy";
            if (_save.Oboles < ItemStruct.Cost)
            {
                transform.Find("BuyButton").GetComponent<Button>().interactable = false;
                transform.Find("BuyButton").GetComponent<Image>().color = Color.gray;
                transform.Find("BuyButton").Find("Text").GetComponent<TextMeshProUGUI>().color = Color.gray;
            }
        }
    }

    public void BuyButton()
    {
        AudioManager.Instance.PlayClickSound();
        if (_isGot) Equip();
        else Buy();
    }

    void Buy()
    {
        if (_save.Oboles >= ItemStruct.Cost)
        {
            _isGot = true;
            SaveManager.AddGottenItem(ItemStruct.ItemId);
            SaveManager.AddOboles(-ItemStruct.Cost);
            Equip();
        }
    }

    void Equip()
    {
        switch (ItemStruct.ItemType)
        {
            case ShopItemType.Material:
                switch (ItemStruct.ItemSubType)
                {
                    case ShopItemSubType.Plank:
                        SaveManager.ChangeSkinPlank(ItemStruct.ItemId);
                        _boatSkinManager.ChangePlankMaterial(ItemStruct.ItemId);
                        break;
                    case ShopItemSubType.Sail:
                        SaveManager.ChangeSkinSail(ItemStruct.ItemId);
                        _boatSkinManager.ChangeSailMaterial(ItemStruct.ItemId);
                        break;
                    default:
                        break;
                }
                break;
            default:
                break;
        }
        Shop.MajShop();
    }

    bool IsEquipped()
    {
        switch (ItemStruct.ItemType)
        {
            case ShopItemType.Material:
                switch (ItemStruct.ItemSubType)
                {
                    case ShopItemSubType.Plank:
                        return _save.Skin.PlankId == ItemStruct.ItemId;
                    case ShopItemSubType.Sail:
                        return _save.Skin.SailId == ItemStruct.ItemId;
                    default:
                        return false;
                }
            default:
                return false;
        }
    }
}

public enum ShopItemType
{
    Material
}

public enum ShopItemSubType
{
    //Material
    Plank,
    Sail
}
