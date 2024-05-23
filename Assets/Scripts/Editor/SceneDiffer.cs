using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;
using NetDiff;
using UnityEngine.SceneManagement;

public class SceneDifferEditorWindow : EditorWindow
{
    private IEnumerable<DiffResult<string>> _results;
    private Vector2 _scrollPosition;
    private GUIStyle _style;
    private string _textBoxContent = "Start by generating a diff...";
    private GUIStyle _textBoxStyle;
    
    private string[] _sceneNames;
    private int _selectedSceneIndex1;
    private int _selectedSceneIndex2;

    private string[] _sceneCapturePaths;

    [MenuItem("Tools/SceneDiff")]
    public static void ShowWindow()
    {
        GetWindow<SceneDifferEditorWindow>("SceneDiff");
    }

    private void OnEnable()
    {
        _textBoxStyle = new GUIStyle(EditorStyles.textArea)
        {
            font = Font.CreateDynamicFontFromOSFont("Courier New", 16),
            richText = true
        };
        
        LoadSceneCaptures();
    }

    private void LoadSceneCaptures()
    {
        var directory = Path.Combine(Application.dataPath, "..", "SceneCaptures");
        if (Directory.Exists(directory))
        {
            _sceneCapturePaths = Directory.GetFiles(directory);
            _sceneNames = new string[_sceneCapturePaths.Length];
            for (int i = 0; i < _sceneCapturePaths.Length; i++)
            {
                _sceneNames[i] = Path.GetFileNameWithoutExtension(_sceneCapturePaths[i]);
            }
        }
        else
        {
            Directory.CreateDirectory(directory);
        }
    }
   
    private void OnGUI()
    {
        if (GUILayout.Button("Snapshot current scene"))
        {
            var sceneGos = SceneManager.GetActiveScene().GetRootGameObjects();
            var sceneContents = new List<string>();
            var traverser = new GameObjectHierarchyTraverser();
            foreach (var go in sceneGos)
            {
                sceneContents.AddRange(traverser.Traverse(go.transform));
            }
        
            var path = Path.Combine(Application.dataPath, "..", "SceneCaptures", $"Scene-{SceneManager.GetActiveScene().name}-{DateTime.Now.ToString("yy-MM-dd-HH-mm-ss")}.snapshot");
            File.WriteAllLines(path, sceneContents);
            LoadSceneCaptures();
        }

        if (_sceneNames == null || _sceneNames.Length < 2)
        {
            GUILayout.Label("Take snapshot of at least two scenes in order to compare");
            return;
        }

        if (_sceneNames != null)
        {
            GUILayout.Space(20);
            GUILayout.Label("Select scenes to compare", EditorStyles.boldLabel);
            _selectedSceneIndex1 = EditorGUILayout.Popup(_selectedSceneIndex1, _sceneNames);
            _selectedSceneIndex2 = EditorGUILayout.Popup(_selectedSceneIndex2, _sceneNames);   
        }

        GUILayout.Space(20);
        if (GUILayout.Button("Generate Diff"))
        {
            var sceneAContent = File.ReadAllLines(_sceneCapturePaths[_selectedSceneIndex1]);
            var sceneBContent = File.ReadAllLines(_sceneCapturePaths[_selectedSceneIndex2]);
            _results = DiffUtil.Diff(sceneAContent, sceneBContent);
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