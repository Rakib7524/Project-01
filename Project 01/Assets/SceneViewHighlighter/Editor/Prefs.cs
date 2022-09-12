using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditorInternal;
using UnityEngine;

namespace SceneViewHighlighter
{
    [AttributeUsage(AttributeTargets.Field)]
    public class PrefsKeyAttribute : Attribute
    {
        public bool Enabled;
        public int Integer;
        public string String;
        public string Color;

        public string Group;
        public bool Indent = false;
        public Type EnumType = null;
        public bool Slider = false;
        public int Min = 0;
        public int Max = 1;
        public int TextLine = 1;
    }

    [AttributeUsage(AttributeTargets.Field)]
    public class PrefsDescriptionAttribute : Attribute
    {
        public string Text;
        public string Tooltip;
        public string Helpbox;
    }

    public class PrefsItem
    {
        public FieldInfo Field;
        public PrefsKeyAttribute Key;
        public PrefsDescriptionAttribute Desc;
    }

    [InitializeOnLoad]
    public static class Prefs
    {
        public static bool SceneViewHighlighter
        {
            get { return _SceneViewHighlighter; }
            set { _SceneViewHighlighter = value; EditorPrefs.SetBool("SceneViewHighlighter.sceneViewHighlighter", value); SaveAllPrefs(); }
        }

        [PrefsKey(Enabled = true)]
        private static bool _SceneViewHighlighter;

        [PrefsKey(Group = "SceneViewHighlighter", Enabled = true)]
        [PrefsDescription(Text = "Hover in Hierarchy", Tooltip = "")]
        public static bool svOutlineHierarchy;
        [PrefsKey(Group = "SceneViewHighlighter", Integer = (int)ModifierKey.None, Indent = true, EnumType = typeof(ModifierKey))]
        [PrefsDescription(Text = "Modifier", Tooltip = "")]
        public static int svOutlineHierarchyModifier;
        [PrefsKey(Group = "SceneViewHighlighter", Enabled = true)]
        [PrefsDescription(Text = "Hover in SceneView", Tooltip = "")]
        public static bool svOutlineSceneView;
        [PrefsKey(Group = "SceneViewHighlighter", Integer = (int)ModifierKey.Ctrl, Indent = true, EnumType = typeof(ModifierKey))]
        [PrefsDescription(Text = "Modifier", Tooltip = "")]
        public static int svOutlineSceneViewModifier;
        [PrefsKey(Group = "SceneViewHighlighter", Enabled = true, Indent = true)]
        [PrefsDescription(Text = "Show Name (Non-Renderer)", Tooltip = "")]
        public static bool svOutlineSceneViewName;
        [PrefsKey(Group = "SceneViewHighlighter", Enabled = true, Indent = true)]
        [PrefsDescription(Text = "Line to Pivot (Non-Renderer)", Tooltip = "")]
        public static bool svOutlineSceneViewLine;
        [PrefsKey(Group = "SceneViewHighlighter", Enabled = true)]
        [PrefsDescription(Text = "Outline Renderer", Tooltip = "Outline the renderer in Scene View when mouse hover the item.")]
        public static bool svOutline;
        [PrefsKey(Group = "SceneViewHighlighter", Color = "0,0,255,50", Indent = true)]
        [PrefsDescription(Text = "Color", Tooltip = "Outline Color")]
        public static Color svOutlineColor;
        [PrefsKey(Group = "SceneViewHighlighter", Integer = 20, Indent = true, Min = 0)]
        [PrefsDescription(Text = "Children Level", Tooltip = "Also outline children of the game object.")]
        public static int svOutlineLevel;
        [PrefsKey(Group = "SceneViewHighlighter", Integer = 500, Indent = true, Min = 1)]
        [PrefsDescription(Text = "Max Renderer", Tooltip = "Max renderer to outline.")]
        public static int svOutlineMax;
        [PrefsKey(Group = "SceneViewHighlighter", Enabled = true)]
        [PrefsDescription(Text = "Outline Selected Sprites", Tooltip = "")]
        public static bool svOutlineSelectedSprites;
        [PrefsKey(Group = "SceneViewHighlighter", Color = "255,102,0,12", Indent = true)]
        [PrefsDescription(Text = "Color", Tooltip = "Selected Sprites Outline Color")]
        public static Color svOutlineSelectedSpritesColor;
        [PrefsKey(Group = "SceneViewHighlighter", Enabled = false)]
        [PrefsDescription(Text = "Outline Buffer Half Size", Tooltip = "Increase speed but decrease quality")]
        public static bool svOutlineHalfSizeBuffer;
        [PrefsKey(Group = "SceneViewHighlighter", Enabled = true)]
        [PrefsDescription(Text = "Indicator", Tooltip = "Draw indicator in Scene View when mouse hover the item.")]
        public static bool svIndicator;
        [PrefsKey(Group = "SceneViewHighlighter", Color = "0,0,255,255", Indent = true)]
        [PrefsDescription(Text = "Color", Tooltip = "Indicator Color")]
        public static Color svIndicatorColor;
        [PrefsKey(Group = "SceneViewHighlighter", Enabled = true)]
        [PrefsDescription(Text = "RectTransform Overlay", Tooltip = "Draw Overlay in Scene View when mouse hover the RectTransform.")]
        public static bool svRectTransform;
        [PrefsKey(Group = "SceneViewHighlighter", Color = "0,0,255,50", Indent = true)]
        [PrefsDescription(Text = "Color", Tooltip = "RectTransform Overlay Color")]
        public static Color svRectTransformOverlayColor;
        [PrefsKey(Group = "SceneViewHighlighter", Enabled = false)]
        [PrefsDescription(Text = "Hide Warning", Tooltip = "")]
        public static bool svHideWarning;

        public static readonly GUIContent sceneViewHighlighterContent = new GUIContent("Scene View Highlighter", "Enable/disable scene view highlighter.");

        private static Vector2 s_Scroll;
        private static List<PrefsItem> s_FieldList;

        static Prefs()
        {
            var fields = typeof(Prefs).GetFields(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).AsEnumerable();
            fields = fields.Where(i => Attribute.GetCustomAttribute(i, typeof(PrefsKeyAttribute)) != null);
            s_FieldList = fields.Select(field =>
            {
                var item = new PrefsItem();
                item.Field = field;
                item.Key = Attribute.GetCustomAttribute(field, typeof(PrefsKeyAttribute)) as PrefsKeyAttribute;
                item.Desc = Attribute.GetCustomAttribute(field, typeof(PrefsDescriptionAttribute)) as PrefsDescriptionAttribute;
                return item;
            }).ToList();
            Update();
        }

        private static void Update()
        {
            LoadAllPrefs();
        }

        public static PrefsItem GetPrefsItem(string name)
        {
            return s_FieldList.Where(i => i.Field.Name.Equals(name)).FirstOrDefault();
        }

        [PreferenceItem("Highlighter")]
        public static void OnPreferencesGUI()
        {
            EditorGUI.BeginChangeCheck();
            s_Scroll = EditorGUILayout.BeginScrollView(s_Scroll, false, false);

            if (!svHideWarning)
                EditorGUILayout.HelpBox("Outline huge amount of renderer may result lag in editor. " +
                        "Please select a proper value."
                        , MessageType.Info, true);

            using (new ToggleableGroup(sceneViewHighlighterContent, ref _SceneViewHighlighter))
                DrawPrefsGUI("SceneViewHighlighter", GetLayoutRect, true);


            if (EditorGUI.EndChangeCheck())
            {
                SaveAllPrefs();
                //EditorApplication.RepaintHierarchyWindow();
                InternalEditorUtility.RepaintAllViews();
            }

            EditorGUILayout.EndScrollView();
        }

        private static Rect GetLayoutRect(float height)
        {
            var rect = new Rect(EditorGUILayout.GetControlRect(true, height));
            return rect;
        }

        private static void DrawPrefsGUI(string group, Func<float, Rect> getRect, bool shading = false)
        {
            bool even = false;
            var indent = EditorGUI.indentLevel;
            var ro = new RectOffset(1, 1, 1, 1);
            foreach (var item in s_FieldList)
            {
                if (item.Key == null || item.Desc == null) continue;
                if (!item.Key.Group.Equals(group)) continue;

                if (item.Key.Indent) EditorGUI.indentLevel++;
                var rect = getRect(EditorGUIUtility.singleLineHeight);
                var content = Utils.TempContent(item.Desc.Text, tooltip: item.Desc.Tooltip);

                if (EditorGUI.indentLevel == indent) even = !even;
                if (even && shading)
                    if (Event.current.type == EventType.Repaint)
                    {
                        var color = EditorGUIUtility.isProSkin? new Color32(0, 0, 0, 25): new Color32(255, 255, 255, 50); ;
                        EditorGUI.DrawRect(ro.Add(rect), color);
                        EditorGUI.DrawRect(ro.Add(rect), color);
                    }

                if (item.Field.FieldType == typeof(bool))
                {
                    var v = EditorGUI.Toggle(rect, content, (bool)item.Field.GetValue(null));
                    item.Field.SetValue(null, v);
                }
                if (item.Field.FieldType == typeof(int))
                {
                    if (item.Key.Slider == true)
                    {
                        int max = item.Key.Max;
                        var v = EditorGUI.IntSlider(rect, content, (int)item.Field.GetValue(null), item.Key.Min, max);
                        item.Field.SetValue(null, v);
                    }
                    else if (item.Key.EnumType == null)
                    {
                        var v = EditorGUI.IntField(rect, content, (int)item.Field.GetValue(null));
                        if (v < item.Key.Min) v = item.Key.Min;
                        item.Field.SetValue(null, v);
                    }
                    else
                    {
                        int value = (int)item.Field.GetValue(null);
                        if (item.Field.Name.Equals("svOutlineSceneViewModifier"))
                            if ((ModifierKey)value == ModifierKey.None)
                                value = (int)ModifierKey.Ctrl;
                        var v = Enum.ToObject(item.Key.EnumType, value);
                        v = EditorGUI.EnumPopup(rect, content, (Enum)v);
                        if (item.Field.Name.Equals("svOutlineSceneViewModifier"))
                            if ((ModifierKey)v == ModifierKey.None)
                                v = value;
                        item.Field.SetValue(null, (int)v);
                    }
                }
                if (item.Field.FieldType == typeof(string))
                {
                    if (item.Key.TextLine <= 1)
                    {
                        var v = EditorGUI.TextField(rect, content, (string)item.Field.GetValue(null));
                        item.Field.SetValue(null, v);
                    }
                    else
                    {
                        var style = new GUIStyle(GUI.skin.textArea);
                        style.wordWrap = true;
                        EditorGUI.PrefixLabel(rect, content);
                        rect = getRect(item.Key.TextLine * EditorGUIUtility.singleLineHeight);
                        var v = EditorGUI.TextArea(rect, (string)item.Field.GetValue(null), style);
                        item.Field.SetValue(null, v);
                    }
                }
                if (item.Field.FieldType == typeof(Color))
                {
                    var v = EditorGUI.ColorField(rect, content, (Color)item.Field.GetValue(null));
                    item.Field.SetValue(null, v);
                }

                if (!string.IsNullOrEmpty(item.Desc.Helpbox))
                {
                    rect = getRect(item.Key.TextLine * EditorGUIUtility.singleLineHeight);
                    EditorGUI.HelpBox(rect, item.Desc.Helpbox, MessageType.Info);
                }

                if (item.Key.Indent) EditorGUI.indentLevel--;
            }
        }

        private static void LoadAllPrefs()
        {
            foreach (var item in s_FieldList)
            {
                if (item.Key == null) continue;

                string key = "SceneViewHighlighter." + item.Field.Name.TrimStart('_');
                if (item.Field.FieldType == typeof(bool))
                {
                    var v = EditorPrefs.GetBool(key, item.Key.Enabled);
                    item.Field.SetValue(null, v);
                }
                if (item.Field.FieldType == typeof(int))
                {
                    var v = EditorPrefs.GetInt(key, item.Key.Integer);
                    item.Field.SetValue(null, v);
                }
                if (item.Field.FieldType == typeof(string))
                {
                    var v = EditorPrefs.GetString(key, item.Key.String);
                    item.Field.SetValue(null, v);
                }
                if (item.Field.FieldType == typeof(Color))
                {
                    var v = EditorPrefs.GetString(key, item.Key.Color);
                    item.Field.SetValue(null, v.ToColor(item.Key.Color.ToColor()));
                }
            }
        }

        public static void SaveAllPrefs(bool reset = false, string prefix = null)
        {
            foreach (var item in s_FieldList)
            {
                if (item.Key == null) continue;

                if (!string.IsNullOrEmpty(prefix) && !item.Field.Name.TrimStart('_').StartsWith(prefix))
                    continue;

                string key = "SceneViewHighlighter." + item.Field.Name.TrimStart('_');
                if (item.Field.FieldType == typeof(bool))
                {
                    bool v = reset ? item.Key.Enabled : (bool)item.Field.GetValue(null);
                    EditorPrefs.SetBool(key, v);
                }
                if (item.Field.FieldType == typeof(int))
                {
                    int v = reset ? item.Key.Integer : (int)item.Field.GetValue(null);
                    EditorPrefs.SetInt(key, v);
                }
                if (item.Field.FieldType == typeof(string))
                {
                    string v = reset ? item.Key.String : (string)item.Field.GetValue(null);
                    EditorPrefs.SetString(key, v);
                }
                if (item.Field.FieldType == typeof(Color))
                {
                    Color v = reset ? item.Key.Color.ToColor() : (Color)item.Field.GetValue(null);
                    EditorPrefs.SetString(key, v.ToPrefString());
                }
            }
        }
    }
}