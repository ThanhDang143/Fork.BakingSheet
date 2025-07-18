// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using Cathei.BakingSheet.Unity;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace Cathei.BakingSheet.Editor
{
    [CustomEditor(typeof(SheetRowScriptableObject), true)]
    public class SheetRowCustomInspector : UnityEditor.Editor
    {
        private SerializedProperty serializedRow;
        private SerializedProperty unityReferences;
        private StyleSheet styleSheet;

        private void OnEnable()
        {
            serializedRow = serializedObject.FindProperty("serializedRow");
            unityReferences = serializedObject.FindProperty("references");
            styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>("Packages/com.cathei.bakingsheet/Editor/StyleSheet.uss");
        }

        public override VisualElement CreateInspectorGUI()
        {
            var inspector = new VisualElement();

            ApplyStyles(inspector);

            var jObject = JObject.Parse(serializedRow.stringValue);

            ExpandIdField(inspector, jObject.Value<string>(nameof(ISheetRow.Id)));

            foreach (var pair in jObject)
            {
                if (pair.Key == nameof(ISheetRow.Id))
                    continue;

                ExpandJsonToken(inspector, pair.Key, pair.Value);
            }

            return inspector;
        }

        private void ApplyStyles(VisualElement inspector)
        {
            if (styleSheet != null)
            {
                inspector.styleSheets.Add(styleSheet);
                return;
            }

            var dimgray = new StyleColor(new Color(0.41f, 0.41f, 0.41f));

            inspector.schedule.Execute(() =>
            {
                inspector.Query<Label>().Build().ForEach(label => label.style.minWidth = 130);
            });

            inspector.schedule.Execute(() =>
            {
                inspector.Query<TextField>(className: "readonly").Build().ForEach(field =>
                {
                    var textInput = field.Q(className: TextField.inputUssClassName);
                    if (textInput != null) textInput.style.color = dimgray;
                });
            });

            inspector.schedule.Execute(() =>
            {
                inspector.Query<ObjectField>(className: "readonly").Build().ForEach(field =>
                {
                    var label = field.Q<Label>();
                    if (label != null)
                        label.style.color = dimgray;

                    var selector = field.Q(className: "unity-object-field-selector");
                    if (selector != null)
                        selector.style.display = DisplayStyle.None;
                });
            });
        }

        private void ExpandJsonToken(VisualElement parent, string label, JToken jToken)
        {
            switch (jToken.Type)
            {
                case JTokenType.Object:
                    ExpandJsonObject(parent, label, (JObject)jToken);
                    break;

                case JTokenType.Array:
                    ExpandJsonArray(parent, label, (JArray)jToken);
                    break;

                default:
                    ExpandJsonValue(parent, label, jToken);
                    break;
            }
        }

        private void ExpandJsonObject(VisualElement parent, string label, JObject jObject)
        {
            if (jObject.TryGetValue("$type", out var metaType))
            {
                switch (metaType.Value<string>())
                {
                    case SheetMetaType.UnityObject:
                        ExpandUnityReference(parent, label, jObject.GetValue("Value"));
                        return;

                    case SheetMetaType.DirectAssetPath:
                        ExpandUnityReference(parent, label, jObject.SelectToken("Asset.Value"));
                        return;

                    case SheetMetaType.ResourcePath:
                        ExpandResourcePath(parent, label, jObject.SelectToken("FullPath"), jObject.SelectToken("SubAssetName"));
                        return;

                    case SheetMetaType.AddressablePath:
                        ExpandJsonToken(parent, label, jObject.GetValue("RawValue"));
                        return;
                }
            }

            var foldout = new Foldout
            {
                text = label
            };

            var box = new Box();

            parent.Add(foldout);
            foldout.Add(box);

            foreach (var pair in jObject)
            {
                if (pair.Key.StartsWith("$"))
                    continue;

                ExpandJsonToken(box, pair.Key, pair.Value);
            }
        }

        private void ExpandJsonArray(VisualElement parent, string label, JArray jArray)
        {
            var foldout = new Foldout
            {
                text = label
            };

            parent.Add(foldout);

            for (int i = 0; i < jArray.Count; ++i)
            {
                ExpandJsonToken(foldout, $"Element {i}", jArray[i]);
            }
        }

        private void ExpandJsonValue(VisualElement parent, string label, JToken jToken)
        {
            var child = new TextField
            {
                label = label,
                value = jToken.Value<string>(),
                isReadOnly = true
            };

            child.AddToClassList("readonly");

            parent.Add(child);
        }

        private void ExpandIdField(VisualElement parent, string value)
        {
            var child = new TextField
            {
                label = nameof(ISheetRow.Id),
                value = value,
                isReadOnly = true
            };

            child.AddToClassList("readonly");

            parent.Add(child);

            // var button = new Button
            // {
            //     text = "Change"
            // };
            //
            // button.RegisterCallback<ClickEvent, TextField>(HandleNameChange, child);
            //
            // child.Add(button);
        }

        // private void HandleNameChange(ClickEvent evt, TextField child)
        // {
        //     serializedObject.targetObject.name = child.text;
        //     serializedObject.ApplyModifiedProperties();
        //     AssetDatabase.SaveAssets();
        // }

        private void ExpandUnityReference(VisualElement parent, string label, JToken jToken)
        {
            int refIndex = jToken?.Value<int>() ?? -1;
            UnityEngine.Object refObj = null;

            if (0 <= refIndex && refIndex < unityReferences.arraySize)
                refObj = unityReferences.GetArrayElementAtIndex(refIndex).objectReferenceValue;

            var child = new ObjectField
            {
                label = label,
                value = refObj,
                objectType = typeof(UnityEngine.Object)
            };

            child.AddToClassList("readonly");
            parent.Add(child);
        }

        private void ExpandResourcePath(VisualElement parent, string label, JToken pathToken, JToken subAssetToken)
        {
            string path = pathToken?.Value<string>();
            string subAssetName = subAssetToken?.Value<string>();

            UnityEngine.Object refObj = null;

            if (!string.IsNullOrEmpty(path))
                refObj = ResourcePath.Load(path, subAssetName);

            var child = new ObjectField
            {
                label = label,
                value = refObj,
                objectType = typeof(UnityEngine.Object)
            };

            child.AddToClassList("readonly");
            parent.Add(child);
        }
    }
}