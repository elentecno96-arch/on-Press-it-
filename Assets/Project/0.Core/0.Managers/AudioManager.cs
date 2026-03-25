using Cysharp.Threading.Tasks;
using Project.Core.Utilities;
using UnityEngine;
using UnityEngine.Audio;
using System;
namespace Project.Core.Managers
{
    /// <summary>
    /// 전역 오디오 담당 매지저
    /// </summary>
    public class AudioManager : BaseSingleton<AudioManager>
    {
        [Header("Audio Mixer")]
        [SerializeField] private AudioMixer mainMixer;
        [SerializeField] private AudioMixerGroup bgmGroup;
        [SerializeField] private AudioMixerGroup sfxGroup;

        [Header("BGM Source")]
        [SerializeField] private AudioSource musicSource;
              
        public AudioMixerGroup BGMGroup => bgmGroup;
        public AudioMixerGroup  SFXGroup => sfxGroup;
                
        private float _bgmVolume = 0.5f; // 기본값
        private float _sfxVolume = 0.5f;
        public float BgmVolume => _bgmVolume;
        public float SfxVolume => _sfxVolume;

        public event Action<float, float> OnRequestAudioSave;

        public void AudioSaveSettings()
        {
            // 데이터 보호를 위해 현재 상태를 로그로 남기고 이벤트를 발생시킵니다.
            if (OnRequestAudioSave != null)
            {
                OnRequestAudioSave.Invoke(_bgmVolume, _sfxVolume);
                Debug.Log($"[AudioManager] 저장 신호 발송 완료: BGM({_bgmVolume}), SFX({_sfxVolume})");
            }
            else
            {
                Debug.LogWarning("[AudioManager] 저장 이벤트를 구독하는 매니저가 없습니다.");
            }
        }
        public override async UniTask Initialize()
        {
            await UniTask.Yield();

            Debug.Log("AudioManager: 믹서 연결 및 초기화 완료");
        }

        public void AssignMixerGroup(AudioSource source, bool isBGM = false)
        {
            source.outputAudioMixerGroup = isBGM ? bgmGroup : sfxGroup;
        }

        /// <summary>
        /// 믹서 볼륨 설정 (volume은 0~1 사이 값)
        /// </summary>
        public void SetVolume(string parameterName, float volume)
        {           
            if (parameterName == "BGM") _bgmVolume = volume;
            else if (parameterName == "SFX") _sfxVolume = volume;

            float dB = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
            mainMixer.SetFloat(parameterName, dB);
        }


        public void PlayBGM(AudioClip clip, bool loop = true)
        {
            if (musicSource.clip == clip) return;
            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.Play();
        }
    }
}

