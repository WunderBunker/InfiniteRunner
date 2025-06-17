using UnityEngine;

public class Relique : MonoBehaviour
{
    [SerializeField] public string BiomeId;
    void OnTriggerEnter(Collider pCollision)
    {
        BiomesManager.Instance.CaptureRelique();
        Destroy(gameObject);
    }
}
