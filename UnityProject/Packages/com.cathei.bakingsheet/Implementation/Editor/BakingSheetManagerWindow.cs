#if UNITY_EDITOR
using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using Sirenix.OdinInspector;
using Sirenix.OdinInspector.Editor;
using Sirenix.Utilities.Editor;
using UnityEditor;
using UnityEngine;

namespace ThanhDV.Cathei.BakingSheet.Implementation
{
    public class BakingSheetManagerWindow : OdinEditorWindow
    {
        [MenuItem("Tools/Baking Sheet/Manager")]
        private static void OpenWindow()
        {
            ShowWindow();
        }

        protected override void OnEnable()
        {
            LoadData();
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            if (cancellationToken != null)
            {
                cancellationToken.Cancel();
                cancellationToken.Dispose();
                cancellationToken = null;
            }
        }

        private static void ShowWindow()
        {
            BakingSheetManagerWindow window = GetWindow<BakingSheetManagerWindow>();
            window.titleContent = new GUIContent("Baking Sheet Manager");
            window.minSize = new Vector2(400, 300);
            window.Show();
        }

        private void LoadData()
        {
            if (EditorPrefs.HasKey(EditorPrefKeys.EXCEL_PATH))
            {
                excelPath = EditorPrefs.GetString(EditorPrefKeys.EXCEL_PATH);
            }
            else
            {
                EditorPrefs.SetString(EditorPrefKeys.EXCEL_PATH, "Assets/_Assets/GameData/Excel");
                excelPath = EditorPrefs.GetString(EditorPrefKeys.EXCEL_PATH);
            }

            if (EditorPrefs.HasKey(EditorPrefKeys.SCRIPTABLE_OBJECT_PATH))
            {
                scriptableObjectPath = EditorPrefs.GetString(EditorPrefKeys.SCRIPTABLE_OBJECT_PATH);
            }
            else
            {
                EditorPrefs.SetString(EditorPrefKeys.SCRIPTABLE_OBJECT_PATH, "Assets/_Assets/GameData/ScriptableObjects");
                scriptableObjectPath = EditorPrefs.GetString(EditorPrefKeys.SCRIPTABLE_OBJECT_PATH);
            }

            if (EditorPrefs.HasKey(EditorPrefKeys.JSON_PATH))
            {
                jsonPath = EditorPrefs.GetString(EditorPrefKeys.JSON_PATH);
            }
            else
            {
                EditorPrefs.SetString(EditorPrefKeys.JSON_PATH, "Assets/_Assets/GameData/Json");
                jsonPath = EditorPrefs.GetString(EditorPrefKeys.JSON_PATH);
            }
        }

        protected override void OnImGUI()
        {
            ShowHeader();

            base.OnImGUI();

            DrawNotification();
        }

        private GUIStyle titleStyle;
        private GUIStyle subtitleStyle;
        private void ShowHeader()
        {
            titleStyle ??= new GUIStyle(EditorStyles.boldLabel)
            {
                fontSize = 30,
                alignment = TextAnchor.MiddleCenter,
            };

            subtitleStyle ??= new GUIStyle(EditorStyles.label)
            {
                fontSize = 12,
                alignment = TextAnchor.MiddleCenter
            };

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Baking Sheet Manager", titleStyle, GUILayout.Height(50)); // Title
            EditorGUILayout.LabelField("Implemented by ThanhDV", subtitleStyle); // Subtitle
            EditorGUILayout.Space();
            SirenixEditorGUI.HorizontalLineSeparator(Color.gray, 1); // Draw horizontal line.
        }

        #region Path Settings
        [Title("Path Settings")]
        [SerializeField, InlineButton("OpenExcelFolder", SdfIconType.FolderSymlinkFill, ""), InlineButton("ChooseExcelFolder", SdfIconType.FolderFill, ""), LabelWidth(150)] private string excelPath = "Assets/_Assets/GameData/Excel";
        private void ChooseExcelFolder()
        {
            string path = EditorUtility.OpenFolderPanel("Select Excel Folder", excelPath, "");
            if (!string.IsNullOrEmpty(path))
            {
                if (path.StartsWith(Application.dataPath))
                {
                    excelPath = "Assets" + path.Substring(Application.dataPath.Length);
                }
                else
                {
                    excelPath = path;
                }
            }

            EditorPrefs.SetString(EditorPrefKeys.EXCEL_PATH, excelPath);
        }

        [SerializeField, InlineButton("OpenJsonFolder", SdfIconType.FolderSymlinkFill, ""), InlineButton("ChooseJsonFolder", SdfIconType.FolderFill, ""), LabelWidth(150)] private string jsonPath = "Assets/_Assets/GameData/Json";
        private void ChooseJsonFolder()
        {
            string path = EditorUtility.OpenFolderPanel("Select Json Folder", jsonPath, "");
            if (!string.IsNullOrEmpty(path))
            {
                if (path.StartsWith(Application.dataPath))
                {
                    jsonPath = "Assets" + path.Substring(Application.dataPath.Length);
                }
                else
                {
                    jsonPath = path;
                }
            }

            EditorPrefs.SetString(EditorPrefKeys.JSON_PATH, jsonPath);
        }

        [SerializeField, InlineButton("OpenSOFolder", SdfIconType.FolderSymlinkFill, ""), InlineButton("ChooseScriptableObjectFolder", SdfIconType.FolderFill, ""), LabelWidth(150)] private string scriptableObjectPath = "Assets/_Assets/GameData/ScriptableObjects";
        private void ChooseScriptableObjectFolder()
        {
            string path = EditorUtility.OpenFolderPanel("Select Scriptable Object Folder", scriptableObjectPath, "");
            if (!string.IsNullOrEmpty(path))
            {
                if (path.StartsWith(Application.dataPath))
                {
                    scriptableObjectPath = "Assets" + path.Substring(Application.dataPath.Length);
                }
                else
                {
                    scriptableObjectPath = path;
                }
            }

            EditorPrefs.SetString(EditorPrefKeys.SCRIPTABLE_OBJECT_PATH, scriptableObjectPath);
        }

        [HorizontalGroup("PathButton"), Button, ShowIf("@!FileIO.IsExistPath(excelPath)")]
        private void CreateExcelFolder()
        {
            if (FileIO.CreatePath(excelPath, out Exception e))
            {
                ShowNotification($"Create directory at path '{excelPath}' success", MessageType.Info);
            }
            else
            {
                ShowNotification($"Failed to create directory at path '{excelPath}'. Error: {e.Message}", MessageType.Error);
            }
        }

        [HorizontalGroup("PathButton"), Button, ShowIf("@!FileIO.IsExistPath(jsonPath)")]
        private void CreateJsonFolder()
        {
            if (FileIO.CreatePath(jsonPath, out Exception e))
            {
                ShowNotification($"Create directory at path '{jsonPath}' success", MessageType.Info);
            }
            else
            {
                ShowNotification($"Failed to create directory at path '{jsonPath}'. Error: {e.Message}", MessageType.Error);
            }
        }

        [HorizontalGroup("PathButton"), Button, ShowIf("@!FileIO.IsExistPath(scriptableObjectPath)")]
        private void CreateScriptableObjectFolder()
        {
            if (FileIO.CreatePath(scriptableObjectPath, out Exception e))
            {
                ShowNotification($"Create directory at path '{scriptableObjectPath}' success", MessageType.Info);
            }
            else
            {
                ShowNotification($"Failed to create directory at path '{scriptableObjectPath}'. Error: {e.Message}", MessageType.Error);
            }
        }



        private void OpenExcelFolder()
        {
            FileIO.OpenFolder(excelPath);
        }

        private void OpenJsonFolder()
        {
            FileIO.OpenFolder(jsonPath);
        }

        private void OpenSOFolder()
        {
            FileIO.OpenFolder(scriptableObjectPath);
        }

        [Button(ButtonSizes.Medium), GUIColor("green"), ShowIf("@IsAllPathSaved()")]
        private void SavePathSettings()
        {
            EditorPrefs.SetString(EditorPrefKeys.EXCEL_PATH, excelPath);
            EditorPrefs.SetString(EditorPrefKeys.JSON_PATH, jsonPath);
            EditorPrefs.SetString(EditorPrefKeys.SCRIPTABLE_OBJECT_PATH, scriptableObjectPath);
        }

        private bool IsAllPathSaved()
        {
            bool excelPathSaved = excelPath == EditorPrefs.GetString(EditorPrefKeys.EXCEL_PATH);
            bool jsonPathSaved = jsonPath == EditorPrefs.GetString(EditorPrefKeys.JSON_PATH);
            bool sOPathSaved = scriptableObjectPath == EditorPrefs.GetString(EditorPrefKeys.SCRIPTABLE_OBJECT_PATH);

            return !excelPathSaved || !jsonPathSaved || !sOPathSaved;
        }

        [Button(ButtonSizes.Medium), GUIColor("red")]
        private void ResetPathSettings()
        {
            EditorPrefs.DeleteAll();
            LoadData();
            ShowNotification($"Reset path successfully!!!", MessageType.Error);
        }

        #endregion

        #region Notification 

        private CancellationTokenSource cancellationToken;
        private bool isShowingNotification;
        private string notificationMessage;
        private MessageType messageType;

        private void DrawNotification()
        {
            if (isShowingNotification) SirenixEditorGUI.MessageBox("   " + notificationMessage, messageType);
        }

        private async void ShowNotification(string _message, MessageType _messageType, int duration = 3000)
        {
            notificationMessage = _message;
            messageType = _messageType;
            isShowingNotification = true;

            if (cancellationToken != null)
            {
                cancellationToken.Cancel();
                cancellationToken.Dispose();
            }

            cancellationToken = new CancellationTokenSource();

            try
            {
                await UniTask.Delay(duration, cancellationToken: cancellationToken.Token);
                isShowingNotification = false;
                Repaint();
            }
            catch (OperationCanceledException)
            {
                isShowingNotification = false;
                Repaint();
            }
        }

        #endregion

        #region Baking Sheet
        private bool isBaking = false;
        [Title("Baking Sheet Settings", "", TitleAlignments.Split)]

        [Button("Bake Excel To ScriptableObject", ButtonSizes.Medium), DisableIf("isBaking")]
        private async void BakeExcelToScriptableObject()
        {
            ExcelProcessor excelProcessor = new();

            isBaking = true;

            bool convertToJson = await excelProcessor.ConvertToJson();
            if (!convertToJson)
            {
                isBaking = false;
                return;
            }
            await excelProcessor.ConvertToScriptableObject();
            isBaking = false;

            ShowNotification("Baking Excel to ScriptableObject complete!", MessageType.Info);
        }

        [HorizontalGroup("BSS"), Button("Delete Json Contents", ButtonSizes.Medium), GUIColor("red"), DisableIf("isBaking")]
        private void DeleteJson()
        {
            FileIO.ClearFolderContents(jsonPath);
        }

        [HorizontalGroup("BSS"), Button("Delete ScriptableObject Contents", ButtonSizes.Medium), GUIColor("red"), DisableIf("isBaking")]
        private void DeleteSO()
        {
            FileIO.ClearFolderContents(scriptableObjectPath);
        }
        #endregion
    }
}
#endif