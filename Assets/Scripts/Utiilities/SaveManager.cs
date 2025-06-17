using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

//GESTION DE LA SAUVEGARDE DES DONNEES
public static class SaveManager
{
    //Champ temporaire permettant de simuler en dur un identifiant pour le joueur (notamment pour le leaderBoard)
    static public string _player = "Wunder";
    static PlayerSave _save;

    //Chargement de la sauvegarde du joueur (Highscore, oboles, comestiques, items achetés...)
    public static PlayerSave LoadPlayerSave()
    {
        PlayerSave vScoresSave = null;
        string vPath = GetSaveFolder() + "/saveGame_" + _player;

        string vTextSave;

        //On récupère la sauvegarde depuis fichier JSon si existe
        if (vPath != null && File.Exists(vPath))
        {
            vTextSave = File.ReadAllText(vPath);

            try
            {
                vScoresSave = JsonUtility.FromJson<PlayerSave>(vTextSave);
            }
            catch (ArgumentException)
            {
                Debug.Log("Erreur dans le chargement de la sauvegarde");
                File.Delete(vPath);
            }
        }
        //Sinon on supprime l'éventuel fichier corrompus et on en créera une nouvelle
        else
        {
            Debug.Log("Pas de fichier save, creation d'un nouveau");
            if (File.Exists(vPath)) File.Delete(vPath);
        }

        //Création d'une nouvelle sauverde si besoin
        if (vScoresSave == null || vScoresSave.Player != _player) _save = new() { Player = _player };
        else _save = vScoresSave;

        return _save;
    }

    //Renvoie la save actuellement chargée (load la save si besoin)
    public static PlayerSave GetPlayerSave()
    {
        if (_save != null) return _save;
        else return LoadPlayerSave();
    }

    //Enregistrement de la sauvegarde du joueur dans un json local
    public static void SavePlayerSave()
    {
        if (_save == null) LoadPlayerSave();

        string vJsonFile = JsonUtility.ToJson(_save);
        string vSavePath = GetSaveFolder() + "/saveGame_" + _player;

        //On met également à jour le leaderBoard concernant les données de ce joueur
        BoardManager.MajSave(_save);
        BoardManager.SaveBoardSave();

        File.WriteAllText(vSavePath, vJsonFile);
    }

    //Renvoie l'emplacemment pour la sauvegarde des données persistentes
    static string GetSaveFolder()
    {
        string vSavePath;
        vSavePath = Application.persistentDataPath;
        return vSavePath;
    }

    // *** Méthodes de maj des données de la sauvegarde ***

    public static void AddOboles(int pNb)
    {
        if (_save == null) LoadPlayerSave();

        _save.Oboles = Math.Max(_save.Oboles + pNb, 0);
    }

    public static void MajScore(int pScore)
    {
        if (_save == null) LoadPlayerSave();
        if (pScore > _save.HighScore)
            _save.HighScore = pScore;
        BoardManager.MajSave(_save);
    }

    public static void ChangeSkinPlank(string pPlankId)
    {
        if (_save == null) LoadPlayerSave();
        _save.Skin.PlankId = pPlankId;
    }
    public static void ChangeSkinSail(string pSailId)
    {
        if (_save == null) LoadPlayerSave();
        _save.Skin.SailId = pSailId;
    }

    public static void AddGottenItem(string pItemId)
    {
        if (!_save.ShopSave.GottenItemsIdList.Contains(pItemId)) _save.ShopSave.GottenItemsIdList.Add(pItemId);
    }

    //***                           ***
}


//GESTION DE LA SAUVEGARDE DES DONNEES DANS LE LEADERBOARD
public static class BoardManager
{
    static Dictionary<string, PlayerSave> _boardSave;

    //Chargement des données du leaderboard à partir d'un json local sous forme d'un dictionnary (plus pratique que la liste dans la classe sérialisée)
    public static Dictionary<string, PlayerSave> LoadBoardSave()
    {
        BoardSave vBoardSave = null;
        string vPath = GetSaveFolder() + "/board";
        string vTextSave;

        //Récupération ou création de la sauveagrde
        if (vPath != null && File.Exists(vPath))
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

        _boardSave = new Dictionary<string, PlayerSave>();

        //Conversion de la liste de sauvegarde en un dictionnary (plus facilement manipulable)
        if (vBoardSave != null)
            foreach (PlayerSave lSave in vBoardSave.Saves)
                _boardSave.Add(lSave.Player, lSave);

        return _boardSave;
    }

    //Sauveagrde des données dans un Json local
    public static void SaveBoardSave()
    {
        if (_boardSave == null) LoadBoardSave();

        //Conversion du Dictionnary en une liste dans une classe sérialisable
        BoardSave vBoardSave = new() { Saves = new() };
        foreach (KeyValuePair<string, PlayerSave> lSave in _boardSave)
            vBoardSave.Saves.Add(lSave.Value);

        string vJsonFile = JsonUtility.ToJson(vBoardSave, true);
        string vSavePath = GetSaveFolder() + "/board";

        File.WriteAllText(vSavePath, vJsonFile);
    }

    //Maj des données d'un joueur dans le leaderboard
    public static void MajSave(PlayerSave pSave)
    {
        if (_boardSave == null) LoadBoardSave();

        //Ajout de la sauvegarde dans le board si nouveau joueur
        if (!_boardSave.ContainsKey(pSave.Player))
            _boardSave.Add(pSave.Player, pSave);
        //Sinon remplacement de l'existente
        else _boardSave[pSave.Player] = pSave;
    }

    static string GetSaveFolder()
    {
        string vSavePath;
        vSavePath = Application.persistentDataPath;
        return vSavePath;
    }
}

//Ensemble des données persistentes concernant le joueur (Highscore, oboles, comestiques, items achetés...)
[Serializable]
public class PlayerSave
{
    public int Oboles;
    public int HighScore;
    public string Player;
    public SkinSave Skin;
    public ShopSave ShopSave;

    public PlayerSave(int pObole = 0, int pHighScore = 0, string pPlayer = "")
    {
        Oboles = pObole;
        HighScore = pHighScore;
        Player = pPlayer;
        Skin = new();
        ShopSave = new();
    }
}

//Données du leaderboard => liste des sauvegarde de tous les joueurs
[Serializable]
public class BoardSave
{
    public List<PlayerSave> Saves;
}

//Données au sein de la sauvegarde joueur concernant les comestiques équipés
[Serializable]
public class SkinSave
{
    public string PlankId = "Pl_1";
    public string SailId = "Sa_1";
}

//Données au sein de la sauvegarde joueur contenat les items achetés dans le shop
[Serializable]
public class ShopSave
{
    public List<string> GottenItemsIdList;
}
