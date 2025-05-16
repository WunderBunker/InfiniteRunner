using UnityEngine;

public class BiomesManager : MonoBehaviour
{
    public string CurrentBiomeId { get; private set; }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ChangeBiome("Egee");
    }

    public void ChangeBiome(string pBiomeId)
    {
        CurrentBiomeId = pBiomeId;
    }
    // Update is called once per frame
    void Update()
    {

    }
}
