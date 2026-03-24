using Project.Core.Ui.StageUi.View;
using Project.Rhythm.Data;
using Project.Rhythm.Interface;
using Project.Rhythm.Judgement;
using System.Collections.Generic;
using UnityEngine;

namespace Project.Rhythm.Presentation
{
    public class StagePresenter : MonoBehaviour
    {
        [SerializeField] private StageView stageView;
        [SerializeField] private GameView inGameView;
        [SerializeField] private ResultView resultView;

        private StageData _stageData;

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
            if (_currentTheme == theme) return;

            // [수정] 이전 테마 자원 비활성화
            if (_backgroundMap.TryGetValue(_currentTheme, out var prevBg))
                prevBg.SetActive(false);

            if (_playerMap.TryGetValue(_currentTheme, out var prevPlayer))
                prevPlayer.SetActive(false);

            if (_backgroundMap.TryGetValue(theme, out var newBg))
            {
                newBg.SetActive(true);
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
            if (_backgroundMap.TryGetValue(_currentTheme, out var bg))
            {
                var stage3 = bg.GetComponentInChildren<Project.Data.Stage.STAGE3.Stage3Environment>();

                if (stage3 != null)
                {
                    stage3.SetBpm(_stageData.bpm);
                    stage3.StartCountdown(targetBeat);
                }
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

        public void ShowJudgeEffect(JudgeResult result)
        {
            Debug.Log($"[Visual Effect] {result}");
        }
    }
}