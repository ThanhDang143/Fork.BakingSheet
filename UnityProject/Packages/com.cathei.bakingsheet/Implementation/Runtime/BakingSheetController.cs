using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using Cathei.BakingSheet;
using Cathei.BakingSheet.Unity;
using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

namespace ThanhDV.Cathei.BakingSheet.Implementation
{
    /// <summary>
    /// Manages loading and caching of BakingSheet data containers.
    /// Thread-safe singleton controller with automatic initialization.
    /// </summary>
    public class BakingSheetController : IDisposable
    {
        #region Singleton
        private static BakingSheetController _instance;
        private static readonly object _lock = new object();

        public static BakingSheetController Instance
        {
            get
            {
                lock (_lock)
                {
                    if (_instance == null)
                    {
                        _instance = new BakingSheetController();
                        Debug.Log($"<color=red>{_instance.GetType().Name} instance is null!!! Auto create new instance!!!</color>");
                    }
                    return _instance;
                }
            }
        }

        public static bool IsExist => _instance != null;

        /// <summary>
        /// Gets whether the controller is properly initialized and ready to use.
        /// </summary>
        public static bool IsInitialized => _instance != null && !_instance._disposed;
        #endregion

        // Thread-safe collections
        private static readonly ConcurrentDictionary<string, SheetContainerBase> _containerCache = new();
        private static readonly ConcurrentDictionary<string, AsyncOperationHandle> _activeHandles = new();
        private static readonly SemaphoreSlim _loadSemaphore = new(1, 1);
        private bool _disposed = false;

        /// <summary>
        /// Asynchronously loads and bakes a sheet container with cancellation support.
        /// </summary>
        /// <typeparam name="T">The type of the sheet container.</typeparam>
        /// <param name="containerAddress">The addressable address of the container.</param>
        /// <param name="cancellationToken">A token to cancel the operation.</param>
        /// <returns>The loaded and baked sheet container.</returns>
        public async UniTask<T> LoadContainerAsync<T>(string containerAddress, CancellationToken cancellationToken = default) where T : SheetContainerBase
        {
            if (_disposed)
                throw new ObjectDisposedException(nameof(BakingSheetController));

            if (string.IsNullOrEmpty(containerAddress))
                throw new ArgumentException("Container address cannot be null or empty", nameof(containerAddress));

            // Check cache first
            var cacheKey = $"{typeof(T).FullName}_{containerAddress}";
            if (_containerCache.TryGetValue(cacheKey, out var cachedContainer) && cachedContainer is T cached)
            {
                return cached;
            }

            // Use semaphore to prevent concurrent loading of same container
            await _loadSemaphore.WaitAsync(cancellationToken);
            try
            {
                // Double-check after acquiring lock
                if (_containerCache.TryGetValue(cacheKey, out cachedContainer) && cachedContainer is T cached2)
                {
                    return cached2;
                }

                return await LoadContainerInternalAsync<T>(containerAddress, cacheKey, cancellationToken);
            }
            finally
            {
                _loadSemaphore.Release();
            }
        }

        /// <summary>
        /// Convenience method to load container without cancellation token.
        /// </summary>
        /// <typeparam name="T">The type of the sheet container.</typeparam>
        /// <param name="containerAddress">The addressable address of the container.</param>
        /// <returns>The loaded and baked sheet container.</returns>
        public async UniTask<T> LoadContainerAsync<T>(string containerAddress) where T : SheetContainerBase
        {
            return await LoadContainerAsync<T>(containerAddress, CancellationToken.None);
        }

        private async UniTask<T> LoadContainerInternalAsync<T>(string containerAddress, string cacheKey, CancellationToken cancellationToken = default) where T : SheetContainerBase
        {
            AsyncOperationHandle<SheetContainerScriptableObject> handle = default;

            try
            {
                // Load the ScriptableObject
                handle = Addressables.LoadAssetAsync<SheetContainerScriptableObject>(containerAddress);
                _activeHandles[cacheKey] = handle;

                await handle.WithCancellation(cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                {
                    throw new OperationCanceledException("Container loading was cancelled.");
                }

                if (handle.Status != AsyncOperationStatus.Succeeded)
                {
                    var operationException = handle.OperationException ?? new InvalidOperationException($"Failed to load container at address: {containerAddress}. Status: {handle.Status}");
                    Debug.Log($"<color=red>Failed to load container at address: {containerAddress}. Status: {handle.Status}, Exception: {operationException.Message}</color>");
                    throw operationException;
                }

                var containerSO = handle.Result;
                if (containerSO == null)
                {
                    throw new InvalidOperationException($"Loaded ScriptableObject is null for address: {containerAddress}");
                }

                // Create importer and container
                var importer = new ScriptableObjectSheetImporter(containerSO);
                var sheetContainer = ProcessorUtilities.CreateSheetContainer(UnityLogger.Default, typeof(T)) as T;

                if (sheetContainer == null)
                {
                    throw new InvalidCastException($"Failed to create container of type {typeof(T).FullName}");
                }

                // Bake the container
                await sheetContainer.Bake(importer);

                // Cache the result
                _containerCache[cacheKey] = sheetContainer;

                return sheetContainer;
            }
            catch (Exception ex)
            {
                Debug.Log($"<color=red>Error loading container {containerAddress}: {ex}</color>");
                throw;
            }
            finally
            {
                // Clean up handle tracking
                _activeHandles.TryRemove(cacheKey, out _);

                // Release the handle (but keep the loaded asset in cache)
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }
        }

        /// <summary>
        /// Checks if a container is already loaded and cached.
        /// </summary>
        /// <typeparam name="T">The type of the sheet container.</typeparam>
        /// <param name="containerAddress">The addressable address of the container.</param>
        /// <returns>True if the container is loaded, false otherwise.</returns>
        public bool IsContainerLoaded<T>(string containerAddress) where T : SheetContainerBase
        {
            if (_disposed) return false;
            var cacheKey = $"{typeof(T).FullName}_{containerAddress}";
            return _containerCache.ContainsKey(cacheKey);
        }

        /// <summary>
        /// Unloads a specific container from the cache.
        /// </summary>
        /// <typeparam name="T">The type of the sheet container.</typeparam>
        /// <param name="containerAddress">The addressable address of the container.</param>
        public void UnloadContainer<T>(string containerAddress) where T : SheetContainerBase
        {
            if (_disposed) return;
            var cacheKey = $"{typeof(T).FullName}_{containerAddress}";
            if (_containerCache.TryRemove(cacheKey, out var container))
            {
                if (container is IDisposable disposable)
                {
                    disposable.Dispose();
                }
                Debug.Log($"<color=red>Unloaded container: {typeof(T).Name} at {containerAddress}</color>");
            }
        }

        /// <summary>
        /// Unloads all cached containers.
        /// </summary>
        public void UnloadAllContainers()
        {
            if (_disposed) return;

            var count = _containerCache.Count;
            foreach (var kvp in _containerCache)
            {
                if (kvp.Value is IDisposable disposable)
                {
                    disposable.Dispose();
                }
            }
            _containerCache.Clear();
            Debug.Log($"<color=red>Unloaded {count} containers.</color>");
        }

        /// <summary>
        /// Gets the current number of cached containers.
        /// </summary>
        public int CachedContainerCount => _disposed ? 0 : _containerCache.Count;

        /// <summary>
        /// Gets the current number of active loading operations.
        /// </summary>
        public int ActiveLoadingCount => _disposed ? 0 : _activeHandles.Count;

        /// <summary>
        /// Disposes all resources used by the controller.
        /// </summary>
        public void Dispose()
        {
            if (_disposed) return;

            UnloadAllContainers();

            // Cancel any pending operations
            foreach (var handle in _activeHandles.Values)
            {
                if (handle.IsValid())
                {
                    Addressables.Release(handle);
                }
            }
            _activeHandles.Clear();

            _loadSemaphore?.Dispose();
            _disposed = true;

            // Clear the singleton instance
            lock (_lock)
            {
                if (_instance == this)
                {
                    _instance = null;
                }
            }

            Debug.Log("<color=red>BakingSheetController disposed.</color>");
        }

        /// <summary>
        /// Finalizer to ensure resources are cleaned up.
        /// </summary>
        ~BakingSheetController()
        {
            Dispose();
        }
    }
}
