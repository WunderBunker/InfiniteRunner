using UnityEngine;

public class Relique : MonoBehaviour
{
    Transform _crown;

    void Awake()
    {
        _crown = transform.Find("Crown");
    }

    void Update()
    {
        transform.Rotate(Vector3.up * Time.deltaTime * 50, Space.World);
        _crown.Rotate(Vector3.forward * Time.deltaTime * 50, Space.Self);
    }

    [SerializeField] public string BiomeId;
    void OnTriggerEnter(Collider pCollision)
    {
        BiomesManager.Instance.CaptureRelique();
        Destroy(gameObject);
    }
}
