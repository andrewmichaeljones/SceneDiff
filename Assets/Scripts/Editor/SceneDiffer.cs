using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using UnityEngine;
using UnityEditor;
using NetDiff;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
using UnityEngine.Windows;

public class SceneDifferEditorWindow : EditorWindow
{
    private string[] _scenePaths;
    private string[] _sceneNames;
    private int _selectedSceneIndex;
    
    private Vector2 scrollPosition;

    private IEnumerable<DiffResult<string>> results;
    private GUIStyle style;
    
    private float _splitPosition = 200f;
    private bool _isResizing = false;
    
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
            
            results = DiffUtil.Diff(sceneAContents, sceneBContents);
            
            System.IO.File.WriteAllLines(@"C:\Users\AJ107346\Development\Games\Milan-Land-Template\Logs\sceneA.txt", sceneAContents);
            System.IO.File.WriteAllLines(@"C:\Users\AJ107346\Development\Games\Milan-Land-Template\Logs\sceneB.txt", sceneBContents);
        }
        
        scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Width(position.width), GUILayout.Height(position.height));
        textBoxContent = GUILayout.TextArea(textBoxContent, textBoxStyle);
        GUILayout.EndScrollView();

        if (results != null)
        {
            var sb = new StringBuilder();
            
            foreach (var result in results)
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

public class GameObjectHierarchyTraverser
{
    public List<string> Traverse(Transform transform)
    {
        var contents = new List<string>();
        TraverseAndPrintNames(transform, 0, contents);
        return contents;
    }

    private void TraverseAndPrintNames(Transform transform, int depth, List<string> contents)
    {
        var indentation = new string('\t', depth);
        
        // Gameobject name
        contents.Add(indentation + transform.gameObject.name);
        var components = transform.GetComponents<Component>();

        foreach (var component in components)
        {
            contents.Add(indentation + component.GetType());

            foreach (var property in component.GetType().GetProperties())
            {
                try
                {
                    // Property name and value
                    var objValue = property.GetValue(component);
                    contents.Add(indentation + $"{property} {objValue}");
                }
                catch (Exception e)
                {
                    // Ignore because this will fail on rigidbody which is deprecated...
                }
            }
        }

        foreach (Transform child in transform)
        {
            TraverseAndPrintNames(child, depth + 1, contents);
        }
    }
}
