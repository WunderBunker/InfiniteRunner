using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class Cours_player : MonoBehaviour
{
    [SerializeField] float _speed;
    Material _material;
    Color _initColor;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _material = new(GetComponent<MeshRenderer>().material);
        GetComponent<MeshRenderer>().material = _material;
        _initColor = _material.GetColor("_Color");
    }

    void Update()
    {
        Vector2 vDirection = Vector2.zero;
        if (Keyboard.current.rightArrowKey.isPressed)
            vDirection += Vector2.right;
        if (Keyboard.current.leftArrowKey.isPressed)
            vDirection += Vector2.left;
        if (Keyboard.current.upArrowKey.isPressed)
            vDirection += Vector2.up;
        if (Keyboard.current.downArrowKey.isPressed)
            vDirection += Vector2.down;

        Vector2 vDelta = vDirection * _speed * Time.deltaTime;
        transform.position += new Vector3(vDelta.x, 0, vDelta.y);
    }

    public void ChangeColor()
    {
        _material.SetColor("_Color", Color.red);
    }
    public void ChangeBackColor()
    {
        _material.SetColor("_Color", _initColor);
    }


}
