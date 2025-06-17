using Cysharp.Threading.Tasks;

namespace ThanhDV.Cathei.BakingSheet.Implementation
{
    public interface IProcessor
    {
        UniTask<bool> ConvertToJson();
        UniTask<bool> ConvertToScriptableObject();
    }
}
