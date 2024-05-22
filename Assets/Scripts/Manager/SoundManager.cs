using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager instance;

    public AudioSource BgmSource;
    public AudioSource SfxSource;
    public AudioClip[] BGM;
    public AudioClip[] SFX;
    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
    }
    public void PlayBgm(int index)
    {
        BgmSource.clip = BGM[index];
        BgmSource.Play();
    }
    public void PlaySfx(int index)
    {
        SfxSource.PlayOneShot(SFX[index]);
    }
}
