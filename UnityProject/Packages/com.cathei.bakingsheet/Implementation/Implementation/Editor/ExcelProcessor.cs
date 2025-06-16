using System;
using System.Collections.Generic;
using System.Linq;
using Cathei.BakingSheet;
using Cathei.BakingSheet.Unity;
using Cysharp.Threading.Tasks;
using UnityEditor;
using UnityEngine;

namespace ThanhDV.Cathei.BakingSheet.Implementation
{
    public class ExcelProcessor : IProcessor
    {
        public async UniTask<bool> ConvertToJson()
        {
            List<UniTask<bool>> convertTasks = new();
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

            bool[] results = await UniTask.WhenAll(convertTasks);

            if (results.Any(task => !task)) return false;

            return true;
        }

        private async UniTask<bool> ConvertToJsonTask(Type containerType)
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

        public async UniTask<bool> ConvertToScriptableObject()
        {
            IProcessor processor = this;
            List<UniTask<bool>> convertTasks = new();
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

            bool[] results = await UniTask.WhenAll(convertTasks);

            if (results.Any(task => !task)) return false;

            return true;
        }

        private async UniTask<bool> ConvertToSOTask(Type containerType)
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

            Debug.Log($"<color=green>[BakingSheet] Convert Json {containerType.Name} to Scriptable Object successfully!!!</color>");
            return true;
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
