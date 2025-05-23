using UnityEngine;

public class Gate : MonoBehaviour
{
    [SerializeField] AudioClip _gateSound;
    [SerializeField] AudioClip _gatePassedSound;

    string _biomeId;

    BiomesManager _BM;

    int _soundToken;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _BM = GameObject.FindGameObjectWithTag("BiomesManager").GetComponent<BiomesManager>();
        Biome vNewBiome = _BM.GetRandomNewBiome();
        _biomeId = vNewBiome.BiomeId;

        Material vMat = new(transform.Find("GateModel").Find("Plane").GetComponent<MeshRenderer>().material);
        vMat.SetColor("_Color", vNewBiome.GateColor);
        transform.Find("GateModel").Find("Plane").GetComponent<MeshRenderer>().material = vMat;

        ParticleSystem.MainModule vMain = gameObject.GetComponent<ParticleSystem>().main;
        vMain.startColor = new ParticleSystem.MinMaxGradient() { color = vNewBiome.GateColor };

        _soundToken = AudioManager.Instance.PlayKeepSound(_gateSound, 1, transform.position);
        transform.Find("Light").GetComponent<Light>().color = vNewBiome.GateColor;
    }

    void OnTriggerEnter(Collider pOther)
    {
        if (pOther.CompareTag("Player"))
        {
            AudioManager.Instance.PlaySound(_gatePassedSound, 1);
            _BM.ChangeBiome(_biomeId);
        }
    }

    void OnDestroy()
    {
        AudioManager.Instance.StopKeepSound(_soundToken);
    }

}
