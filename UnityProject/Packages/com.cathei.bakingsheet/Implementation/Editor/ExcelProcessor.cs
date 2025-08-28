#if UNITY_EDITOR
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Cathei.BakingSheet;
using Cathei.BakingSheet.Unity;
using UnityEditor;
using UnityEditor.AddressableAssets;
using UnityEditor.AddressableAssets.Settings;
using UnityEngine;
using AddressableGroupSchemas = UnityEditor.AddressableAssets.Settings.GroupSchemas;

namespace ThanhDV.Cathei.BakingSheet.Implementation
{
    public class ExcelProcessor : IProcessor
    {
        public async Task<bool> ConvertToJson()
        {
            List<Task<bool>> convertTasks = new();
            List<Type> containerTypes = ProcessorUtilities.FindSheetContainerType();

            if (containerTypes == null || containerTypes.Count <= 0)
            {
                Debug.LogError("[BackingSheet]  No valid SheetContainer found to convert!!!");
                return false;
            }

            foreach (Type containerType in containerTypes)
            {
                convertTasks.Add(ConvertToJsonTask(containerType));
            }

            bool[] results = await Task.WhenAll(convertTasks);

            if (results.Any(task => !task)) return false;

            return true;
        }

        private async Task<bool> ConvertToJsonTask(Type containerType)
        {
            UnityLogger logger = UnityLogger.Default;

            SheetContainerBase sheetContainer = ProcessorUtilities.CreateSheetContainer(logger, containerType);
            if (sheetContainer == null) return false;

            string excelPath = EditorPrefs.GetString(EditorPrefKeys.EXCEL_PATH);
            ExcelSheetConverter excelConverter = new(excelPath, TimeZoneInfo.Utc);
            await sheetContainer.Bake(excelConverter);

            if (!VerifySheetContainer(sheetContainer, containerType)) return false;

            sheetContainer.Verify(); // Không có tác dụng nếu không tạo SheetVerifier hoặc override VerifyAssets 

            string jsonPath = EditorPrefs.GetString(EditorPrefKeys.JSON_PATH);
            JsonSheetConverter jsonConverter = new(jsonPath);
            await sheetContainer.Store(jsonConverter);

            AssetDatabase.Refresh();

            Debug.Log($"<color=green>[BakingSheet] Convert Excel {containerType.Name} to Json successfully!!!</color>");
            return true;
        }

        public async Task<bool> ConvertToScriptableObject()
        {
            IProcessor processor = this;
            List<Task<bool>> convertTasks = new();
            List<Type> containerTypes = ProcessorUtilities.FindSheetContainerType();

            if (containerTypes == null || containerTypes.Count <= 0)
            {
                Debug.LogError("[BackingSheet]  No valid SheetContainer found to convert!!!");
                return false;
            }

            foreach (Type containerType in containerTypes)
            {
                convertTasks.Add(ConvertToSOTask(containerType));
            }

            bool[] results = await Task.WhenAll(convertTasks);

            if (results.Any(task => !task)) return false;

            return true;
        }

        private async Task<bool> ConvertToSOTask(Type containerType)
        {
            UnityLogger logger = UnityLogger.Default;
            IProcessor processor = this;

            SheetContainerBase sheetContainer = ProcessorUtilities.CreateSheetContainer(logger, containerType);
            if (sheetContainer == null) return false;

            string jsonPath = EditorPrefs.GetString(EditorPrefKeys.JSON_PATH);
            JsonSheetConverter importer = new(jsonPath);
            await sheetContainer.Bake(importer);

            if (!VerifySheetContainer(sheetContainer, containerType)) return false;

            sheetContainer.Verify(); // Không có tác dụng nếu không tạo SheetVerifier hoặc override VerifyAssets 

            string sOPath = EditorPrefs.GetString(EditorPrefKeys.SCRIPTABLE_OBJECT_PATH);
            ScriptableObjectSheetExporter exporter = new(sOPath);
            await sheetContainer.Store(exporter);

            AssetDatabase.Refresh();

            MakeAddressable(exporter, sOPath);

            Debug.Log($"<color=green>[BakingSheet] Convert Json {containerType.Name} to Scriptable Object successfully!!!</color>");
            return true;
        }

        private void MakeAddressable(ScriptableObjectSheetExporter exporter, string sOPath)
        {
            string assetName = $"{exporter.Result.name}.asset";
            string assetPath = Path.Combine(sOPath, assetName);
            string guid = AssetDatabase.AssetPathToGUID(assetPath);

            if (string.IsNullOrEmpty(guid))
            {
                Debug.LogError($"[BakingSheet] Could not find asset at path {assetPath} to make it addressable. GUID is null or empty.");
            }
            else
            {
                AddressableAssetSettings settings = AddressableAssetSettingsDefaultObject.Settings;
                if (settings == null)
                {
                    Debug.LogError("[BakingSheet] Addressable Asset Settings not found. Please initialize Addressables in your project (Window > Asset Management > Addressables > Groups, then click 'Create Addressables Settings').");
                }
                else
                {
                    AddressableAssetGroup group = settings.FindGroup("BakingSheet");
                    if (group == null)
                    {
                        group = settings.CreateGroup("BakingSheet", false, false, true, null, typeof(AddressableGroupSchemas.BundledAssetGroupSchema));
                        if (group == null)
                        {
                            Debug.LogError("[BakingSheet] No Addressable Asset Group found or could be created. Cannot make asset addressable.");
                        }
                    }

                    if (group != null)
                    {
                        AddressableAssetEntry entry = settings.CreateOrMoveEntry(guid, group, false, false);
                        if (entry != null)
                        {
                            entry.address = exporter.Result.name;
                            settings.SetDirty(AddressableAssetSettings.ModificationEvent.EntryModified, entry, true);
                            Debug.Log($"<color=green>[BakingSheet] Made ScriptableObject '{assetName}' addressable with address '{entry.address}' in group '{group.Name}'.</color>");
                        }
                        else
                        {
                            Debug.LogError($"[BakingSheet] Failed to create or move Addressable entry for {assetName} (GUID: {guid}).");
                        }
                    }
                }
            }
        }

        private bool VerifySheetContainer(SheetContainerBase sheetContainer, Type containerType)
        {
            foreach (var prop in sheetContainer.GetSheetProperties().Values)
            {
                ISheet sheet = prop.GetValue(sheetContainer) as ISheet;
                if (sheet == null || sheet.Count <= 0)
                {
                    Debug.LogError($"[BakingSheet] Sheet '{prop.Name}' in container '{containerType.Name}' is empty or was not loaded after baking. Check if a corresponding sheet tab exists in Excel files.");
                    return false;
                }
            }

            return true;
        }
    }
}
#endif