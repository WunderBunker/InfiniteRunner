using System;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;

//GESTION DES DEPLACEMENTS DU JOUEUR    
public class PlayerControls : MonoBehaviour
{
    public float CurrentSpeed { get; private set; }
    //True si phase de boss  (caméra et directions sur l'axe X sont inversés )
    [NonSerialized] public bool IsBossMode;
    public float CurrentMaxSpeed { get; private set; }

    //Vitesse max en début de run
    [SerializeField] public float MaxSpeedInitial;
    //Vitesse max en fin de run mètres
    [SerializeField] public float MaxSpeedFinal;

    [SerializeField] float _minSpeed;
    //Temps nécessaire pour un dmpnening à pleine puissance pour passer de maxSpeed à minSpeed
    [SerializeField] float _FullDampTime;
    //Ecart d'angle maximal entre la voile et le vent pour profiter d'une accélération. En dessous de cett valeur*2, le bateau subit le dampening
    [SerializeField] float _maxAngleForFullWind;

    //Temps nécessaire à un changement de lane
    [SerializeField] float _sideMoveLatency;
    //Vitesse de rotation de la voile
    [SerializeField] float _sailTurnSpeed;
    //Gonflement de la voile
    [SerializeField] float _minSailZScale;
    [SerializeField] float _maxSailZScale;

    //Sons relatifs à la vitesses et aux déplacement
    [SerializeField] List<AudioClip> _windSounds = new();
    [SerializeField] List<AudioClip> _waterSlideSounds = new();
    [SerializeField] AudioClip[] _sailSounds = new AudioClip[2];

    PlayerManager _playerManager;

    LanesManager _laneManager;
    bool _isMovingOnSide;
    bool _goingToNextLane;
    float _XTarget;
    Vector3 _SDVelocityRef;

    bool _sailTaken = false;
    Transform _sailTransform;
    Color _sailInitColor;
    WindManager _windManager;
    float _currentSailAngle;
    float _windDampeningCoef = 0;


    ParticleSystem _speedPS;
    float _maxSpeedPartEmission;
    float _minSpeedPartRadius;

    ParticleSystem _splashPS;
    float _maxSplashPartEmission;

    bool _isBlockOnLane;
    float _blockTimer;

    byte _currentWindSound = 0;
    byte _currentSailSound = 0;
    int _windSoudnToken = 0;
    int _sailSoudnToken = 0;

    void Start()
    {
        _playerManager = GetComponent<PlayerManager>();
        _laneManager = GameObject.FindGameObjectWithTag("LanesManager").GetComponent<LanesManager>();

        _sailTransform = transform.Find("Sail");
        _sailTransform.localScale = new Vector3(_sailTransform.localScale.x, _sailTransform.localScale.y, _minSailZScale);
        _sailInitColor = _sailTransform.GetComponent<MeshRenderer>().materials[1].color;

        _windManager = GameObject.FindGameObjectWithTag("WindManager").GetComponent<WindManager>();
        _currentSailAngle = _sailTransform.localEulerAngles.y % 360;
        CurrentSpeed = MaxSpeedInitial;
        CurrentMaxSpeed = MaxSpeedInitial;

        _speedPS = GameObject.FindGameObjectWithTag("MainCamera").transform.Find("SpeedEffect").GetComponent<ParticleSystem>();
        _minSpeedPartRadius = _speedPS.shape.radius;
        _maxSpeedPartEmission = _speedPS.emission.rateOverTime.constant;

        _splashPS = transform.Find("Splash").GetComponent<ParticleSystem>();
        _maxSplashPartEmission = _splashPS.emission.rateOverTime.constant;

    }

    //Méthode déclenchée par les inputs de mouvements latéraux
    public void OnMoveInput(InputAction.CallbackContext pContext)
    {
        //Si bloqué (par une sirène) on ne fait rien
        if (_isBlockOnLane) return;

        if (pContext.performed)
        {
            //On récupère le sens de l'input et on l'inverse si on est à l'envers
            Vector2 vInputValue = pContext.ReadValue<Vector2>();
            if (IsBossMode) vInputValue *= -1;

            //Bruit de vague de déplacement
            AudioManager.Instance.PlaySound(_waterSlideSounds[new System.Random().Next(0, _waterSlideSounds.Count)], 1);

            //On récupère la position du centre de la lane vers laquelle on va si elle existe (donc pas au bord, sinon juste la moitié d'une lane dans la direction souhaité avant demi-tour)
            float? vNewXTarget = _laneManager.GetNextLaneX((int)vInputValue.x);
            _goingToNextLane = vNewXTarget != null;
            _isMovingOnSide = true;
            _XTarget = _goingToNextLane ? (float)vNewXTarget : _XTarget + math.sign(vInputValue.x) * _laneManager.LaneWidth / 2;
        }
    }

    //Méthode déclenchée par l'input de prise de contrôle de la voile
    public void OnTakeSail(InputAction.CallbackContext pContext)
    {
        if (pContext.started) _sailTaken = true;
        if (pContext.canceled) _sailTaken = false;
    }

    //Méthode déclenchée par le déplacement de la souris (ou pad) 
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

        //On controle qu'on reste bien dans les bornes du wind (de -Max à +Max) et on traduit en positif entre 0 et 360
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
        if (_isBlockOnLane)
        {
            _blockTimer -= Time.deltaTime;
            if (_blockTimer <= 0) _isBlockOnLane = false;
        }

        //Maj de la vitesse maximal tous les 500m (equivaut à interpolation lineaire sur 50000m)
        CurrentMaxSpeed = math.min(math.lerp(MaxSpeedInitial, MaxSpeedFinal, _playerManager.TotalDistance / 5000), MaxSpeedFinal);

        //Maj du coefficient de décélération
        UpdateWindDampening();

        //Maj de l'avancée vers l'avant et de sffets associés
        UpdateForwardMove();

        //Eventuel déplacement de côté pour changement de lane
        if (_isMovingOnSide) UpdateLaneMove();

    }

    //Permetde ralentir le bateau (en cas de collision avec obstacle)
    public void SlowDown(float pDecreaseCoef)
    {
        CurrentSpeed = math.max(CurrentSpeed - pDecreaseCoef * CurrentSpeed, _minSpeed);
    }


    //Attire et bloque le bateau sur une lane pour un certain temps (sirène)
    public void BlockLane(byte pLane, float pTime)
    {
        _isBlockOnLane = true;
        _blockTimer = pTime;

        //Si il faut attirer le bateau sur la lane on modifie la target su X
        int vDirection = pLane > _laneManager.CurrentPlayerLane ? 1 : pLane < _laneManager.CurrentPlayerLane ? -1 : 0;
        if (vDirection != 0)
        {
            float? vNewXTarget = _laneManager.GetNextLaneX(vDirection);
            _goingToNextLane = vNewXTarget != null;
            _isMovingOnSide = true;
            _XTarget = _goingToNextLane ? (float)vNewXTarget : _XTarget + math.sign(vDirection) * _laneManager.LaneWidth / 2;
        }
    }

    //Maj du coefficient de décélération
    void UpdateWindDampening()
    {
        //Calcul du coefficient de prise de vent en fonction de l'angle du vent et de la voile
        //Les angles vont de 0 à 360, on décale leur valeur de façon à ce que l'angle 360-Max soit le 0 (pour pouvoir comparer des valeurs allant de 0 à la distance maximal, soit 2*Max)
        float vEcartMax = 2 * _windManager.MaxWindAngle;
        float vWindAngleFromZero = (_windManager.CurrentAngle + _windManager.MaxWindAngle) % 360;
        vWindAngleFromZero = math.clamp(vWindAngleFromZero, 0, vEcartMax);
        float vSailAngleFromZero = (_currentSailAngle + _windManager.MaxWindAngle) % 360;
        vSailAngleFromZero = math.clamp(vSailAngleFromZero, 0, vEcartMax);

        float vAngleDiff = math.abs(vSailAngleFromZero - vWindAngleFromZero);
        float vNewCoeff;
        if (IsBossMode || vAngleDiff < _maxAngleForFullWind) vNewCoeff = -2;
        else if (vAngleDiff < 2 * _maxAngleForFullWind) vNewCoeff = 0;
        else
        {
            //On proportionnalise l'écart en fonction de l'angle min et l'écart max
            vAngleDiff = vAngleDiff * vEcartMax / (vEcartMax - _maxAngleForFullWind) - _maxAngleForFullWind;
            //On détermine notre coeff qui servira au dampening de la vitesse
            vNewCoeff = vAngleDiff / vEcartMax;
        }
        if (_windDampeningCoef != vNewCoeff) _windManager.SetPointerColor(vNewCoeff < 0 ? Color.green : (vNewCoeff == 0 ? Color.blue : Color.red));
        _windDampeningCoef = vNewCoeff;

        //Maj bruit de la voile
        byte vNewSailSound = (byte)(vNewCoeff < 0 ? 1 : (vNewCoeff == 0 ? 0 : 3));
        if (vNewSailSound != _currentSailSound)
        {
            if (_sailSoudnToken != 0 && _currentSailSound != 3) AudioManager.Instance.StopKeepSound(_sailSoudnToken);
            if (vNewSailSound != 3) _sailSoudnToken = AudioManager.Instance.PlayKeepSound(_sailSounds[vNewSailSound], 1);
            _currentSailSound = vNewSailSound;
        }

        DebugTool.DrawDebugOnUI(2, "Sail angle : " + _currentSailAngle.ToString("0.0"));
        DebugTool.DrawDebugOnUI(3, "Current speed : " + CurrentSpeed.ToString("0.0") + " / angle diff : " + vAngleDiff.ToString("0.0")
        + " / dampening coef  : " + _windDampeningCoef.ToString("0.0") + " / current max speed : " + CurrentMaxSpeed.ToString("0.0"));
    }

    //Maj de l'avancée vers l'avant et des effets associés
    void UpdateForwardMove()
    {
        //Modifications visuelles de la voile
        _sailTransform.localScale = new Vector3(_sailTransform.localScale.x, _sailTransform.localScale.y,
            math.lerp(_minSailZScale, _maxSailZScale, _windDampeningCoef < 0 ? 1 : 1 - _windDampeningCoef));
        _sailTransform.GetComponent<MeshRenderer>().materials[1].color = Color.Lerp(new Color(0.8f, 0.8f, 0.8f, _sailInitColor.a) , _sailInitColor, _windDampeningCoef < 0 ? 1 : 1 - _windDampeningCoef);

        //On applique un éventuel dampening à la  vitesse
        if (_windDampeningCoef > 0) CurrentSpeed = math.max(CurrentSpeed - Time.deltaTime * (CurrentMaxSpeed - _minSpeed) / _FullDampTime * _windDampeningCoef, _minSpeed);
        //Ou on revient au contraire à la vitesse max
        else if (_windDampeningCoef < 0) CurrentSpeed = math.min(CurrentSpeed - Time.deltaTime * (CurrentMaxSpeed - _minSpeed) / _FullDampTime * _windDampeningCoef, CurrentMaxSpeed);
        //Lorsque le coef est à 0 on revient à la vitesse max sans boost particulier
        else CurrentSpeed = math.min(CurrentSpeed + Time.deltaTime * (CurrentMaxSpeed - _minSpeed) / _FullDampTime, CurrentMaxSpeed);

        if (CurrentSpeed >= MaxSpeedFinal / 4)
        {
            if (!_speedPS.isPlaying) _speedPS.Play();

            ParticleSystem.EmissionModule vEmission = _speedPS.emission;
            vEmission.rateOverTime = new() { constant = math.lerp(0, _maxSpeedPartEmission, CurrentSpeed / MaxSpeedFinal) };
            ParticleSystem.ShapeModule vShape = _speedPS.shape;
            vShape.radius = math.lerp(_minSpeedPartRadius + 7, _minSpeedPartRadius, CurrentSpeed / MaxSpeedFinal);
        }
        else if (_speedPS.isPlaying) _speedPS.Stop();

        ParticleSystem.EmissionModule vSpashEmission = _splashPS.emission;
        vSpashEmission.rateOverTime = new() { constant = math.lerp(0, _maxSplashPartEmission, CurrentSpeed / MaxSpeedFinal) };

        //Maj du bruit des vagues
        byte vNewWindSound = (byte)(CurrentSpeed < MaxSpeedFinal / 3 ? 0 : (CurrentSpeed < MaxSpeedFinal * 2 / 3 ? 1 : 2));
        if (vNewWindSound != _currentWindSound)
        {
            if (_windSoudnToken != 0) AudioManager.Instance.StopKeepSound(_windSoudnToken);
            _windSoudnToken = AudioManager.Instance.PlayKeepSound(_windSounds[vNewWindSound], 1);
            _currentWindSound = vNewWindSound;
        }

        //On avance tout droit
        transform.position += CurrentSpeed * Time.deltaTime * Vector3.forward;
    }

    //Déplacement de côté pour changement de lane
    void UpdateLaneMove()
    {
        Vector3 vTempTargetPosition = new Vector3(_XTarget, transform.position.y, transform.position.z);

        //Soit on va vers une lane en smoothdamp...
        if (_goingToNextLane)
            transform.position = Vector3.SmoothDamp(transform.position, vTempTargetPosition,
                ref _SDVelocityRef, _sideMoveLatency);
        //...soit on est au bord et on va vers un léger déplacement (pas en smoothdamp pour que le demi-our se asse correctement ensuite)
        else
            transform.position = Vector3.MoveTowards(transform.position, vTempTargetPosition, _laneManager.LaneWidth / _sideMoveLatency * Time.deltaTime);


        //On s'arrête à l'emplacement target
        if (Mathf.Abs(transform.position.x - _XTarget) < 0.05f)
        {
            transform.position = vTempTargetPosition;


            if (_goingToNextLane) _isMovingOnSide = false;
            //Si on était au bord on entame le demi-tour vers la lane actuelle
            else
            {
                _XTarget = (float)_laneManager.GetLaneCenter(_laneManager.CurrentPlayerLane);
                _goingToNextLane = true;
            }
        }
    }
}
