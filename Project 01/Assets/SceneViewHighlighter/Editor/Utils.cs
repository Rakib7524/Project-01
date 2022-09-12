using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Reflection.Emit;
using UnityEditor;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SceneViewHighlighter
{
    public enum ModifierKey { None = 0, Ctrl = 1, Alt = 2, Shift = 3, Command = 4 }

    public static class Utils
    {
        private static GUIContent s_GUIContent = new GUIContent();

        public static GUIContent TempContent(string text = null, string image = null, Texture tex = null, string tooltip = null)
        {
            s_GUIContent.text = string.IsNullOrEmpty(text) ? string.Empty : text;
            s_GUIContent.image = tex != null ? tex : string.IsNullOrEmpty(image) ? null : EditorGUIUtility.IconContent(image).image;
            s_GUIContent.tooltip = string.IsNullOrEmpty(tooltip) ? string.Empty : tooltip;
            return s_GUIContent;
        }

        public static bool CheckModifierKey(ModifierKey mk, bool none = false)
        {
            switch (mk)
            {
                case ModifierKey.None:
                    return none;
                case ModifierKey.Ctrl:
                    return Event.current.control;
                case ModifierKey.Alt:
                    return Event.current.alt;
                case ModifierKey.Shift:
                    return Event.current.shift;
                case ModifierKey.Command:
                    return Event.current.command;
            }
            return false;
        }

        public static string ToPrefString(this Color c)
        {
            string[] r = new string[4];
            for (int i = 0; i < 4; i++)
                r[i] = Mathf.FloorToInt(Mathf.Clamp01(c[i]) * 255).ToString();
            return string.Join(",", r);
        }

        public static Color ToColor(this string s, Color c = new Color())
        {
            var e = s.Split(',').Select(i => i.ToIntDef(-1));
            e = e.Where(i => i >= 0 && i <= 255);
            var l = e.ToArray();
            if (l.Length == 4)
            {
                for (int i = 0; i < 4; i++)
                    c[i] = l[i] / 255f;
            }
            return c;
        }

        public static int ToIntDef(this string s, int def = 0)
        {
            int r;
            return int.TryParse(s, out r) ? r : def;
        }

        public static IEnumerable<T> Yield<T>(this T item)
        {
            yield return item;
        }

        public static List<Transform> GetParentList(this Transform transform)
        {
            var list = new List<Transform>();
            while (transform != null)
            {
                list.Add(transform);
                transform = transform.parent;
            }
            list.Reverse();
            return list;
        }

        public static IEnumerable<Scene> GetAllLoadedScene()
        {
            for (int i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (scene.isLoaded && scene.IsValid())
                    yield return scene;
            }
        }

        public static IEnumerable<GameObject> GetParentList(this GameObject go)
        {
            foreach (var trans in go.transform.GetParentList())
                yield return trans.gameObject;
        }

        public static IEnumerable<GameObject> GetChildrenList(this GameObject go, bool self = true)
        {
            if (self) yield return go;
            var stack = new Stack<IEnumerator>();
            stack.Push(go.transform.GetEnumerator());
            while (stack.Count > 0)
            {
                var enumerator = stack.Peek();
                if (enumerator.MoveNext())
                {
                    var t = enumerator.Current as Transform;
                    yield return t.gameObject;
                    stack.Push(t.GetEnumerator());
                }
                else
                {
                    stack.Pop();
                }
            }
        }
    }

    public class TransformComparer : IComparer<Transform>
    {
        public int Compare(Transform x, Transform y)
        {
            var xPrefab = EditorUtility.IsPersistent(x);
            var yPrefab = EditorUtility.IsPersistent(y);
            if (xPrefab && yPrefab) return 0;
            if (xPrefab) return 1;
            if (yPrefab) return -1;

            var xParents = x.GetParentList();
            var yParents = y.GetParentList();
            int i = 0;
            while (i < xParents.Count && i < yParents.Count && xParents[i] == yParents[i])
                i++;
            if (i >= xParents.Count || i >= yParents.Count)
                return xParents.Count.CompareTo(yParents.Count);
            return xParents[i].GetSiblingIndex().CompareTo(yParents[i].GetSiblingIndex());
        }
    }

    public static class ReflectionHelper
    {
        public static T MakeFunc<T>(this MethodInfo method) where T : class
        {
            return Delegate.CreateDelegate(typeof(T), method) as T;
        }

        public static T MakeStaticFunc<T>(this MethodInfo method) where T : class
        {
            return Delegate.CreateDelegate(typeof(T), null, method) as T;
        }

        public static T MakeFuncGenericThis<T>(this MethodInfo method) where T : class
        {
            var obj = Expression.Parameter(typeof(object), "obj");
            var item = Expression.Convert(obj, method.DeclaringType);
            var call = Expression.Call(item, method);
            var lambda = Expression.Lambda<T>(call, obj);
            return lambda.Compile();
        }

        public static Func<object, object> Getter(this FieldInfo field)
        {
            var name = field.ReflectedType.FullName + ".get_" + field.Name;
            var method = new DynamicMethod(name, typeof(object), new[] { typeof(object) }, field.Module, true);
            var il = method.GetILGenerator();
            if (field.IsStatic)
            {
                il.Emit(OpCodes.Ldsfld, field);
                il.Emit(field.FieldType.IsClass ? OpCodes.Castclass : OpCodes.Box, field.FieldType);
            }
            else
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Castclass, field.DeclaringType);
                il.Emit(OpCodes.Ldfld, field);
                il.Emit(field.FieldType.IsClass ? OpCodes.Castclass : OpCodes.Box, field.FieldType);
            }
            il.Emit(OpCodes.Ret);
            return (Func<object, object>)method.CreateDelegate(typeof(Func<object, object>));
        }

        public static Action<object, object> Setter(this FieldInfo field)
        {
            var name = field.ReflectedType.FullName + ".set_" + field.Name;
            var method = new DynamicMethod(name, null, new[] { typeof(object), typeof(object) }, field.Module, true);
            var il = method.GetILGenerator();
            if (field.IsStatic)
            {
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(field.FieldType.IsClass ? OpCodes.Castclass : OpCodes.Unbox_Any, field.FieldType);
                il.Emit(OpCodes.Stsfld, field);
            }
            else
            {
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Castclass, field.DeclaringType);
                il.Emit(OpCodes.Ldarg_1);
                il.Emit(field.FieldType.IsClass ? OpCodes.Castclass : OpCodes.Unbox_Any, field.FieldType);
                il.Emit(OpCodes.Stfld, field);
            }
            il.Emit(OpCodes.Ret);
            return (Action<object, object>)method.CreateDelegate(typeof(Action<object, object>));
        }
    }

    public class ToggleableGroup : GUI.Scope
    {
        public ToggleableGroup(GUIContent content, ref bool value)
        {
            EditorGUILayout.BeginVertical("box", GUILayout.ExpandWidth(true));
            var style = new GUIStyle(EditorStyles.label);
            style.fontStyle = FontStyle.Bold;
            value = EditorGUILayout.ToggleLeft(content, value, style);
            //if (value)
            {
                var rect = GUILayoutUtility.GetLastRect();
                rect.yMin = rect.yMax;
                rect.height = 1;
                var start = new Vector2(rect.x, rect.y);
                var end = new Vector2(rect.xMax, rect.y);
                Handles.color = Color.black;
                Handles.DrawLine(start, end);
            }
            EditorGUI.indentLevel++;
            EditorGUI.BeginDisabledGroup(!value);
        }

        protected override void CloseScope()
        {
            EditorGUI.EndDisabledGroup();
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
        }
    }

    public class GUIIconSize : GUI.Scope
    {
        private Vector2 m_Size;

        public GUIIconSize(int w, int h)
        {
            m_Size = EditorGUIUtility.GetIconSize();
            EditorGUIUtility.SetIconSize(new Vector2(w, h));
        }

        public GUIIconSize(Vector2 size)
        {
            m_Size = EditorGUIUtility.GetIconSize();
            EditorGUIUtility.SetIconSize(size);
        }

        protected override void CloseScope()
        {
            EditorGUIUtility.SetIconSize(m_Size);
        }
    }

    public class GUIColorTint : GUI.Scope
    {
        private Color m_Color;

        public GUIColorTint(Color color)
        {
            m_Color = GUI.color;
            GUI.color = color;
        }

        public GUIColorTint(float alpha)
        {
            m_Color = GUI.color;
            var col = m_Color;
            col.a = alpha;
            GUI.color = col;
        }

        public GUIColorTint(Color col, float alpha)
        {
            m_Color = GUI.color;
            col.a = alpha;
            GUI.color = col;
        }

        protected override void CloseScope()
        {
            GUI.color = m_Color;
        }
    }

    public class HandlesColorTint : GUI.Scope
    {
        private Color m_Color;

        public HandlesColorTint(Color color)
        {
            m_Color = Handles.color;
            Handles.color = color;
        }

        public HandlesColorTint(Color col, float alpha)
        {
            m_Color = Handles.color;
            col.a = alpha;
            Handles.color = col;
        }

        protected override void CloseScope()
        {
            Handles.color = m_Color;
        }
    }
}