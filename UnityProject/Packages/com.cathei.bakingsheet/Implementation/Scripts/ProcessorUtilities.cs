using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Cathei.BakingSheet;
using Cathei.BakingSheet.Unity;
using UnityEngine;

namespace ThanhDV.Cathei.BakingSheet.Implementation
{
    public static class ProcessorUtilities
    {
        public static SheetContainerBase CreateSheetContainer(UnityLogger logger, Type containerType)
        {
            if (containerType == null)
            {
                Debug.LogError("[BakingSheet] containerType can not be null!!!");
                return null;
            }

            try
            {
                return Activator.CreateInstance(containerType, logger) as SheetContainerBase;
            }
            catch (Exception e)
            {
                Debug.LogError($"[BakingSheet] Failed to instantiate {containerType.Name}. Make sure it has a constructor that accepts an ILogger. Error: {e.Message}");
                return null;
            }
        }

        public static List<Type> FindSheetContainerType()
        {
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();

            IEnumerable<Assembly> userAssemblies = assemblies.Where(a => !a.FullName.StartsWith("Unity"));

            List<Type> containerTypes = userAssemblies.SelectMany(assembly =>
            {
                try
                {
                    return assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException)
                {
                    return Type.EmptyTypes;
                }
            }).Where(type => type != null && typeof(SheetContainerBase).IsAssignableFrom(type) && !type.IsAbstract && !type.IsInterface).ToList();

            if (containerTypes.Count == 0)
            {
                Debug.LogError("[BakingSheet] No class inheriting from 'SheetContainerBase' found in user assemblies.");
                return null;
            }

            return containerTypes;
        }
    }
}
