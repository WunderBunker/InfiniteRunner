using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;


public static class SaveManager
{
    static public string _player = "Wunder";
    static Save _save;

    public static Save LoadSave()
    {
        string vPath;

        Save vScoresSave = null;
        vPath = GetSaveFolder() + "/saveGame_" + _player;

        string vTextSave;

        if (File.Exists(vPath) && !Equals(vPath, null))
        {
            vTextSave = File.ReadAllText(vPath);

            try
            {
                vScoresSave = JsonUtility.FromJson<Save>(vTextSave);
            }
            catch (ArgumentException)
            {
                Debug.Log("Erreur dans le chargement de la sauvegarde");
                File.Delete(vPath);
            }
        }
        else
        {
            Debug.Log("Pas de fichier save, creation d'un nouveau");
            if (File.Exists(vPath)) File.Delete(vPath);
        }

        if (vScoresSave == null || vScoresSave.Player != _player) _save = new() { Player = _player };
        else _save = vScoresSave;

        return _save;
    }

    public static Save GetSave()
    {
        if (_save != null) return _save;
        else return LoadSave();
    }

    public static void SaveSave()
    {
        if (_save == null) LoadSave();

        string vJsonFile = JsonUtility.ToJson(_save);
        string vSavePath = GetSaveFolder() + "/saveGame_" + _player;

        BoardManager.MajSave(_save);
        BoardManager.SaveBoardSave();

        File.WriteAllText(vSavePath, vJsonFile);
    }

    public static void AddOboles(int pNb)
    {
        if (_save == null) LoadSave();

        _save.Oboles = Math.Max(_save.Oboles + pNb, 0);
    }


    public static void MajScore(int pScore)
    {
        if (_save == null) LoadSave();
        if (pScore > _save.HighScore)
            _save.HighScore = pScore;
        BoardManager.MajSave(_save);
    }

    static string GetSaveFolder()
    {
        string vSavePath;
        vSavePath = Application.persistentDataPath;
        return vSavePath;
    }

    public static void ChangeSkinPlank(string pPlankId)
    {
        if (_save == null) LoadSave();
        _save.Skin.PlankId = pPlankId;
    }
    public static void ChangeSkinSail(string pSailId)
    {
        if (_save == null) LoadSave();
        _save.Skin.SailId = pSailId;
    }

    public static void AddGottenItem(string pItemId)
    {
        if (!_save.ShopSave.GottenItemsIdList.Contains(pItemId)) _save.ShopSave.GottenItemsIdList.Add(pItemId);
    }
}

public static class BoardManager
{
    static Dictionary<string, Save> _boardSave;

    public static Dictionary<string, Save> LoadBoardSave()
    {
        string vPath;

        BoardSave vBoardSave = null;
        vPath = GetSaveFolder() + "/board";

        string vTextSave;

        if (File.Exists(vPath) && !Equals(vPath, null))
        {
            vTextSave = File.ReadAllText(vPath);

            try
            {
                vBoardSave = JsonUtility.FromJson<BoardSave>(vTextSave);
            }
            catch (ArgumentException)
            {
                Debug.Log("Erreur dans le chargement de la sauvegarde du board");
                File.Delete(vPath);
            }
        }
        else
        {
            Debug.Log("Pas de fichier board save, creation d'un nouveau");
            if (File.Exists(vPath)) File.Delete(vPath);
        }

        _boardSave = new Dictionary<string, Save>();
        if (vBoardSave != null)
            foreach (Save lSave in vBoardSave.Saves)
                _boardSave.Add(lSave.Player, lSave);

        return _boardSave;
    }

    public static void SaveBoardSave()
    {
        if (_boardSave == null) LoadBoardSave();

        BoardSave vBoardSave = new() { Saves = new() };
        foreach (KeyValuePair<string, Save> lSave in _boardSave)
            vBoardSave.Saves.Add(lSave.Value);

        string vJsonFile = JsonUtility.ToJson(vBoardSave, true);
        string vSavePath = GetSaveFolder() + "/board";

        File.WriteAllText(vSavePath, vJsonFile);
    }

    public static void MajSave(Save pSave)
    {
        if (_boardSave == null) LoadBoardSave();
        if (!_boardSave.ContainsKey(pSave.Player))
            _boardSave.Add(pSave.Player, pSave);
        else _boardSave[pSave.Player] = pSave;
    }

    static string GetSaveFolder()
    {
        string vSavePath;
        vSavePath = Application.persistentDataPath;
        return vSavePath;
    }
}

[Serializable]
public class Save
{
    public int Oboles;
    public int HighScore;
    public string Player;
    public SkinSave Skin;
    public ShopSave ShopSave;

    public Save(int pObole = 0, int pHighScore = 0, string pPlayer = "")
    {
        Oboles = pObole;
        HighScore = pHighScore;
        Player = pPlayer;
        Skin = new();
        ShopSave = new();
    }
}

[Serializable]
public class BoardSave
{
    public List<Save> Saves;
}

[Serializable]
public class SkinSave
{
    public string PlankId = "Pl_1";
    public string SailId = "Sa_1";
}

[Serializable]
public class ShopSave
{
    public List<string> GottenItemsIdList;
}
