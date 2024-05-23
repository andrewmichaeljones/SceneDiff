using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;
using NetDiff;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

public class SceneDifferEditorWindow : EditorWindow
{
    private IEnumerable<DiffResult<string>> _results;
    private Vector2 _scrollPosition;
    private GUIStyle _style;
    private string _textBoxContent = "Start by generating a diff...";
    private GUIStyle _textBoxStyle;
    
    private string[] _scenePaths;
    private string[] _sceneNames;
    private int _selectedSceneIndex1;
    private int _selectedSceneIndex2;
    
    [MenuItem("Tools/SceneDiff")]
    public static void ShowWindow()
    {
        GetWindow<SceneDifferEditorWindow>("SceneDiff");
    }

    private void OnEnable()
    {
        var sceneCount = SceneManager.sceneCountInBuildSettings;
        
        _scenePaths = new string[sceneCount];
        _sceneNames = new string[sceneCount];

        for (int i = 0; i < sceneCount; i++)
        {
            _scenePaths[i] = SceneUtility.GetScenePathByBuildIndex(i);
            _sceneNames[i] = Path.GetFileNameWithoutExtension(_scenePaths[i]);
        }
        
        _textBoxStyle = new GUIStyle(EditorStyles.textArea)
        {
            font = Font.CreateDynamicFontFromOSFont("Courier New", 16),
            richText = true
        };
    }

    private void OnGUI()
    {
        if (SceneManager.sceneCountInBuildSettings < 2)
        {
            GUILayout.Label("Ensure you have two or more scenes added to the Build Settings");
            return;
        }
        
        GUILayout.Space(20);
        GUILayout.Label("Select scenes to compare", EditorStyles.boldLabel);
        _selectedSceneIndex1 = EditorGUILayout.Popup(_selectedSceneIndex1, _sceneNames);
        _selectedSceneIndex2 = EditorGUILayout.Popup(_selectedSceneIndex2, _sceneNames);

        GUILayout.Space(20);
        if (GUILayout.Button("Generate Diff"))
        {
            EditorSceneManager.OpenScene(_scenePaths[_selectedSceneIndex1]);
            
            var sceneAGos = SceneManager.GetActiveScene().GetRootGameObjects();
            var sceneAContents = new List<string>();
            var traverser = new GameObjectHierarchyTraverser();
            foreach (var go in sceneAGos)
            {
                sceneAContents.AddRange(traverser.Traverse(go.transform));
            }
            
            EditorSceneManager.OpenScene(_scenePaths[_selectedSceneIndex2]);
            var sceneBGos = SceneManager.GetActiveScene().GetRootGameObjects();
            var sceneBContents = new List<string>();
            traverser = new GameObjectHierarchyTraverser();
            foreach (var go in sceneBGos)
            {
                sceneBContents.AddRange(traverser.Traverse(go.transform));
            }
            
            _results = DiffUtil.Diff(sceneAContents, sceneBContents);
        }
        
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Width(position.width), GUILayout.Height(position.height));
        _textBoxContent = GUILayout.TextArea(_textBoxContent, _textBoxStyle);
        GUILayout.EndScrollView();

        if (_results != null)
        {
            var sb = new StringBuilder();
            
            foreach (var result in _results)
            {
                var resultString = result.ToFormatString();
                
                if (resultString.StartsWith("+"))
                {
                    resultString = $"<color=lime>{resultString}</color>";
                }
                if (resultString.StartsWith("-"))
                {
                    resultString = $"<color=red>{resultString}</color>";
                }

                sb.AppendLine(resultString);
            }

            _textBoxContent = sb.ToString();
        }
    }
}