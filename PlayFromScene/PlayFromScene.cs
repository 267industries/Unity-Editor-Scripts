using System.IO;
using System.Linq;
using System.Text;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Assets.Editor
{
    [ExecuteInEditMode]
    public class PlayFromScene : EditorWindow
    {
        private string _lastScene = string.Empty;
        private int _targetScene = 0;
        private string _waitScene = string.Empty;

        private static string[] _sceneNames;
        private static EditorBuildSettingsScene[] _scenes;

        [MenuItem("Edit/Play from scene %0")]
        public static void Run()
        {
            GetWindow<PlayFromScene>(true, "Play from scene");
        }

        #region Unity Functions

        private void OnEnable()
        {
            _scenes = EditorBuildSettings.scenes;
            _sceneNames = _scenes.Select(s => AsSpacedCamelCase(Path.GetFileNameWithoutExtension(s.path))).ToArray();
        }

        private void Update()
        {
            if (!EditorApplication.isPlaying)
            {
                if (string.IsNullOrEmpty(_waitScene) && !string.IsNullOrEmpty(_lastScene))
                {
                    EditorSceneManager.OpenScene(_lastScene);
                    _lastScene = string.Empty;
                }
            }
        }

        private void OnGUI()
        {
            if (EditorApplication.isPlaying)
            {
                if (SceneManager.GetActiveScene().path == _waitScene) _waitScene = string.Empty;
                GenerateUiDuringPlaying();
                return;
            }

            if (SceneManager.GetActiveScene().path == _waitScene) EditorApplication.isPlaying = true;

            if (_sceneNames == null) return;
            _targetScene = EditorGUILayout.Popup(new GUIContent("Start scene"), _targetScene, _sceneNames,
                GUILayout.Height(20));
            GenerateUiDuringNotPlaying();
        }

        #endregion

        private void GenerateUiDuringPlaying()
        {
            if (string.IsNullOrEmpty(_lastScene))
            {
                GUILayout.Label("Not started by script!");
                return;
            }
            
            GUILayout.Label($"Stop, back to scene: {AsSpacedCamelCase(Path.GetFileNameWithoutExtension(_lastScene))}",
                GUILayout.Height(20));
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Stop", GUILayout.Width(100), GUILayout.Height(25)))
            {
                EditorApplication.isPlaying = false;
            }
            GUILayout.EndHorizontal();
        }

        private void GenerateUiDuringNotPlaying()
        {
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            if (GUILayout.Button("Play", GUILayout.Width(100), GUILayout.Height(25)))
            {
                _lastScene = SceneManager.GetActiveScene().path;
                _waitScene = _scenes[_targetScene].path;
                EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
                EditorSceneManager.OpenScene(_waitScene);
                EditorApplication.isPlaying = true;
            }
            GUILayout.EndHorizontal();
        }

        private string AsSpacedCamelCase(string source)
        {
            var sb = new StringBuilder();
            sb.Append(char.ToUpper(source[0]));
            for (var i = 1; i < source.Length; i++)
            {
                if (char.IsUpper(source[i]) && source[i - 1] != ' ')
                {
                    sb.Append(' ');
                }
                sb.Append(source[i]);
            }
            return sb.ToString();
        }

    }
}