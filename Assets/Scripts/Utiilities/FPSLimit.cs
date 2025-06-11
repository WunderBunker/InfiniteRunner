using UnityEngine;

public class FPSLimit : MonoBehaviour
{
    [SerializeField] int _targetFPS = 60;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Application.targetFrameRate = _targetFPS;
    }
}
