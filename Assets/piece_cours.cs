using System.Collections;
using UnityEngine;

public class piece_cours : MonoBehaviour
{
    [SerializeField] float _roattionSpeed;

    MeshRenderer _meshRenderer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _meshRenderer = GetComponent<MeshRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        transform.Rotate(Vector3.up * Time.deltaTime * _roattionSpeed);
    }
    IEnumerator Wink()
    {
        for (int i = 0; i < 10; i++)
        {
            _meshRenderer.enabled = false;
            yield return new WaitForSeconds(0.1f);
            _meshRenderer.enabled = true;
            yield return new WaitForSeconds(0.1f);
        }
        Cours_Global.Instance.AddScore(1);
        Destroy(gameObject);
    }

    void OnTriggerEnter(Collider other)
    {
        StartCoroutine(Wink());
    }
}
