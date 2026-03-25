using Cysharp.Threading.Tasks;
using Project.Core.Utilities;
using UnityEngine;
using UnityEngine.Audio;
using System; // Action 사용을 위해 추가되었습니다.

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
        public AudioMixerGroup SFXGroup => sfxGroup;

        public override async UniTask Initialize()
        {
            await UniTask.Yield();
            var playerdata = PlayerManager.Instance.Data;
            AudioSetting(playerdata);
            Debug.Log("AudioManager: 믹서 연결 및 초기화 완료");
        }
        public void AudioSetting(PlayerData playerData)
        {
            SetVolume("BGM",playerData.bgmVolume);
            SetVolume("SFX", playerData.sfxVolume);
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
            float dB = Mathf.Log10(volume) * 20;
            mainMixer.SetFloat(parameterName, dB);
        }
        public void PlayBGM(AudioClip clip, bool loop = true)
        {
            if (musicSource.clip == clip) return;
            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.Play();
        }
        // =========================================================================
        // 아래는 기존 로직을 건드리지 않고 추가된 저장 관련 기능입니다.
        // =========================================================================
        public event Action<float, float> OnRequestAudioSave;

        private float _bgmVolume = 1.0f;
        private float _sfxVolume = 1.0f;

        /// <summary>
        /// PlayerManager 등 구독자에게 현재 볼륨 데이터를 발송합니다.
        /// </summary>
        public void AudioSaveSettings()
        {
            // mainMixer에서 현재 값을 가져와 업데이트 (기존 SetVolume 로직을 수정하지 않기 위함)
            mainMixer.GetFloat("BGM", out float bgmDB);
            mainMixer.GetFloat("SFX", out float sfxDB);

            // dB를 다시 0~1 값으로 역산하여 저장용 변수에 할당
            _bgmVolume = Mathf.Pow(10, bgmDB / 20f);
            _sfxVolume = Mathf.Pow(10, sfxDB / 20f);

            OnRequestAudioSave?.Invoke(_bgmVolume, _sfxVolume);
            Debug.Log($"[AudioManager] 저장 신호 발송: BGM({_bgmVolume}), SFX({_sfxVolume})");
        }
    }
}