using UnityEngine;
using Project.Core.Managers;
using Cysharp.Threading.Tasks;
using System;

public class MainUiSoundView : MonoBehaviour
{
    [Header("--- BGM ---")]
    [SerializeField] private AudioClip _mainBgmClip;

    [Header("--- SFX Clips ---")]
    [SerializeField] private AudioClip _clipA;
    [SerializeField] private AudioClip _clipB;
    [SerializeField] private AudioClip _clipC;

    // Presenter가 AudioManager에 직접 접근하지 않도록 속성 제공
    public float BgmVolume => AudioManager.Instance != null ? AudioManager.Instance.BgmVolume : 0.5f;
    public float SfxVolume => AudioManager.Instance != null ? AudioManager.Instance.SfxVolume : 0.5f;

    // 딜레이 재생 로직을 이쪽으로 이동
    public async UniTaskVoid PlayMainBgmWithDelay(float delaySeconds)
    {
        await UniTask.Delay(TimeSpan.FromSeconds(delaySeconds));
        if (_mainBgmClip != null) AudioManager.Instance?.PlayBGM(_mainBgmClip);
    }

    public void StopBgm() => AudioManager.Instance?.StopBGM();

    // 볼륨 조절 로직 위임
    public void SetVolume(string type, float vol) => AudioManager.Instance?.SetVolume(type, vol);

    // 설정 저장 로직 위임
    public void SaveAudioSettings() => AudioManager.Instance?.AudioSaveSettings();

    // 효과음 재생 메서드들
    public void PlaySfxA() => Play(_clipA);
    public void PlaySfxB() => Play(_clipB);
    public void PlaySfxC() => Play(_clipC);

    private void Play(AudioClip clip)
    {
        if (clip != null)
        {
            Debug.Log($"[SoundView] 효과음 재생 시도: {clip.name}");
            AudioManager.Instance?.PlaySFX(clip);
        }
        else
        {
            Debug.LogWarning("[SoundView] 재생할 효과음 클립이 없습니다!");
        }
    }
}