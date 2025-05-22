using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class AudioManager : MonoBehaviour
{
    [SerializeField] protected AudioSource gAudioSource;
    [SerializeField] protected AudioMixer gMainMixer;

    private Transform _player;

    Dictionary<int, AudioSource> _audioSources = new Dictionary<int, AudioSource>();
    private int _keepSoundToken = 0;

    List<AudioSource> _fadingSources = new List<AudioSource>();

    void Awake()
    {
        _player = GameObject.Find("Player").transform;
    }

    void Update()
    {
        FadeSound();
    }

    public void PlaySound(AudioClip pAudio, float pVolume, Vector3? pPosition =null)
    {
        AudioSource vAudioSource = Instantiate(gAudioSource, pPosition==null?_player.position:(Vector3)pPosition, Quaternion.identity);

        vAudioSource.outputAudioMixerGroup = gMainMixer.FindMatchingGroups("FX")[0];
        vAudioSource.clip = pAudio;
        vAudioSource.volume = pVolume;
        vAudioSource.Play();

        Destroy(vAudioSource.gameObject, vAudioSource.clip.length);
    }

    public int PlayKeepSound(AudioClip pAudio, float pVolume, Vector3? pPosition =null)
    {
        AudioSource vAudioSource = Instantiate(gAudioSource, pPosition==null?_player.position:(Vector3)pPosition, Quaternion.identity);

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

    public IEnumerator PlayKeepSoundForATime(AudioClip pAudio, float pVolume, float pTime, Vector3? pPosition =null)
    {
        AudioSource vAudioSource = Instantiate(gAudioSource, pPosition==null?_player.position:(Vector3)pPosition, Quaternion.identity);

        vAudioSource.outputAudioMixerGroup = gMainMixer.FindMatchingGroups("FX")[0];
        vAudioSource.clip = pAudio;
        vAudioSource.volume = pVolume;
        vAudioSource.loop = true;
        vAudioSource.Play();

        yield return new WaitForSeconds(pTime);

        _fadingSources.Add(vAudioSource);
    }

    private void FadeSound()
    {
        if (_fadingSources.Count == 0) return;

        List<AudioSource> vAudioToStop = new List<AudioSource>();
        foreach (AudioSource lSource in _fadingSources)
        {
            lSource.volume -= Time.deltaTime * 2;
            if (lSource.volume <= 0)
            {
                lSource.Stop();
                vAudioToStop.Add(lSource);
            }
        }

        foreach (AudioSource lSource in vAudioToStop)
        {
            _fadingSources.Remove(lSource);
            Destroy(lSource.gameObject);
        }
        vAudioToStop.Clear();
    }
}
