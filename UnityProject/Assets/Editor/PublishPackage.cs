using System.Diagnostics;
using System.IO;
using UnityEditor;
using UnityEngine;

namespace ThanhDV.CiCd
{
    public static class Publisher
    {
        [MenuItem("Tools/Publish Current Package")]
        public static void Publish()
        {
            string packageJsonPath = FindPackageJsonPath();

            if (string.IsNullOrEmpty(packageJsonPath))
            {
                UnityEngine.Debug.LogError("Could not find package.json. This script must be run within a Unity project containing the package.");
                return;
            }

            string packagePath = Path.GetDirectoryName(packageJsonPath);
            UnityEngine.Debug.Log($"Preparing to publish package at: {packagePath}");

            ProcessStartInfo processInfo = new ProcessStartInfo
            {
                FileName = "npm.cmd", 
                Arguments = "publish",
                WorkingDirectory = packagePath,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            Process process = new Process { StartInfo = processInfo };

            process.OutputDataReceived += (sender, e) => { if (e.Data != null) UnityEngine.Debug.Log(e.Data); };
            process.ErrorDataReceived += (sender, e) => { if (e.Data != null) UnityEngine.Debug.LogError(e.Data); };

            UnityEngine.Debug.Log("Starting 'npm publish'...");
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            
            process.WaitForExit();
            
            if (process.ExitCode == 0)
            {
                UnityEngine.Debug.Log("'npm publish' completed successfully!");
            }
            else
            {
                throw new System.Exception($"'npm publish' failed with exit code: {process.ExitCode}");
            }
        }

        private static string FindPackageJsonPath()
        {
            var projectRoot = new DirectoryInfo(Application.dataPath).Parent;

            if (projectRoot == null) return null;

            var packageFolders = new DirectoryInfo(Path.Combine(projectRoot.FullName, "Packages"));
            
            if (packageFolders.Exists)
            {
                foreach (var dir in packageFolders.GetDirectories())
                {
                    var files = dir.GetFiles("package.json", SearchOption.TopDirectoryOnly);
                    if (files.Length > 0)
                    {
                        return files[0].FullName;
                    }
                }
            }
            
            var rootFiles = projectRoot.GetFiles("package.json", SearchOption.TopDirectoryOnly);
            if (rootFiles.Length > 0)
            {
                return rootFiles[0].FullName;
            }

            return null;
        }
    }
}