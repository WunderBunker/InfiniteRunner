using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    static public AudioManager Instance;
    [SerializeField] protected AudioSource gAudioSource;
    [SerializeField] protected AudioMixer gMainMixer;
    [SerializeField] protected AudioClip _clickSound;

    private Transform _center;

    Dictionary<int, AudioSource> _audioSources = new Dictionary<int, AudioSource>();
    private int _keepSoundToken = 0;

    List<AudioSource> _fadingSources = new List<AudioSource>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        _center = GameObject.Find("Player")?.transform;
        if (_center == null) _center = GameObject.FindGameObjectWithTag("MainCamera").transform;
    }

    void Update()
    {
        FadeSound();
    }

    public void PlaySound(AudioClip pAudio, float pVolume, Vector3? pPosition = null)
    {
        AudioSource vAudioSource = Instantiate(gAudioSource, pPosition == null ? _center.position : (Vector3)pPosition, Quaternion.identity);

        vAudioSource.outputAudioMixerGroup = gMainMixer.FindMatchingGroups("FX")[0];
        vAudioSource.clip = pAudio;
        vAudioSource.volume = pVolume;
        vAudioSource.Play();

        Destroy(vAudioSource.gameObject, vAudioSource.clip.length);
    }

    public int PlayKeepSound(AudioClip pAudio, float pVolume, Vector3? pPosition = null)
    {
        AudioSource vAudioSource = Instantiate(gAudioSource, pPosition == null ? _center.position : (Vector3)pPosition, Quaternion.identity);

        vAudioSource.outputAudioMixerGroup = gMainMixer.FindMatchingGroups("FX")[0];
        vAudioSource.clip = pAudio;
        vAudioSource.volume = pVolume;
        vAudioSource.loop = true;
        vAudioSource.Play();
        _keepSoundToken += 1;

        _audioSources.Add(_keepSoundToken, vAudioSource);

        return _keepSoundToken;
    }

    public void StopKeepSound(int pToken)
    {
        _fadingSources.Add(_audioSources[pToken]);
        _audioSources.Remove(pToken);
    }

    public IEnumerator PlayKeepSoundForATime(AudioClip pAudio, float pVolume, float pTime, Vector3? pPosition = null)
    {
        AudioSource vAudioSource = Instantiate(gAudioSource, pPosition == null ? _center.position : (Vector3)pPosition, Quaternion.identity);

        vAudioSource.outputAudioMixerGroup = gMainMixer.FindMatchingGroups("FX")[0];
        vAudioSource.clip = pAudio;
        vAudioSource.volume = pVolume;
        vAudioSource.loop = true;
        vAudioSource.Play();

        yield return new WaitForSeconds(pTime);

        _fadingSources.Add(vAudioSource);
    }

    public void PlayClickSound()
    {
        PlaySound(_clickSound, 1);
    }

    private void FadeSound()
    {
        if (_fadingSources.Count == 0) return;

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

        foreach (AudioSource lSource in vAudioToStop)
        {
            _fadingSources.Remove(lSource);
            if(lSource != null) Destroy(lSource.gameObject);
        }
        vAudioToStop.Clear();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}
