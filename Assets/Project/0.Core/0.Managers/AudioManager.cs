using Cysharp.Threading.Tasks;
<<<<<<< Updated upstream
=======
using Project.Core.Utilities;
>>>>>>> Stashed changes
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
namespace Project.Core.Managers
<<<<<<< Updated upstream
{

    public class AudioManager : MonoBehaviour
{
=======
{    public class AudioManager : BaseSingleton<AudioManager>
    {
>>>>>>> Stashed changes
        [Header("Audio Mixer")]
        [SerializeField] private AudioMixer mainMixer;

        [Header("BGM Source")]
        [SerializeField] private AudioSource musicSource;

<<<<<<< Updated upstream
        public async UniTask Initialize()
=======
        public override async UniTask Initialize()
>>>>>>> Stashed changes
        {
            // 설정 저장소(PlayerPrefs 등)에서 기존 볼륨 값을 가져와 세팅할 수 있습니다.
            await UniTask.Yield();
            Debug.Log("AudioManager: 믹서 연결 및 초기화 완료");
        }

        /// <summary>
        /// 믹서 볼륨 설정 (volume은 0~1 사이 값)
        /// </summary>
        public void SetVolume(string parameterName, float volume)
        {
            // 오디오 믹서는 데시벨(dB) 단위를 사용하므로 로그 계산이 필요합니다.
            // 0은 -80dB, 1은 0dB가 되도록 매핑합니다.
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
<<<<<<< Updated upstream
}
=======
}
>>>>>>> Stashed changes
