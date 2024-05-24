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
        var indentation = new string(' ', depth * 4);
        
        contents.Add(indentation + transform.gameObject.name);
        var components = transform.GetComponents<Component>();

        foreach (var component in components)
        {
            contents.Add(indentation + component.GetType());
            
            foreach (var property in component.GetType().GetProperties())
            {
                try
                {
                    // TODO: This calls the getter on the property which can have some odd side effects
                    // especially if the getter is doing more than just returning the value!
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