using System;
using System.IO;
using UnityEngine;


public static class SaveManager
{
    static Save _save;

    static string _savePath;

    public static Save LoadSave()
    {
        string vPath;

        Save vScoresSave = null;
        vPath = GetSaveFolder() + "/saveGame";

        string vTextSave;

        if (File.Exists(vPath) && !Equals(vPath, null))
        {
            FileStream vReadStream = new FileStream(vPath, FileMode.Open);
            StreamReader vStreamReader = new StreamReader(vReadStream);
            vTextSave = vStreamReader.ReadToEnd();
            vStreamReader.Close();
            vReadStream.Close();

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
            Debug.Log("No Saves File, creating new one");
            if (File.Exists(vPath)) File.Delete(vPath);
        }

        _save = vScoresSave == null ? new() : vScoresSave;

        return _save;
    }


    public static void SaveSave()
    {
        if (_save == null) LoadSave();

        string vJsonFile = JsonUtility.ToJson(_save);
        string vSavePath = GetSaveFolder() + "/saveGame";

        FileStream vReadStream = new FileStream(vSavePath, FileMode.Create);
        StreamWriter vStreamWriter = new StreamWriter(vReadStream);
        vStreamWriter.WriteLine(vJsonFile);
        vStreamWriter.Close();
        vReadStream.Close();
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
    }

    static string GetSaveFolder()
    {
        string vSavePath;
        vSavePath = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        vSavePath = Path.Combine(vSavePath, "InfiniteRunner");
        Directory.CreateDirectory(vSavePath);
        return vSavePath;
    }
}

public class Save
{
    public int Oboles;
    public int HighScore;
    public Save(int pObole = 0, int pHighScore = 0)
    {
        Oboles = pObole;
        HighScore = pHighScore;
    }
}