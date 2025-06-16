using System;
using Cathei.BakingSheet;
using Cysharp.Threading.Tasks;

namespace ThanhDV.Cathei.BakingSheet.Implementation
{
    public interface IDataSheetController
    {
        UniTask<T> LoadContainerAsync<T>(string containerAddress) where T : SheetContainerBase;
    }
}
