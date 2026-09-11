#if UNITY_EDITOR
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MonoBehaviour), true)]
[CanEditMultipleObjects]
public class InspectorButtonEditor : Editor
{
    private List<MemberInfo> orderedMembers;

    private void OnEnable()
    {
        var mono = target as MonoBehaviour;
        if (mono != null)
        {
            var type = mono.GetType();
            
            var fields = type.GetFields(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Cast<MemberInfo>();
            var methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)
                .Where(m => Attribute.IsDefined(m, typeof(InspectorButtonAttribute)))
                .GroupBy(m => m.GetBaseDefinition())
                .Select(g => g.First())
                .Cast<MemberInfo>();
                
            var allMembers = fields.Concat(methods).ToList();
            
            var monoScript = MonoScript.FromMonoBehaviour(mono);
            string source = monoScript != null ? monoScript.text : "";

            orderedMembers = allMembers.OrderBy(m => GetTypeDepth(m.DeclaringType))
                .ThenBy(m => 
                {
                    if (m.DeclaringType != type || string.IsNullOrEmpty(source)) 
                        return m.MetadataToken;
                    
                    if (m.MemberType == MemberTypes.Method)
                    {
                        var match = Regex.Match(source, @"\b" + m.Name + @"\s*\(");
                        return match.Success ? match.Index : m.MetadataToken;
                    }
                    else
                    {
                        var match = Regex.Match(source, @"\b" + m.Name + @"\s*[=;,]");
                        return match.Success ? match.Index : m.MetadataToken;
                    }
                }).ToList();
        }
    }

    private int GetTypeDepth(Type t)
    {
        int depth = 0;
        while (t != null)
        {
            depth++;
            t = t.BaseType;
        }
        return depth;
    }

    public override void OnInspectorGUI()
    {
        if (orderedMembers == null || orderedMembers.Count == 0)
        {
            base.OnInspectorGUI();
            return;
        }

        serializedObject.Update();

        SerializedProperty iterator = serializedObject.GetIterator();
        bool enterChildren = true;

        if (iterator.NextVisible(enterChildren))
        {
            GUI.enabled = false;
            EditorGUILayout.PropertyField(iterator, true);
            GUI.enabled = true;
            enterChildren = false;
        }

        int memberIndex = 0;

        while (iterator.NextVisible(enterChildren))
        {
            int targetIndex = -1;
            for (int i = memberIndex; i < orderedMembers.Count; i++)
            {
                if (orderedMembers[i].MemberType == MemberTypes.Field && orderedMembers[i].Name == iterator.name)
                {
                    targetIndex = i;
                    break;
                }
            }

            if (targetIndex != -1)
            {
                while (memberIndex < targetIndex)
                {
                    var member = orderedMembers[memberIndex];
                    if (member.MemberType == MemberTypes.Method)
                    {
                        DrawButton((MethodInfo)member);
                    }
                    memberIndex++;
                }
                memberIndex++;
            }

            EditorGUILayout.PropertyField(iterator, true);
        }

        while (memberIndex < orderedMembers.Count)
        {
            var member = orderedMembers[memberIndex];
            if (member.MemberType == MemberTypes.Method)
            {
                DrawButton((MethodInfo)member);
            }
            memberIndex++;
        }

        serializedObject.ApplyModifiedProperties();
    }

    private void DrawButton(MethodInfo methodInfo)
    {
        var attr = methodInfo.GetCustomAttribute<InspectorButtonAttribute>();
        string methodName = attr != null && !string.IsNullOrEmpty(attr.Name) ? attr.Name : methodInfo.Name;

        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button(methodName, GUILayout.ExpandWidth(true)))
        {
            foreach (var targetObj in targets)
            {
                var mono = targetObj as MonoBehaviour;
                if (mono == null) continue;

                var val = methodInfo.Invoke(mono, null);
                if (val is IEnumerator coroutine)
                    mono.StartCoroutine(coroutine);
            }
        }
        EditorGUILayout.EndHorizontal();
    }
}
#endif