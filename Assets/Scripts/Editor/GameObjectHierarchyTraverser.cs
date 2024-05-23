using System;
using System.Collections.Generic;
using UnityEngine;

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
                catch (Exception)
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