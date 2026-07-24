using System.Threading;
using UnityEngine;

public class AudioController : MonoBehaviour
{
    [SerializeField] private AudioSource _audioBGMSource;
    [SerializeField] private AudioSource _audioSFXSource;

    public static AudioController Instance { get; private set; }

    public float BGMVolume
    {
        get { return _audioBGMSource.volume; }
        set { _audioBGMSource.volume = value; }
    }

    public float SFXVolume
    {
        get { return _audioSFXSource.volume; }
        set { _audioSFXSource.volume = value; }
    }

    private CancellationTokenSource _bgmCts;

    private void Awake()
    {
        if (Instance != null)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void PlayBGM(string soundDataId)
    {
        if (_bgmCts != null)
        {
            _bgmCts.Cancel();
            _bgmCts.Dispose();
        }

        _bgmCts = new CancellationTokenSource();

        GameUtil.LoadAndPlayAudioClip(_audioBGMSource, soundDataId, isLoop: true, cancellationToken: _bgmCts.Token).Forget();
    }

    public void PlaySFX(string soundDataId)
    {
        GameUtil.LoadAndPlayAudioClip(_audioSFXSource, soundDataId, cancellationToken: _bgmCts.Token).Forget();
    }

    public void StopBGM()
    {
        _audioBGMSource.Stop();
    }

    public void StopSFX()
    {
        _audioSFXSource.Stop();
    }
}
