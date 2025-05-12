using System;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

public class playerControls : MonoBehaviour
{
    public float CurrentSpeed;
    //True si phase de boss  (caméra et directions sur l'axe X sont inversés )
    [NonSerialized] public bool IsBossMode;
    public float CurrentMaxSpeed { get; private set; }

    //Vitesse max en début de run
    [SerializeField] float _maxSpeedInitial;
    //Vitesse max en fin de run mètres
    [SerializeField] float _maxSpeedFinal;

    [SerializeField] float _minSpeed;
    //Temps nécessaire pour un dmpnening à pleine puissance pour passer de maxSpeed à minSpeed
    [SerializeField] float _FullDampTime;
    //Ecart d'angle maximal entre la voile et le vent pour profiter d'une accélération. En dessous de cett valeur*2, le bateau subit le dampening
    [SerializeField] float _maxAngleForFullWind;

    //Temps nécessaire à un changement de lane
    [SerializeField] float _sideMoveLatency;
    //Vitesse de rotation de la voile
    [SerializeField] float _sailTurnSpeed;

    PlayerManager _playerManager;


    LanesManager _laneManager;
    bool _isMovingOnSide;
    bool _goingToNextLane;
    float _XTarget;
    Vector3 _SDVelocityRef;

    bool _sailTaken = false;
    Transform _sailTransform;
    WindManager _windManager;
    float _currentSailAngle;
    float _windDampeningCoef = 0;

    void Start()
    {
        _playerManager = GetComponent<PlayerManager>();
        _laneManager = GameObject.FindGameObjectWithTag("LanesManager").GetComponent<LanesManager>();
        _sailTransform = transform.Find("Sail");
        _windManager = GameObject.FindGameObjectWithTag("WindManager").GetComponent<WindManager>();
        _currentSailAngle = _sailTransform.localEulerAngles.y % 360;
        CurrentSpeed = _maxSpeedInitial;
        CurrentMaxSpeed = _maxSpeedInitial;
    }
    public void OnMoveInput(InputAction.CallbackContext pContext)
    {
        if (pContext.performed)
        {
            Vector2 vInputValue = pContext.ReadValue<Vector2>();
            if (IsBossMode) vInputValue *= -1;

            float? vNewXTarget = _laneManager.GetNextLaneX((int)vInputValue.x);
            _goingToNextLane = vNewXTarget != null;
            _isMovingOnSide = true;
            _XTarget = _goingToNextLane ? (float)vNewXTarget : _XTarget + math.sign(vInputValue.x) * _laneManager.LaneWidth / 2;
        }
    }

    public void OnTakeSail(InputAction.CallbackContext pContext)
    {
        if (pContext.started) _sailTaken = true;
        if (pContext.canceled) _sailTaken = false;
    }

    public void OnTurnSail(InputAction.CallbackContext pContext)
    {
        //Si mouvement trop faible on ne fait rien
        if (!_sailTaken || math.abs(pContext.ReadValue<Vector2>().x) < 0.5f) return;

        //Calcul de la rotation correspondant au mouvement de souris (ou de pad) 
        Vector2 vMoveInput = pContext.ReadValue<Vector2>();
        float vRotation = (vMoveInput.x > 0 ? 1 : -1) * _sailTurnSpeed * Time.deltaTime;

        //Calcul du nouvel angle, remis en angle positif de 0 à 360 (pour faciliter les calculs)
        float vNewAngle = (_currentSailAngle + vRotation) % 360;
        if (vNewAngle < 0) vNewAngle += 360;

        //On controle qu'on reste bien dans les bornes de win (de -Max à +Max mais traduit en positif entre 0 et 360)
        if (vRotation > 0 && vNewAngle > _windManager.MaxWindAngle && _currentSailAngle < 180)
            vNewAngle = _windManager.MaxWindAngle;
        else if (vRotation < 0 && vNewAngle < (360 - _windManager.MaxWindAngle) && vNewAngle > 180)
            vNewAngle = 360 - _windManager.MaxWindAngle;

        //On applique la nouvelle rotation à la voile
        _sailTransform.rotation = Quaternion.Euler(new Vector3(_sailTransform.localEulerAngles.x, vNewAngle, _sailTransform.localEulerAngles.z));
        _currentSailAngle = vNewAngle;

    }


    void Update()
    {
        //Maj de la vitesse maximal tous les 500m (equivaut à interpolation lineaire sur 50000m)
        CurrentMaxSpeed = math.min(math.lerp(_maxSpeedInitial, _maxSpeedFinal, _playerManager.TotalDistance / 5000), _maxSpeedFinal);

        //Calcul du coefficient de prise de vent en fonction de l'angle du vent et de la voile
        //Les angles vont de 0 à 360, on décale leur valeur de façon à ce que l'angle 360-Max soit le 0 (pour pouvoir comparer des valeurs allant de 0 à la distance maximal, soit 2*Max)
        float vEcartMax = 2 * _windManager.MaxWindAngle;
        float vWindAngleFromZero = (_windManager.CurrentAngle + _windManager.MaxWindAngle) % 360;
        vWindAngleFromZero = math.clamp(vWindAngleFromZero, 0, vEcartMax);
        float vSailAngleFromZero = (_currentSailAngle + _windManager.MaxWindAngle) % 360;
        vSailAngleFromZero = math.clamp(vSailAngleFromZero, 0, vEcartMax);

        float vAngleDiff = math.abs(vSailAngleFromZero - vWindAngleFromZero);

        if (IsBossMode || vAngleDiff < _maxAngleForFullWind) _windDampeningCoef = -2;
        else if (vAngleDiff < 2 * _maxAngleForFullWind) _windDampeningCoef = 0;
        else
        {
            //On proportionnalise l'écart en fonction de l'angle min et l'écart max
            vAngleDiff = vAngleDiff * vEcartMax / (vEcartMax - _maxAngleForFullWind) - _maxAngleForFullWind;
            //On détermine notre coeff qui servira au dampening de la vitesse
            _windDampeningCoef = vAngleDiff / vEcartMax;
        }
        DebugTool.DrawDebugOnUI(2, "vAngleDiff : " + vAngleDiff.ToString("0.0"));
        DebugTool.DrawDebugOnUI(3, "wind coeff sail : " + _windDampeningCoef.ToString("0.0"));

        //On applique un éventuel dampening à la  vitesse
        if (_windDampeningCoef > 0) CurrentSpeed = math.max(CurrentSpeed - Time.deltaTime * (CurrentMaxSpeed - _minSpeed) / _FullDampTime * _windDampeningCoef, _minSpeed);
        //Ou on revient au contraire à la vitesse max
        else if (_windDampeningCoef < 0) CurrentSpeed = math.min(CurrentSpeed - Time.deltaTime * (CurrentMaxSpeed - _minSpeed) / _FullDampTime * _windDampeningCoef, CurrentMaxSpeed);
        //Lorsque le coef est à 0 on revient à la vitesse max sans boost particulier
        else CurrentSpeed = math.min(CurrentSpeed + Time.deltaTime * (CurrentMaxSpeed - _minSpeed) / _FullDampTime, CurrentMaxSpeed);


        //On avance tout droit
        transform.position += CurrentSpeed * Time.deltaTime * Vector3.forward;

        //Eventuel deplacement de côté pour changement de lane
        if (_isMovingOnSide)
        {
            Vector3 vTempTargetPosition = new Vector3(_XTarget, transform.position.y, transform.position.z);

            if (_goingToNextLane)
                transform.position = Vector3.SmoothDamp(transform.position, vTempTargetPosition,
                    ref _SDVelocityRef, _sideMoveLatency);
            else
                transform.position = Vector3.MoveTowards(transform.position, vTempTargetPosition, _laneManager.LaneWidth / _sideMoveLatency * Time.deltaTime);

            if (Mathf.Abs(transform.position.x - _XTarget) < 0.05f)
            {
                transform.position = vTempTargetPosition;

                if (_goingToNextLane) _isMovingOnSide = false;
                else
                {
                    _XTarget = (float)_laneManager.GetLaneCenter(_laneManager.CurrentPlayerLane);
                    _goingToNextLane = true;
                }
            }
        }

    }
}
