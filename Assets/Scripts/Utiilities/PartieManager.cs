using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PartieManager : MonoBehaviour
{
    PlayerManager _playerManager;
    

    public static void OnDebug(InputAction.CallbackContext pContext)
    {
        if (pContext.started) DebugTool.ChangeDebugMode();
    }

    void Start()
    {
        _playerManager = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerManager>();
    }


    void OnApplicationQuit()
    {
        SaveManager.AddOboles(_playerManager.CollectedOboles);
        SaveManager.SaveSave();
    }

    public void Death()
    {
        SaveManager.AddOboles(_playerManager.CollectedOboles);
        SaveManager.SaveSave();
    }

}

public static class DebugTool
{
    public static bool IsModeDebug;

    static Transform __debugUI;
    static Transform _debugUI
    {
        get
        {
            if (__debugUI == null) __debugUI = GameObject.FindGameObjectWithTag("MainCanvas").transform.Find("Debug");
            return __debugUI;
        }
    }

    public static void ChangeDebugMode()
    {
        IsModeDebug = !IsModeDebug;
        for (int lCptChild = 0; lCptChild < _debugUI.childCount; lCptChild++)
            _debugUI.GetChild(lCptChild).gameObject.GetComponent<TextMeshPro>().text = "";
    }


    static public void DrawDebugOnUI(int pSlot, string pText)
    {
        if (!IsModeDebug) return;
        TextMeshPro vTextMesh = _debugUI.Find(pSlot == 1 ? "Slot1" : (pSlot == 2 ? "Slot2" : "Slot3")).gameObject.GetComponent<TextMeshPro>();
        vTextMesh.text = pText;
    }

}
