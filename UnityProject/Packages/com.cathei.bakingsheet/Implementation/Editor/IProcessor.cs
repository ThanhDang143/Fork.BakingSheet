using System.Threading.Tasks;

namespace ThanhDV.Cathei.BakingSheet
{
    public interface IProcessor
    {
        Task<bool> ConvertToJson();
        Task<bool> ConvertToScriptableObject();
    }
}
