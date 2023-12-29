#nullable enable

using UnityEngine;

namespace MocopiSender
{
    static class ObjectFinder
    {
        internal static Transform? FindObjectByName(Transform parentTransform, string objectName)
        {
            if(parentTransform.name == objectName)
            {
                return parentTransform;
            }

            var foundTransform = parentTransform.Find(objectName);

            if (foundTransform != null)
            {
                return foundTransform;
            }

            for (int i = 0; i < parentTransform.childCount; i++)
            {
                var foundInChildren = FindObjectByName(parentTransform.GetChild(i), objectName);
                if (foundInChildren != null)
                {
                    return foundInChildren;
                }
            }

            return null;
        }        
    }
}