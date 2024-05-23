using UnityEngine;

public class SoundManager : Singleton<SoundManager>
{
    public AudioSource[] BgmSources;
    public AudioSource[] SfxSources;
    
    public void PlayBgm(int index)
    {
        if (index >= 0 && index < BgmSources.Length)
        {
            foreach (var bgm in BgmSources)
            {
                bgm.Stop();
            }
            BgmSources[index].Play();
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogWarning("Invalid BGM index.");
#endif
            
        }
    }
    
    public void StopBgm()
    {
        foreach (var bgm in BgmSources)
        {
            bgm.Stop();
        }
    }
    public void PlaySfx(int index)
    {
        if (index >= 0 && index < SfxSources.Length)
        {
            SfxSources[index].PlayOneShot(SfxSources[index].clip);
        }
        else
        {
            return;
#if UNITY_EDITOR
            Debug.LogWarning("Invalid SFX index.");
#endif
            
        }
    }
    
    public void StopSfx(int index)
    {
        if (index >= 0 && index < SfxSources.Length)
        {
            SfxSources[index].Stop();
        }
        else
        {
#if UNITY_EDITOR
            Debug.LogWarning("Invalid SFX index.");
#endif
           
        }
    }
}
