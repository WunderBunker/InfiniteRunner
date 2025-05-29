using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class PartieManager : MonoBehaviour
{
    public static PartieManager Instance;

    PlayerManager _playerManager;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public static void OnDebug(InputAction.CallbackContext pContext)
    {
        Debug.Log("vho -1");
        if (pContext.started) DebugTool.ChangeDebugMode();
    }

    void Start()
    {
        _playerManager = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerManager>();
    }


    void OnApplicationQuit()
    {
        SaveRunData();
    }

    public void Death()
    {
        SaveRunData();
    }

    public void SaveRunData()
    {
        SaveManager.AddOboles(_playerManager.CollectedOboles);
        SaveManager.MajScore((int)_playerManager.Score);
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
            _debugUI.GetChild(lCptChild).gameObject.GetComponent<TextMeshProUGUI>().text = "";
    }


    static public void DrawDebugOnUI(int pSlot, string pText)
    {
        if (!IsModeDebug) return;
        TextMeshProUGUI vTextMesh = _debugUI.Find(pSlot == 1 ? "Slot1" : (pSlot == 2 ? "Slot2" : "Slot3")).gameObject.GetComponent<TextMeshProUGUI>();
        vTextMesh.text = pText;
    }

}
