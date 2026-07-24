using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; set; }

    private void Awake()
    {
        if (AudioController.Instance == null)
        {
            return;
        }

        Instance = this;
    }

    public void PlayBGM(string soundDataId)
    {
        AudioController.Instance.PlayBGM(soundDataId);
    }

    public void PlaySFX(string soundDataId)
    {
        AudioController.Instance.PlaySFX(soundDataId);
    }

    public void StopBGM()
    {
        AudioController.Instance.StopBGM();
    }

    public void StopSFX()
    {
        AudioController.Instance.StopSFX();
    }
}
