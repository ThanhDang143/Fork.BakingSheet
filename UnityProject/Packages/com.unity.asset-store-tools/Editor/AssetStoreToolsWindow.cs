﻿using UnityEditor;
using UnityEngine;

namespace AssetStoreTools
{
    public abstract class AssetStoreToolsWindow : EditorWindow
    {
        protected abstract string WindowTitle { get; }

        protected virtual void Init()
        {
            titleContent = new GUIContent(WindowTitle);
        }

        private void OnEnable()
        {
            Init();
        }
        
    }
}