using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    [SerializeField] private AudioMixer mixer; // Audio Mixer
    
    [Header("BGM")]
    [SerializeField] private AudioSource bgmSource; // BGM 소스
    [SerializeField] private List<AudioClip> bgmList; // BGM 리스트
    [Header("SFX")]
    [SerializeField] private AudioSource sfxSource; // SFX 소스
    
    
    
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
        
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        PlayBGM(0); // BGM 재생
    }

    // BGM
    public void PlayBGM(int index)
    {
        bgmSource.clip = bgmList[index]; // BGM 소스의 클립을 변경
        bgmSource.Play(); // BGM 소스 재생
    }
    
    public void StopBGM()
    {
        bgmSource.Stop(); // BGM 소스 정지
    }
    
    public void SetBGMVolume(float volume)
    {
        mixer.SetFloat("BGM", Mathf.Log10(volume) * 20); // BGM 볼륨 변경
    }
    
    // SFX
    public void PlaySFX(AudioClip clip = null)
    {
        // sfxSource.PlayOneShot(clip); // SFX 소스 재생
		sfxSource.PlayOneShot(sfxSource.clip); // SFX 소스 재생
    }
    
    public void SetSFXVolume(float volume)
    {
        mixer.SetFloat("SFX", Mathf.Log10(volume) * 20); // SFX 볼륨 변경
    }
    
    public void StopSFX()
    {
        sfxSource.Stop(); // SFX 소스 정지
    }
}
