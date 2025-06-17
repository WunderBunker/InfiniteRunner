using TMPro;
using UnityEngine;
using UnityEngine.UI;

//GESTION D'UN UI D'ELEMENT EN VENTE DANS LE SHOP
public class ShopItem : MonoBehaviour
{
    public ShopItemStruct ItemStruct;
    public Shop Shop;

    PlayerSave _save;
    bool _isGot;
    [SerializeField] bool _isEquipped;
    SkinManager _boatSkinManager;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _save = SaveManager.GetPlayerSave();
        _isGot = _save.ShopSave.GottenItemsIdList.Contains(ItemStruct.ItemId);
        _boatSkinManager = FindAnyObjectByType<SkinManager>();
        transform.Find("Image").GetComponent<Image>().sprite = ItemStruct.Sprite;
        InitItemInfo();
    }

    //Initialisation des infos affichés (cout, acheté/équippé...)
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

    //On équipe l'item 
    void Equip()
    {
        //sauvegarde + application du skin sur le modèle de showcase en fonction du type et du sous-type de l'élément
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
        //Maj des infos du shop
        Shop.MajShop();
    }

    //Permet de savoir si l'item en cours est équippé en comparant avec la sauvegarde
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

//Type d'item (matériau, mesh, lumière...)
public enum ShopItemType
{
    Material
}

//Sous-type d'élément (planche, voile, canon...)
public enum ShopItemSubType
{
    //Material
    Plank,
    Sail
}
