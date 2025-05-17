using UnityEngine;

public class Gate : MonoBehaviour
{
    string _biomeId;

    BiomesManager _BM;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _BM = GameObject.FindGameObjectWithTag("BiomesManager").GetComponent<BiomesManager>();
        Biome vNewBiome = _BM.GetRandomNewBiome();
        _biomeId = vNewBiome.BiomeId;
        GetComponent<MeshRenderer>().material.SetColor("_Color", vNewBiome.GateColor);
    }

    void OnTriggerEnter(Collider pOther)
    {
        if (pOther.CompareTag("Player"))
            _BM.ChangeBiome(_biomeId);
    }

}
