using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{
    public static SoundManager instance;

    public AudioSource BgmSource;
    public AudioSource SfxSource;
    public AudioClip[] BGM;
    public AudioClip[] SFX;

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
