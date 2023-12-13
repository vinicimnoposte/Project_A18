using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Abiogenesis3d {
    [ExecuteInEditMode]
    public class MirrorOnRenderImage : MonoBehaviour {
        public delegate void RenderImageCallback(RenderTexture src, RenderTexture dest);
        public RenderImageCallback renderImageCallback;

        [Header("Debug")]
        public string target;
        public GameObject targetGO;
        // public bool logInvocationItems;
        // public string logInvocationItemsStr;

        void OnRenderImage(RenderTexture src, RenderTexture dest) {
            if (renderImageCallback?.GetInvocationList().Length > 0) {
                renderImageCallback.Invoke(src, dest);

                // logInvocationItemsStr = "";
                // // Log invocation list item callback namespace and name
                // foreach (var item in renderImageCallback.GetInvocationList())
                //     logInvocationItemsStr += item.Method.DeclaringType.Namespace + "." + item.Method.DeclaringType.Name + "." + item.Method.Name + ",\n";

                // if (logInvocationItems)
                // {
                //     Debug.Log("MirrorOnRenderImage (" + name + "):" + logInvocationItemsStr);
                //     logInvocationItems = false;
                // }
            }
            else {
                Graphics.SetRenderTarget(dest);
                Graphics.Blit(src, dest);
            }
        }
    }
}
