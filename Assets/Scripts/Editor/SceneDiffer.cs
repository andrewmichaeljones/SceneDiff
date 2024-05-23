using System.Collections.Generic;
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
    private string textBoxContent = "Start by generating a diff...";
    private GUIStyle textBoxStyle;
    
    [MenuItem("Tools/SceneDiff")]
    public static void ShowWindow()
    {
        GetWindow<SceneDifferEditorWindow>("SceneDiff");
    }

    private void OnEnable()
    {
        textBoxStyle = new GUIStyle(EditorStyles.textArea);
        textBoxStyle.font = Font.CreateDynamicFontFromOSFont("Courier New", 16);
        textBoxStyle.richText = true;
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Generate Diff"))
        {
            var path = SceneUtility.GetScenePathByBuildIndex(1);
            
            EditorSceneManager.OpenScene("Assets/Scenes/SceneA.unity");
            
            var sceneAGos = SceneManager.GetActiveScene().GetRootGameObjects();
            var sceneAContents = new List<string>();
            var traverser = new GameObjectHierarchyTraverser();
            foreach (var go in sceneAGos)
            {
                sceneAContents.AddRange(traverser.Traverse(go.transform));
            }
            
            EditorSceneManager.OpenScene("Assets/Scenes/SceneB.unity");
            var sceneBGos = SceneManager.GetActiveScene().GetRootGameObjects();
            var sceneBContents = new List<string>();
            traverser = new GameObjectHierarchyTraverser();
            foreach (var go in sceneBGos)
            {
                sceneBContents.AddRange(traverser.Traverse(go.transform));
            }
            
            _results = DiffUtil.Diff(sceneAContents, sceneBContents);
            
            System.IO.File.WriteAllLines(@"C:\Users\AJ107346\Development\Games\Milan-Land-Template\Logs\sceneA.txt", sceneAContents);
            System.IO.File.WriteAllLines(@"C:\Users\AJ107346\Development\Games\Milan-Land-Template\Logs\sceneB.txt", sceneBContents);
        }
        
        _scrollPosition = GUILayout.BeginScrollView(_scrollPosition, GUILayout.Width(position.width), GUILayout.Height(position.height));
        textBoxContent = GUILayout.TextArea(textBoxContent, textBoxStyle);
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

            textBoxContent = sb.ToString();
        }
    }
}