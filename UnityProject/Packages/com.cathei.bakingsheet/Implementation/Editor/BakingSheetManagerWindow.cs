using System;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace ThanhDV.Cathei.BakingSheet.Implementation
{
    // Removed dependency on Odin. Re-implemented UI with standard IMGUI.
    public class BakingSheetManagerWindow : EditorWindow
    {
        [MenuItem("Tools/Baking Sheet/Manager")]
        private static void OpenWindow()
        {
            ShowWindow();
        }

        private void OnEnable()
        {
            LoadData();
        }

        private void OnDestroy()
        {
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

        private void OnGUI()
        {
            string title = "Baking Sheet Manager";
            string subtitle = "Implemented by ThanhDV";
            EditorHelper.CreateHeader(title, subtitle);
            EditorGUILayout.Space();

            DrawPathSettings();
            EditorGUILayout.Space(12);
            DrawBakingSettings();

            GUILayout.FlexibleSpace();
            DrawNotification();
        }

        private void DrawLine()
        {
            var rect = EditorGUILayout.GetControlRect(false, 2);
            rect.height = 1;
            EditorGUI.DrawRect(rect, new Color(0.3f, 0.3f, 0.3f));
        }

        #region Path Settings
        private string excelPath = "Assets/_Assets/GameData/Excel";
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
                CommitTextFieldChange();
            }

            EditorPrefs.SetString(EditorPrefKeys.EXCEL_PATH, excelPath);
        }

        private string jsonPath = "Assets/_Assets/GameData/Json";
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
                CommitTextFieldChange();
            }

            EditorPrefs.SetString(EditorPrefKeys.JSON_PATH, jsonPath);
        }

        private string scriptableObjectPath = "Assets/_Assets/GameData/ScriptableObjects";
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
                CommitTextFieldChange();
            }

            EditorPrefs.SetString(EditorPrefKeys.SCRIPTABLE_OBJECT_PATH, scriptableObjectPath);
        }

        private void CreateExcelFolder()
        {
            if (string.IsNullOrWhiteSpace(excelPath))
            {
                ShowNotification("Excel Path is empty", MessageType.Error);
                return;
            }
            if (FileIO.CreatePath(excelPath, out Exception e))
            {
                ShowNotification($"Create directory at path '{excelPath}' success", MessageType.Info);
            }
            else
            {
                ShowNotification($"Failed to create directory at path '{excelPath}'. Error: {e.Message}", MessageType.Error);
            }
        }

        private void CreateJsonFolder()
        {
            if (string.IsNullOrWhiteSpace(jsonPath))
            {
                ShowNotification("Json Path is empty", MessageType.Error);
                return;
            }
            if (FileIO.CreatePath(jsonPath, out Exception e))
            {
                ShowNotification($"Create directory at path '{jsonPath}' success", MessageType.Info);
            }
            else
            {
                ShowNotification($"Failed to create directory at path '{jsonPath}'. Error: {e.Message}", MessageType.Error);
            }
        }

        private void CreateScriptableObjectFolder()
        {
            if (string.IsNullOrWhiteSpace(scriptableObjectPath))
            {
                ShowNotification("ScriptableObject Path is empty", MessageType.Error);
                return;
            }
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
            if (string.IsNullOrWhiteSpace(excelPath))
            {
                ShowNotification("Excel Path is empty", MessageType.Warning);
                return;
            }
            FileIO.OpenFolder(excelPath);
        }

        private void OpenJsonFolder()
        {
            if (string.IsNullOrWhiteSpace(jsonPath))
            {
                ShowNotification("Json Path is empty", MessageType.Warning);
                return;
            }
            FileIO.OpenFolder(jsonPath);
        }

        private void OpenSOFolder()
        {
            if (string.IsNullOrWhiteSpace(scriptableObjectPath))
            {
                ShowNotification("ScriptableObject Path is empty", MessageType.Warning);
                return;
            }
            FileIO.OpenFolder(scriptableObjectPath);
        }

        private void SavePathSettings()
        {
            if (!ArePathsValid())
            {
                ShowNotification("Cannot save: one or more paths are empty", MessageType.Error);
                return;
            }
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

        private bool ArePathsValid()
        {
            return !string.IsNullOrWhiteSpace(excelPath) &&
                   !string.IsNullOrWhiteSpace(jsonPath) &&
                   !string.IsNullOrWhiteSpace(scriptableObjectPath);
        }

        private bool SafePathExists(string path)
        {
            if (string.IsNullOrWhiteSpace(path)) return false;
            try { return FileIO.IsExistPath(path); }
            catch { return false; }
        }

        // Force IMGUI text fields to refresh displayed value when we change bound string programmatically while focused.
        private void CommitTextFieldChange()
        {
            EditorGUIUtility.editingTextField = false; // end current text editing session
            GUIUtility.keyboardControl = 0; // remove focus
            Repaint();
        }

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
            if (!isShowingNotification) return;
            var color = messageType switch
            {
                MessageType.Error => new Color(0.8f, 0.3f, 0.3f),
                MessageType.Warning => new Color(0.9f, 0.6f, 0.2f),
                MessageType.Info => new Color(0.3f, 0.6f, 0.9f),
                _ => new Color(0.5f, 0.5f, 0.5f)
            };
            EditorGUILayout.Space();
            var prev = GUI.backgroundColor;
            GUI.backgroundColor = color;
            EditorGUILayout.HelpBox(notificationMessage, messageType);
            GUI.backgroundColor = prev;
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
                await Task.Delay(duration, cancellationToken: cancellationToken.Token);
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

        private async void BakeExcelToScriptableObject()
        {
            if (!ArePathsValid())
            {
                ShowNotification("Please fill all paths before baking", MessageType.Error);
                return;
            }
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

        private void DeleteJson()
        {
            FileIO.ClearFolderContents(jsonPath);
        }

        private void DeleteSO()
        {
            FileIO.ClearFolderContents(scriptableObjectPath);
        }

        private void DrawPathSettings()
        {
            GUILayout.Label("Path Settings", EditorStyles.boldLabel);
            EditorHelper.DrawHorizontalLine(1, -5, Vector4.one * 0.5f);
            GUILayout.Space(10);

            // Excel Path Field + Buttons
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Excel Path", GUILayout.Width(150));
            excelPath = EditorGUILayout.TextField(excelPath);
            if (GUILayout.Button("Open", GUILayout.Width(50))) OpenExcelFolder();
            if (GUILayout.Button("...", GUILayout.Width(30))) ChooseExcelFolder();
            EditorGUILayout.EndHorizontal();
            if (string.IsNullOrWhiteSpace(excelPath))
            {
                EditorGUILayout.HelpBox("Excel Path is empty", MessageType.Warning);
            }
            else if (!SafePathExists(excelPath))
            {
                if (GUILayout.Button("Create Excel Folder")) CreateExcelFolder();
            }

            // JSON Path
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("Json Path", GUILayout.Width(150));
            jsonPath = EditorGUILayout.TextField(jsonPath);
            if (GUILayout.Button("Open", GUILayout.Width(50))) OpenJsonFolder();
            if (GUILayout.Button("...", GUILayout.Width(30))) ChooseJsonFolder();
            EditorGUILayout.EndHorizontal();
            if (string.IsNullOrWhiteSpace(jsonPath))
            {
                EditorGUILayout.HelpBox("Json Path is empty", MessageType.Warning);
            }
            else if (!SafePathExists(jsonPath))
            {
                if (GUILayout.Button("Create Json Folder")) CreateJsonFolder();
            }

            // ScriptableObject Path
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField("ScriptableObject Path", GUILayout.Width(150));
            scriptableObjectPath = EditorGUILayout.TextField(scriptableObjectPath);
            if (GUILayout.Button("Open", GUILayout.Width(50))) OpenSOFolder();
            if (GUILayout.Button("...", GUILayout.Width(30))) ChooseScriptableObjectFolder();
            EditorGUILayout.EndHorizontal();
            if (string.IsNullOrWhiteSpace(scriptableObjectPath))
            {
                EditorGUILayout.HelpBox("ScriptableObject Path is empty", MessageType.Warning);
            }
            else if (!SafePathExists(scriptableObjectPath))
            {
                if (GUILayout.Button("Create ScriptableObject Folder")) CreateScriptableObjectFolder();
            }

            EditorGUILayout.Space(6);
            EditorGUILayout.BeginHorizontal();
            if (IsAllPathSaved())
            {
                var prev = GUI.color;
                GUI.color = ArePathsValid() ? Color.green : new Color(0.7f, 0.7f, 0.7f);
                GUI.enabled = ArePathsValid();
                if (GUILayout.Button("Save Path Settings", GUILayout.Height(24))) SavePathSettings();
                GUI.enabled = true;
                GUI.color = prev;
            }
            var prev2 = GUI.color;
            GUI.color = Color.red;
            if (GUILayout.Button("Reset Path Settings", GUILayout.Height(24))) ResetPathSettings();
            GUI.color = prev2;
            EditorGUILayout.EndHorizontal();
        }

        private void DrawBakingSettings()
        {
            GUILayout.Label("Baking Sheet Settings", EditorStyles.boldLabel);
            EditorHelper.DrawHorizontalLine(1, -5, Vector4.one * 0.5f);
            GUILayout.Space(10);

            EditorGUILayout.BeginHorizontal();
            GUI.enabled = !isBaking;
            if (GUILayout.Button("Bake Excel To ScriptableObject", GUILayout.Height(30)))
            {
                BakeExcelToScriptableObject();
            }
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            GUI.enabled = !isBaking;
            var prev = GUI.color;
            GUI.color = Color.red;
            if (GUILayout.Button("Delete Json Contents")) DeleteJson();
            if (GUILayout.Button("Delete ScriptableObject Contents")) DeleteSO();
            GUI.color = prev;
            GUI.enabled = true;
            EditorGUILayout.EndHorizontal();
        }
        #endregion
    }
}