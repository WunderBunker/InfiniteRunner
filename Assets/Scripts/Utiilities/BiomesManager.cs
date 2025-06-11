using System.Collections.Generic;
using UnityEngine;

public class BiomesManager : MonoBehaviour
{
    public static BiomesManager Instance;

    public string CurrentBiomeId { get; private set; }
    public GameObject CurrentBoss { get; private set; }
    [SerializeField] List<Biome> _biomes = new();

    PatternsManager _PM;
    Material _waterMaterial;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _PM = GameObject.FindGameObjectWithTag("PatternsManager").GetComponent<PatternsManager>();
        _waterMaterial = GameObject.FindGameObjectWithTag("WaterPlane").GetComponent<MeshRenderer>().material;
        ChangeBiome("Egee");
    }

    public void ChangeBiome(string pBiomeId)
    {
        CurrentBiomeId = pBiomeId;

        foreach (Biome lBiome in _biomes)
            if (lBiome.BiomeId == CurrentBiomeId)
            {
                //Patterns
                _PM.SetNewBiome(lBiome.Patterns);

                //Visuels
                _waterMaterial.SetColor("_Color", lBiome.WaterColor);
                _waterMaterial.SetColor("HorizonColor", lBiome.HorizonColor);
                RenderSettings.skybox.SetColor("_SkyTint", lBiome.SkyColor);
                RenderSettings.skybox.SetColor("_GroundColor", lBiome.HorizonColor);
                RenderSettings.fogColor = lBiome.HorizonColor;

                //UI
                if (GameObject.FindGameObjectWithTag("MainCanvas").transform.Find("BossIndicator").childCount > 0)
                    Destroy(GameObject.FindGameObjectWithTag("MainCanvas").transform.Find("BossIndicator").GetChild(0).gameObject);
                if (GameObject.FindGameObjectWithTag("MainCanvas").transform.Find("Filters").Find("BossSpecific").childCount > 0)
                    Destroy(GameObject.FindGameObjectWithTag("MainCanvas").transform.Find("Filters").Find("BossSpecific").GetChild(0).gameObject);

                //Boss
                Destroy(CurrentBoss);
                CurrentBoss = Instantiate(lBiome.Boss, transform.Find("Boss").position, lBiome.Boss.transform.rotation, transform.Find("Boss"));

                break;
            }
    }

    public Biome GetRandomNewBiome()
    {
        Biome vNewBiome = null;
        string vNewId = CurrentBiomeId;
        System.Random vRandom = new();

        int vWhileCount = 0;
        while (vNewId == CurrentBiomeId)
        {
            vWhileCount++;
            if (vWhileCount > 100)
            {
                Debug.Log("vho boucle infie while biome manager");
                break;
            }

            vNewBiome = _biomes[vRandom.Next(0, _biomes.Count)];
            vNewId = vNewBiome.BiomeId;
        }

        return vNewBiome;
    }

    public void RaiseNoise(byte pValue)
    {
        if (CurrentBoss.GetComponent<Boss>() is INoiseSensitive) CurrentBoss.GetComponent<INoiseSensitive>().AddNoise(pValue);
    }
}
