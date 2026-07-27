using UnityEngine;
using UnityEngine.Video;

public class FailVideoPlayerUI : UIBase
{
    private VideoPlayer _videoPlayer;

    private void Awake()
    {
        _videoPlayer = GetComponent<VideoPlayer>();
    }

    private void OnEnable()
    {
        if (_videoPlayer != null)
        {
            _videoPlayer.loopPointReached += OnVideoFinished;
        }
    }

    private void OnDisable()
    {
        if (_videoPlayer != null)
        {
            _videoPlayer.loopPointReached -= OnVideoFinished;
        }
    }

    private void OnVideoFinished(VideoPlayer vp)
    {
        gameObject.SetActive(false);
    }
}