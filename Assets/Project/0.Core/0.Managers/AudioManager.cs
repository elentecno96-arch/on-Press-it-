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

        [Header("Audio Sources")]
        [SerializeField] private AudioSource musicSource;
        [SerializeField] private AudioSource sfxSource;

        public AudioMixerGroup BGMGroup => bgmGroup;
        public AudioMixerGroup  SFXGroup => sfxGroup;
                
        private float _bgmVolume = 0.5f; // 기본값
        private float _sfxVolume = 0.5f;
        public float BgmVolume => _bgmVolume;
        public float SfxVolume => _sfxVolume;

        public event Action<float, float> OnRequestAudioSave;

        protected override void Awake()
        {
            base.Awake();
            // 씬이 바뀌어도 파괴되지 않게 설정 (선택 사항이지만 추천)
            DontDestroyOnLoad(gameObject);
        }

        public override async UniTask Initialize()
        {
            await UniTask.Yield();
            // PlayerManager 체크 로직 추가 (안전성)
            if (PlayerManager.Instance != null && PlayerManager.Instance.Data != null)
            {
                AudioSetting(PlayerManager.Instance.Data);
            }

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
            if (parameterName == "BGM") _bgmVolume = volume;
            else if (parameterName == "SFX") _sfxVolume = volume;

            float dB = volume > 0 ? Mathf.Log10(volume) * 20 : -80f;
            mainMixer.SetFloat(parameterName, dB);
        }


        public void PlayBGM(AudioClip clip, bool loop = true)
        {
            if (clip == null) return;

            // 1. 이미 같은 음악이 재생 중이라면 중복 재생 방지
            if (musicSource.clip == clip && musicSource.isPlaying) return;

            // 2. 새로운 음악을 재생하기 전에 기존 소리를 정지
            musicSource.Stop();

            musicSource.clip = clip;
            musicSource.loop = loop;
            musicSource.Play();

            Debug.Log($"[BGM 재생] {clip.name}");
        }
        public void StopBGM()
        {
            if (musicSource != null) musicSource.Stop();
        }

        /// [중요] 효과음 재생 (MainUiPresenter에서 호출하는 함수)
        /// </summary>
        public void PlaySFX(AudioClip clip, float volumeScale = 0.1f)
        {
            if (clip == null || sfxSource == null) return;
            // PlayOneShot을 써야 여러 효과음이 겹쳐서 들립니다.
            sfxSource.PlayOneShot(clip, volumeScale);
        }

        // =========================================================================
        // 아래는 기존 로직을 건드리지 않고 추가된 저장 관련 기능입니다.
        // =========================================================================

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