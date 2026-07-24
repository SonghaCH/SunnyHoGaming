using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private void Awake()
    {
        if (AudioController.Instance == null)
        {
            return;
        }
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
