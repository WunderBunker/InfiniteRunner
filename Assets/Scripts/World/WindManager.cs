using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class WindManager : MonoBehaviour
{
    public float MaxWindAngle;
    public float CurrentAngle { get; private set; } = 0;
    [SerializeField] float _changeAngleFrequency;
    [SerializeField] float _changeAngleLatency;
    float _changeAngleTimer;
    bool _isMovingAngle;
    float _targetAngle;
    System.Random _random = new();
    Quaternion _refQuaternionDeriv;
    Transform _compassPointerTransform;
    Image _compassPointeImage;
    Transform _windParticles;
    Transform _playerTransform;
    float _initDistanceFromPlayer;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        _compassPointerTransform = GameObject.FindGameObjectWithTag("MainCanvas").transform.Find("Compass").Find("Pointer");
        _compassPointeImage = _compassPointerTransform.Find("Pointe").GetComponent<Image>();
        _changeAngleTimer = _changeAngleFrequency;

        CurrentAngle = (-_compassPointerTransform.localEulerAngles.z) % 360;
        if (CurrentAngle < 0) CurrentAngle += 360;

        _windParticles = transform.Find("Particles");
        _playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
        _initDistanceFromPlayer = _windParticles.transform.position.z - _playerTransform.position.z;
        _windParticles.localRotation = Quaternion.Euler(new Vector3(_windParticles.localEulerAngles.x, CurrentAngle, _windParticles.localEulerAngles.z));
        _windParticles.transform.position = new Vector3(_playerTransform.position.x + _initDistanceFromPlayer * (float)math.cos(CurrentAngle * Math.PI / 180 + math.PI / 2), _windParticles.transform.position.y, _playerTransform.position.z + _initDistanceFromPlayer * (float)math.sin(CurrentAngle * Math.PI / 180 + math.PI / 2));
    }

    // Update is called once per frame
    void Update()
    {
        _changeAngleTimer -= Time.deltaTime;

        //Si timer à 0 alors on change la direction du vent
        if (_changeAngleTimer <= 0)
        {
            _changeAngleTimer = _changeAngleFrequency;

            //Nouvel angle compris entre -Max et +Max mais remis en angle positif de 0 à 360 (pour faciliter les calculs)
            _targetAngle = _random.Next((int)-MaxWindAngle, (int)MaxWindAngle);
            if (_targetAngle < 0) _targetAngle += 360;
            _isMovingAngle = true;
        }

        //Si le vent est en train de tourner on maj la rotation
        if (_isMovingAngle)
        {
            //On prend ici l'opposé de l'angle pour que la rotation soit raccord avec celle de la voile (l'axe z étant en avant et l'axe y vers le haut)
            Quaternion vRotationTarget = Quaternion.Euler(new Vector3(_compassPointerTransform.localRotation.x, _compassPointerTransform.localRotation.y, -_targetAngle));
            //On calcul la nouvelle rotation via un smoothDamp vers la target
            _compassPointerTransform.localRotation = Tools.QuaternionSmoothDamp(_compassPointerTransform.localRotation,
                vRotationTarget, ref _refQuaternionDeriv, _changeAngleLatency);

            CurrentAngle = (-_compassPointerTransform.localEulerAngles.z) % 360;
            if (CurrentAngle < 0) CurrentAngle += 360;

            if (math.abs(_targetAngle - CurrentAngle) <= 1)
            {
                //On prend ici l'opposé de l'angle pour que la rotation soit raccord avec celle de la voile (l'axe z étant en avant et l'axe y vers le haut)
                _compassPointerTransform.localRotation = Quaternion.Euler(new Vector3(_compassPointerTransform.localRotation.x, _compassPointerTransform.localRotation.y, -_targetAngle));
                CurrentAngle = _targetAngle;
                _isMovingAngle = false;
                DebugTool.DrawDebugOnUI(1, "Wind angle : " + CurrentAngle.ToString("0.0"));
            }
            //Maj de l'orientation des particules de vent
            _windParticles.localRotation = Quaternion.Euler(new Vector3(_windParticles.localEulerAngles.x, CurrentAngle, _windParticles.localEulerAngles.z));
        }
        //maj de la position des particules de vent sur axe z
        _windParticles.transform.position = new Vector3(_playerTransform.position.x - _initDistanceFromPlayer * (float)math.cos(CurrentAngle * Math.PI / 180 + math.PI / 2), _windParticles.transform.position.y, _playerTransform.position.z + _initDistanceFromPlayer * (float)math.sin(CurrentAngle * Math.PI / 180 + math.PI / 2));
    }

    public void SetPointerColor(Color pColor) =>  _compassPointeImage.color = pColor;
       
}
