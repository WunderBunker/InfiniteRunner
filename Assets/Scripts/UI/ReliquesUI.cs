using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ReliquesUI : MonoBehaviour
{
    GameObject _reliqueUI;
    Dictionary<string, Sprite> _biomes = new();

    void Awake()
    {
        _reliqueUI = transform.GetChild(0).gameObject;
    }

    public void InitLayout(List<Biome> pBiome)
    {
        foreach (Biome lBiome in pBiome)
        {
            Instantiate(_reliqueUI, transform);
            _biomes.Add(lBiome.BiomeId, lBiome.ReliqueUI);
        }
        Destroy(_reliqueUI);
    }

    public void CaptureRelique(string pBiomeId)
    {
        int vCpt = 0;

        foreach (KeyValuePair<string, Sprite> lBiome in _biomes)
        {
            if (lBiome.Key == pBiomeId)
            {
                transform.GetChild(vCpt).GetComponent<Image>().sprite = lBiome.Value;
                return;
            }
            vCpt++;
        }
    }
}
