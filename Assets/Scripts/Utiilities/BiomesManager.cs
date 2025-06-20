using System.Collections.Generic;
using UnityEngine;

//GESTION DES BIOMES 
public class BiomesManager : MonoBehaviour
{
    public static BiomesManager Instance;

    public string CurrentBiomeId { get; private set; }
    public GameObject CurrentBoss { get; private set; }
    public GameObject CurrentRelique { get; private set; }
    [SerializeField] List<Biome> _biomes = new();
    [SerializeField] AudioClip _relicSound;
    [SerializeField] AudioClip _allRelicsSound;
    List<string> _capturedReliques = new();

    PatternsManager _PM;
    Material _waterMaterial;
    GameObject _mainCanvas;
    ReliquesUI _reliquesUI;


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
        _mainCanvas = GameObject.FindGameObjectWithTag("MainCanvas");
        _reliquesUI = _mainCanvas.transform.Find("Reliques").GetComponent<ReliquesUI>();
        _reliquesUI.InitLayout(_biomes);
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
                if (_mainCanvas.transform.Find("BossIndicator").childCount > 0)
                    Destroy(_mainCanvas.transform.Find("BossIndicator").GetChild(0).gameObject);
                if (_mainCanvas.transform.Find("Filters").Find("BossSpecific").childCount > 0)
                    Destroy(_mainCanvas.transform.Find("Filters").Find("BossSpecific").GetChild(0).gameObject);

                //Boss
                Destroy(CurrentBoss);
                CurrentBoss = Instantiate(lBiome.Boss, transform.Find("Boss").position, lBiome.Boss.transform.rotation, transform.Find("Boss"));

                //Relique
                CurrentRelique = _capturedReliques.Contains(CurrentBiomeId) ? null : lBiome.Relique;

                break;
            }
    }

    //Récupération d'un biome alléatoire différent du biome actuel
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
                Debug.Log("vho boucle infinie while biome manager");
                break;
            }

            vNewBiome = _biomes[vRandom.Next(0, _biomes.Count)];
            vNewId = vNewBiome.BiomeId;
        }

        return vNewBiome;
    }

    //Ajout d'une barre de bruit si le boss actuel le gère
    public void RaiseNoise(byte pValue)
    {
        if (CurrentBoss.GetComponent<Boss>() is INoiseSensitive) CurrentBoss.GetComponent<INoiseSensitive>().AddNoise(pValue);
    }

    //Récupération de la relique du biome
    public void CaptureRelique()
    {
        if (!_capturedReliques.Contains(CurrentBiomeId))
        {
            _capturedReliques.Add(CurrentBiomeId);
            _reliquesUI.CaptureRelique(CurrentBiomeId);
        }

        PlayerManager vPlayerManager = GameObject.FindGameObjectWithTag("Player").GetComponent<PlayerManager>();
        //Si plus de relique alors Jackpot
        if (_capturedReliques.Count == _biomes.Count)
        {
            vPlayerManager.AddPonctualScore(5000);
            vPlayerManager.AddOboles(100);
            AudioManager.Instance.PlaySound(_allRelicsSound, 1f);
        }
        //Sinon 500
        else
        {
            vPlayerManager.AddPonctualScore(500);
            AudioManager.Instance.PlaySound(_relicSound, 1f);
        }
    }
}
