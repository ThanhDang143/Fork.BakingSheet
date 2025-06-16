using System.Collections.Generic;
using Cathei.BakingSheet;
using Cathei.BakingSheet.Unity;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;

namespace ThanhDV.Cathei.BakingSheet.Implementation
{
    public class BakingSheetController
    {
        private List<SheetContainerBase> sheetContainers;

        public async UniTask<T> LoadContainer<T>(string containerAddress) where T : SheetContainerBase
        {
            sheetContainers ??= new();
            for (int i = 0; i < sheetContainers.Count; i++)
            {
                if (sheetContainers[i] is T sC) return sC;
            }

            var handle = Addressables.LoadAssetAsync<SheetContainerScriptableObject>(containerAddress);
            await handle.Task;

            SheetContainerScriptableObject containerSO = handle.Result;
            ScriptableObjectSheetImporter importer = new(containerSO);

            T sheetContainer = ProcessorUtilities.CreateSheetContainer(UnityLogger.Default, typeof(T)) as T;
            await sheetContainer.Bake(importer);

            sheetContainers.Add(sheetContainer);

            Addressables.Release(handle);

            return sheetContainer;
        }

        // public async UniTask<> LoadRow()
    }
}
