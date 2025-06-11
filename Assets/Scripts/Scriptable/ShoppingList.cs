using UnityEngine;

[CreateAssetMenu(fileName = "ShoppingList", menuName = "Scriptable Objects/ShoppingList")]
public class ShoppingList : ScriptableObject
{
    [SerializeField] public ShopItemStruct[] ItemsList;
}

