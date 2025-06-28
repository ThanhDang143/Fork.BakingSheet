#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ThanhDV.Cathei.BakingSheet.Implementation
{
    public static class FileIO
    {
        public static bool CreatePath(string path, out System.Exception e)
        {
            e = null;
            try
            {
                string fullPath = Path.GetFullPath(path);

                Directory.CreateDirectory(fullPath);
                AssetDatabase.Refresh();

                return true;
            }
            catch (System.Exception ex)
            {
                e = ex;
                return false;
            }
        }

        public static bool IsExistPath(string path)
        {
            string fullPath = Path.GetFullPath(path);
            return Directory.Exists(fullPath);
        }

        public static void OpenFolder(string path)
        {
            if (!IsExistPath(path))
            {
                Debug.LogError("[BakingSheet] Folder is not exist!!!");
                return;
            }

            Object folderObject = AssetDatabase.LoadAssetAtPath<Object>(path);

            if (folderObject == null)
            {
                Debug.LogError("[BakingSheet] Folder is not exist!!!");
                return;
            }

            EditorUtility.FocusProjectWindow();

            EditorGUIUtility.PingObject(folderObject);

            Selection.activeObject = folderObject;
        }

        public static bool ClearFolderContents(string path)
        {
            if (!IsExistPath(path))
            {
                Debug.LogError("[BakingSheet] Folder is not exist!!!");
                return false;
            }

            path = path.TrimEnd('/');

            if (path.ToLower() == "assets")
            {
                Debug.LogError("[BakingSheet] Can not delete Assets!!!");
                return false;
            }

            try
            {
                string[] allEntries = Directory.GetFileSystemEntries(path);

                foreach (string entryPath in allEntries)
                {
                    if (entryPath.Contains(".meta")) continue;

                    if (AssetDatabase.DeleteAsset(entryPath))
                    {
                        Debug.Log($"[BakingSheet] Deleted: {entryPath}");
                    }
                    else
                    {
                        Debug.LogError($"[BakingSheet] Can not delete: {entryPath}");
                    }
                }

                AssetDatabase.Refresh();
                Debug.Log("[BakingSheet] Deleted completed!!!");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[BakingSheet] Fail to clear folder: {e.Message}!!!");
                return false;
            }
        }

    }
}
#endif