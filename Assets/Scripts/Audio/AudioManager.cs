using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

//GESTION DU LANCEMENT ET DE L'ARRET DES SONS DU JEUX
public class AudioManager : MonoBehaviour
{
    static public AudioManager Instance;
    [SerializeField] protected AudioSource _audioSource;
    [SerializeField] protected AudioMixer _mainMixer;
    [SerializeField] protected AudioClip _clickSound;

    private Transform _center;

    Dictionary<int, AudioSource> _audioSources = new Dictionary<int, AudioSource>();
    private int _keepSoundToken = 0;

    List<AudioSource> _fadingSources = new List<AudioSource>();

    void Awake()
    {
        //Application du design Singleton + persistence de scène
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        //Au chargement d'une scène on récupère une position d'écoute par défaut "centrale"
        //Si il y a un player c'est lui sinon on prend la caméra
        _center = GameObject.Find("Player")?.transform;
        if (_center == null) _center = GameObject.FindGameObjectWithTag("MainCamera").transform;
    }

    void Update()
    {
        FadeSound();
    }

    //Joue un son sans boucle
    public void PlaySound(AudioClip pAudio, float pVolume, Vector3? pPosition = null)
    {
        AudioSource vAudioSource = Instantiate(_audioSource, pPosition == null ? _center.position : (Vector3)pPosition, Quaternion.identity);

        vAudioSource.outputAudioMixerGroup = _mainMixer.FindMatchingGroups("FX")[0];
        vAudioSource.clip = pAudio;
        vAudioSource.volume = pVolume;
        vAudioSource.Play();

        Destroy(vAudioSource.gameObject, vAudioSource.clip.length);
    }

    //Joue un son avec boucle. Renvoie un token permettant à l'objet appelant de demander la coupure du son 
    public int PlayKeepSound(AudioClip pAudio, float pVolume, Vector3? pPosition = null)
    {
        AudioSource vAudioSource = Instantiate(_audioSource, pPosition == null ? _center.position : (Vector3)pPosition, Quaternion.identity);

        vAudioSource.outputAudioMixerGroup = _mainMixer.FindMatchingGroups("FX")[0];
        vAudioSource.clip = pAudio;
        vAudioSource.volume = pVolume;
        vAudioSource.loop = true;
        vAudioSource.Play();
        _keepSoundToken += 1;

        _audioSources.Add(_keepSoundToken, vAudioSource);

        return _keepSoundToken;
    }

    //Coupure d'un son qui boucle
    public void StopKeepSound(int pToken)
    {
        //On bascule en fait le son dans une liste dont les éléments sont tûs progressivement
        _fadingSources.Add(_audioSources[pToken]);
        _audioSources.Remove(pToken);
    }

    //Joue un son qui boucle mais s'arrête de lui-même au bout d'un certain temps
    public IEnumerator PlayKeepSoundForATime(AudioClip pAudio, float pVolume, float pTime, Vector3? pPosition = null)
    {
        AudioSource vAudioSource = Instantiate(_audioSource, pPosition == null ? _center.position : (Vector3)pPosition, Quaternion.identity);

        vAudioSource.outputAudioMixerGroup = _mainMixer.FindMatchingGroups("FX")[0];
        vAudioSource.clip = pAudio;
        vAudioSource.volume = pVolume;
        vAudioSource.loop = true;
        vAudioSource.Play();

        yield return new WaitForSeconds(pTime);

        _fadingSources.Add(vAudioSource);
    }

    //Joue le son de click sur un bouton (son très utilisé donc on en fait une méthode à part sans paramètres)
    public void PlayClickSound()
    {
        PlaySound(_clickSound, 1);
    }

    //Diminution progressive des sons à couper puis destruction des sources
    private void FadeSound()
    {
        if (_fadingSources.Count == 0) return;

        //On dimminue progressivement
        List<AudioSource> vAudioToStop = new List<AudioSource>();
        foreach (AudioSource lSource in _fadingSources)
        {
            if (lSource == null) vAudioToStop.Add(lSource);
            else
            {
                lSource.volume -= Time.deltaTime * 2;
                if (lSource.volume <= 0)
                {
                    lSource.Stop();
                    vAudioToStop.Add(lSource);
                }
            }
        }

        //On supprime les sons dont le volume à atteint 0
        foreach (AudioSource lSource in vAudioToStop)
        {
            _fadingSources.Remove(lSource);
            if (lSource != null) Destroy(lSource.gameObject);
        }
        vAudioToStop.Clear();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
