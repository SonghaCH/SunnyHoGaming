using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; set; }

    private void Awake()
    {
        Instance = this;
    }

    public void PlayBGM(string soundDataId)
    {
        if (AudioController.Instance == null) return;
        AudioController.Instance.PlayBGM(soundDataId);
    }

    public void PlaySFX(string soundDataId, bool isLoop = false)
    {
        if (AudioController.Instance == null) return;
        AudioController.Instance.PlaySFX(soundDataId, isLoop);
    }

    public void PlaySFX(AudioSource targetSource, string soundDataId, bool isLoop = false)
    {
        if (AudioController.Instance == null) return;
        AudioController.Instance.PlaySFX(targetSource, soundDataId, isLoop);
    }

    public void StopBGM()
    {
        if (AudioController.Instance == null) return;
        AudioController.Instance.StopBGM();
    }

    public void StopSFX()
    {
        if (AudioController.Instance == null) return;
        AudioController.Instance.StopSFX();
    }
}