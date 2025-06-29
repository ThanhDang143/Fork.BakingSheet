﻿// BakingSheet, Maxwell Keonwoo Kang <code.athei@gmail.com>, 2022

using Cathei.BakingSheet.Internal;

namespace Cathei.BakingSheet
{
    /// <summary>
    /// Generic ISheetAssetPath implementation.
    /// </summary>
    public class AssetPath : ISheetAssetPath
    {
        public string RawValue { get; }
        public string FullPath { get; }

        public virtual string BasePath => string.Empty;
        public virtual string Extension => string.Empty;
        public virtual string DirectorySeparator => "/";

        [Preserve]
        public AssetPath(string rawValue)
        {
            RawValue = rawValue;

            if (string.IsNullOrEmpty(RawValue))
                return;

            string filePath = RawValue;

            if (!string.IsNullOrEmpty(Extension))
                filePath = $"{filePath}.{Extension}";

            FullPath = CombinePath(BasePath, filePath, DirectorySeparator);
        }

        // Defining our own, since Path.Combine or similar methods does not support custom separator,
        public static string CombinePath(string basePath, string filePath, string separator)
        {
            if (string.IsNullOrEmpty(basePath))
                return filePath;

            if (basePath.EndsWith(separator))
                return $"{basePath}{filePath}";

            return $"{basePath}{separator}{filePath}";
        }
    }

    public static class AssetPathExtensions
    {
        public static bool IsValid(this ISheetAssetPath assetPath)
        {
            return !string.IsNullOrEmpty(assetPath?.RawValue);
        }
    }
}
