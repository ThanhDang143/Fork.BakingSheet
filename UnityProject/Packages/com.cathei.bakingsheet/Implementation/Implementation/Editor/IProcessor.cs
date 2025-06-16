using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cathei.BakingSheet;
using Cathei.BakingSheet.Unity;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace ThanhDV.Cathei.BakingSheet.Implementation
{
    public interface IProcessor
    {
        UniTask<bool> ConvertToJson();
        UniTask<bool> ConvertToScriptableObject();
    }
}
