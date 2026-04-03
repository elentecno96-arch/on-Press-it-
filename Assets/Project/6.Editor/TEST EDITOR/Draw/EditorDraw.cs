#if UNITY_EDITOR
using Project.Editor.TestEditor.Draw.View;
using Project.Editor.TestEditor.Engine;
using UnityEngine;

namespace Project.Editor.TestEditor.Draw
{
    /// <summary>
    /// 에디터 화면을 그려주는 역활 최상위 클래스
    /// </summary>
    public class EditorDraw
    {
        private readonly EditorEngine _engine;

        private const float SETTINGS_HEIGHtT = 150f;          // 상단 설정바 높이
        private const float NOTE_INSPECTOR_WIDTH = 210f;      // 우측 인스펙터 너비

        private readonly NoteInspectorView _noteView;
        private readonly SettingInspectorView _settingView;
        private readonly TimelineView _timelineView;

        public EditorDraw(EditorEngine engine)
        {
            _engine = engine;

            _noteView = new NoteInspectorView(_engine);
            _settingView = new SettingInspectorView(_engine);
            _timelineView = new TimelineView(_engine);
        }

        public void OnDraw(Rect totalArea)
        {
            float settingsHeight = SETTINGS_HEIGHtT;          // 상단 설정바 높이
            float noteInspectorWidth = NOTE_INSPECTOR_WIDTH;     // 우측 인스펙터 너비
            float mainContentHeight = totalArea.height - settingsHeight;
            float timelineWidth = totalArea.width - noteInspectorWidth;

            Rect settingsRect = new Rect(0, 0, totalArea.width, settingsHeight);
            Rect timelineRect = new Rect(0, settingsHeight, timelineWidth, mainContentHeight);
            Rect noteInspectorRect = new Rect(timelineWidth, settingsHeight, noteInspectorWidth, mainContentHeight);

            _settingView.Draw(settingsRect);
            _timelineView.Draw(timelineRect);
            _noteView.Draw(noteInspectorRect);
        }
    }
}
#endif
