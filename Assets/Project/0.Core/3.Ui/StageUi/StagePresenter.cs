using Project.Core.Ui.StageUi.View;
using Project.Rhythm.Data;
using Project.Rhythm.Data.Enum;
using Project.Rhythm.Interface;
using Project.Rhythm.Judgement;
using Project.Rhythm.Visual;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Rhythm.Presentation
{
    public class StagePresenter : MonoBehaviour
    {
        [SerializeField] private StageView stageView;
        [SerializeField] private GameView inGameView;
        [SerializeField] private PlayUiView playUiView;
        [SerializeField] private ResultView resultView;
        [SerializeField] private GuideView guideView;

        private StageData _stageData;
        private BaseRhythmVisual _currentEnvironment;
        private JudgementSystem _judgementSystem;

        private bool _isGuideShowing;
        private float _guideEndBeat;

        private readonly Dictionary<StageThemeType, GameObject> _backgroundMap = new();
        private readonly Dictionary<StageThemeType, GameObject> _playerMap = new();
        private readonly Dictionary<StageThemeType, GameObject> _notePrefabMap = new();

        private readonly Dictionary<int, Note.Note> _fixedNoteMap = new();

        private ITouchVisual _currentTouchVisual;
        private StageThemeType _currentTheme = (StageThemeType)(-1);

        public void Initialize(StageData data)
        {
            _stageData = data;
            if (stageView == null) return;

            stageView.Clear();

            InitializeThemes();
            InitializeUI();

            StageGuide();

            if (_stageData.themeResources != null && _stageData.themeResources.Count > 0)
            {
                var firstTheme = _stageData.themeResources[0].theme;

                Debug.Log($"[초기 테마 설정] {firstTheme}");

                _currentTheme = (StageThemeType)(-999);
                ChangeTheme(firstTheme);
            }
            else
            {
                Debug.LogError("[Presenter] themeResources 비어있음");
            }
        }

        /// <summary>
        /// 스테이지 데이터의 설정을 확인하여 가이드를 실행합니다.
        /// </summary>
        /// <remarks>
        /// 패턴 타입에 따라 가이드 내용을 변경 하며, 스테이지 인덱스가 0이거나, 
        /// 스테이지 데이터의 skipGuide 플래그가 true인 경우 가이드 표시를 건너뜁니다
        /// </remarks>
        private void StageGuide()
        {
            if (guideView == null || _stageData == null) return;

            _isGuideShowing = false;

            if (_stageData.skipGuide || _stageData.stageIndex <= 0)
            {
                guideView.Hide();
                return;
            }

            if (_stageData.actions != null && _stageData.actions.Count > 0)
            {
                PatternType mainType = _stageData.actions[0].type;
                guideView.Show(mainType);

                _isGuideShowing = true;
                _guideEndBeat = 8f;
            }
        }

        private void InitializeThemes()
        {
            _backgroundMap.Clear();
            _playerMap.Clear();
            _notePrefabMap.Clear();

            foreach (var res in _stageData.themeResources)
            {
                var bg = stageView.CreateBackground(res.backgroundPrefab);
                bg.SetActive(false);
                _backgroundMap.Add(res.theme, bg);

                var player = stageView.CreatePlayer(res.playerPrefab);
                player.SetActive(false);
                _playerMap.Add(res.theme, player);

                if (res.notePrefab == null)
                {
                    Debug.LogError($"[Presenter] NotePrefab NULL: {res.theme}");
                }
                _notePrefabMap[res.theme] = res.notePrefab;
            }
        }


        public void ChangeTheme(StageThemeType theme)
        {
            Debug.Log($"[ChangeTheme 호출됨] {theme}");

            if (_currentTheme == theme) return;

            // 이전 비활성화
            if (_backgroundMap.TryGetValue(_currentTheme, out var prevBg))
                prevBg.SetActive(false);

            if (_playerMap.TryGetValue(_currentTheme, out var prevPlayer))
                prevPlayer.SetActive(false);

            // 새 테마 활성화
            if (_backgroundMap.TryGetValue(theme, out var newBg))
            {
                newBg.SetActive(true);

                _currentEnvironment = newBg.GetComponentInChildren<BaseRhythmVisual>();
                if (_currentEnvironment != null)
                {
                    _currentEnvironment.SetBpm(_stageData.bpm);
                }
            }

            if (_playerMap.TryGetValue(theme, out var newPlayer))
            {
                newPlayer.SetActive(true);
                _currentTouchVisual = newPlayer.GetComponentInChildren<ITouchVisual>();
            }

            _currentTheme = theme;
        }

        public Note.Note GetOrSpawnPersistent(string id)
        {
            if (string.IsNullOrEmpty(id)) return null;
            int key = id.GetHashCode();

            if (_fixedNoteMap.TryGetValue(key, out var existingNote))
            {
                if (existingNote != null)
                {
                    if (!existingNote.gameObject.activeSelf) existingNote.gameObject.SetActive(true);
                    return existingNote;
                }
                _fixedNoteMap.Remove(key);
            }

            GameObject prefab = GetCurrentNotePrefab();
            if (prefab == null) return null;

            GameObject obj = stageView.CreateNote(prefab);
            if (obj != null && obj.TryGetComponent<Note.Note>(out var note))
            {
                _fixedNoteMap.Add(key, note);
                Debug.Log($"[Presenter] Persistent Note '{id}' 최초 스폰 완료");
                return note;
            }

            return null;
        }

        private void InitializeUI()
        {
            if (inGameView != null)
            {
                inGameView.gameObject.SetActive(true);
                inGameView.SetStageName(_stageData.stageName);
                inGameView.UpdateProgress(0f);
            }

            if (resultView != null)
            {
                resultView.gameObject.SetActive(false);
            }
        }

        public void UpdateUI(float currentTime)
        {
            if (inGameView == null || _stageData == null) return;

            float progress = Mathf.Clamp01(currentTime / _stageData.endPosition);
            inGameView.UpdateProgress(progress);

            if (_isGuideShowing)
            {
                float currentBeat = (currentTime * _stageData.bpm) / 60f;
                if (currentBeat >= _guideEndBeat)
                {
                    _isGuideShowing = false;
                    guideView.Hide();
                }
            }
        }

        public void ShowResult(int p, int gr, int go, int m)
        {
            if (inGameView != null) inGameView.gameObject.SetActive(false);

            if (resultView != null)
            {
                resultView.DisplayResult(p, gr, go, m);
            }
        }

        public void StartCountdown(float targetBeat)
        {
            if (_currentEnvironment != null)
            {
                _currentEnvironment.StartCountdown(targetBeat);
            }
            else
            {
                Debug.LogWarning("[Presenter] 현재 활성화된 Environment 비주얼이 없습니다.");
            }
        }

        public GameObject GetCurrentNotePrefab()
        {
            if (_currentTheme == (StageThemeType)(-1))
            {
                Debug.LogError("[Presenter] 현재 테마 설정 안됨");
                return null;
            }

            if (_notePrefabMap.TryGetValue(_currentTheme, out var prefab))
                return prefab;

            Debug.LogError($"[Presenter] NotePrefab 없음: {_currentTheme}");
            return null;
        }

        public GameObject SpawnNote()
        {
            var prefab = GetCurrentNotePrefab();
            if (prefab == null) return null;

            var obj = stageView.CreateNote(prefab);

            if (obj != null)
                obj.transform.SetAsFirstSibling();
            return obj;
        }

        public Note.Note GetFixedNote(string id) =>
            _fixedNoteMap.TryGetValue(id.GetHashCode(), out var note) ? note : null;

        public ITouchVisual GetTouchVisual() => _currentTouchVisual;

        public GameObject SpawnNote(GameObject notePrefab)
        {
            var obj = stageView.CreateNote(notePrefab);

            if (obj != null)
                obj.transform.SetAsFirstSibling();

            return obj;
        }

        public void SetJudgementSystem(JudgementSystem system)
        {
            _judgementSystem = system;
            _judgementSystem.OnJudged += HandleOnJudged;
        }

        private void HandleOnJudged(JudgeResult result, Note.Note note)
        {
            if (playUiView == null) return;

            if (_judgementSystem != null)
            {
                float currentScore = _judgementSystem.CalculateFinalScore();
                playUiView.UpdateScore(currentScore);
            }

            if (!_judgementSystem.IsHolding)
            {
                playUiView.ShowJudgement(result);
            }
        }

        public void ShowJudgeEffect(JudgeResult result)
        {
            if (playUiView != null)
            {
                playUiView.ShowJudgement(result);

                if (_judgementSystem != null)
                    playUiView.UpdateScore(_judgementSystem.CalculateFinalScore());
            }
        }
        private void OnDestroy()
        {
            if (_judgementSystem != null)
                _judgementSystem.OnJudged -= HandleOnJudged;
        }
    }
}