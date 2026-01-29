#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ThanhDV.Cathei.BakingSheet
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
                Debug.Log($"<color=red>[BakingSheet] Folder is not exist!!!</color>");
                return;
            }

            Object folderObject = AssetDatabase.LoadAssetAtPath<Object>(path);

            if (folderObject == null)
            {
                Debug.Log($"<color=red>[BakingSheet] Folder is not exist!!!</color>");
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
                Debug.Log($"<color=red>[BakingSheet] Folder is not exist!!!</color>");
                return false;
            }

            path = path.TrimEnd('/');

            if (path.ToLower() == "assets")
            {
                Debug.Log($"<color=red>[BakingSheet] Can not delete Assets!!!</color>");
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
                        Debug.Log($"<color=red>[BakingSheet] Deleted: {entryPath}!!!</color>");
                    }
                    else
                    {
                        Debug.Log($"<color=red>[BakingSheet] Can not delete: {entryPath}!!!</color>");
                    }
                }

                AssetDatabase.Refresh();
                Debug.Log($"<color=red>[BakingSheet] Deleted completed!!!</color>");
                return true;
            }
            catch (System.Exception e)
            {
                Debug.Log($"<color=red>[BakingSheet] Fail to clear folder: {e.Message}!!!</color>");
                return false;
            }
        }

    }
}
#endif