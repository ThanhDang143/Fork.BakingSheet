using System.Threading.Tasks;

namespace ThanhDV.Cathei.BakingSheet.Implementation
{
    public interface IProcessor
    {
        Task<bool> ConvertToJson();
        Task<bool> ConvertToScriptableObject();
    }
}
