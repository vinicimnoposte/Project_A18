using System;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

namespace Abiogenesis3d
{
    [ExecuteInEditMode]
    public class UPixelatorRenderHandler : MonoBehaviour
    {
        [HideInInspector] public UPixelator uPixelator;
        // NOTE: these are set by UPixelator after attaching this script
        public UPixelatorCameraInfo camInfo;
        [HideInInspector] public MirrorOnRenderImage mirrorOnRenderImage;
        [HideInInspector] public UPixelatorSnappable camSnappable;

        Quaternion storedCamRotation;

        float origOrthoSize;

        public void LateUpdate()
        {
            if (camInfo.cam == null) return;

            #if UNITY_PIPELINE_URP
            // NOTE: this will cleanup unused builtin-only components
            if (mirrorOnRenderImage) DestroyImmediate(mirrorOnRenderImage);
            #else
            EnsureMirrorOnRenderImage();
            #endif
            EnsureCamSnappable();

            // NOTE: this ensures OnEndCameraRendering is called after other callbacks
            // otherwise the camera snapped position might be reset back too early

            #if UNITY_PIPELINE_URP
            RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
            RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
            #else

            Camera.onPostRender -= PostRender;
            Camera.onPostRender += PostRender;

            mirrorOnRenderImage.renderImageCallback -= RenderImage;
            mirrorOnRenderImage.renderImageCallback += RenderImage;
            #endif
        }

        float GetSnapSize()
        {
            return uPixelator.pixelMultiplier * (2 * camInfo.cam.orthographicSize) / uPixelator.screenSize.y;
        }

        #if UNITY_PIPELINE_URP
        #else
        void EnsureMirrorOnRenderImage()
        {
            if (mirrorOnRenderImage) return;

            var mirrorOnRenderImages = camInfo.cam.GetComponents<MirrorOnRenderImage>();
            foreach (var mirror in mirrorOnRenderImages)
            {
                // NOTE: cleanup previous leftover components
                if (mirror.target == "UPixelator") DestroyImmediate(mirror);
            }

            mirrorOnRenderImage = camInfo.cam.gameObject.AddComponent<MirrorOnRenderImage>();
            mirrorOnRenderImage.target = "UPixelator";
            mirrorOnRenderImage.targetGO = gameObject;
        }
        #endif

        void EnsureCamSnappable()
        {
            if (camSnappable) return;
            camSnappable = camInfo.cam.GetComponent<UPixelatorSnappable>();
            if (!camSnappable) camSnappable = camInfo.cam.gameObject.AddComponent<UPixelatorSnappable>();
        }

        void HandleSnap()
        {
            // TODO: how to reproduce the issue that this causes?
            // NOTE: when cam.targetTexture is set cam.pixelHeight is wrong
            // if (camInfo.cam.targetTexture != null) return;

            if (ShouldSnap())
            {
                // NOTE: first store all or transform is changed by afterwards snapped parent
                foreach (UPixelatorSnappable snappable in uPixelator.snappables)
                {
                    if (snappable == null) continue;
                    if (!snappable.isActiveAndEnabled) continue;

                    if (snappable.snapPosition) snappable.StorePosition();
                    if (snappable.snapRotation) snappable.StoreRotation();
                    if (snappable.snapLocalScale) snappable.StoreLocalScale();
                }

                var isCamRotationDirty = false;
                if (camSnappable != null)
                {
                    if (camInfo.cam.transform.rotation != storedCamRotation)
                    {
                        isCamRotationDirty = true;
                        storedCamRotation = camInfo.cam.transform.rotation;
                    }
                }

                foreach (UPixelatorSnappable snappable in uPixelator.snappables)
                {
                    if (snappable == null) continue;
                    if (!snappable.isActiveAndEnabled) continue;
                    if (snappable == camSnappable)
                    {
                        UpdateRenderQuadPosition(HandleCamSnap());
                        continue;
                    }

                    if (isCamRotationDirty) snappable.initialPosition = snappable.transform.position;
                    if (snappable.snapPosition) snappable.SnapPosition(camInfo.cam.transform.rotation, GetSnapSize());
                    if (snappable.snapRotation && !snappable.isLocalRotation) snappable.SnapRotation(snappable.snapRotationAngles);
                    if (snappable.snapRotation && snappable.isLocalRotation) snappable.SnapLocalRotation(snappable.snapRotationAngles);
                    if (snappable.snapRotation) snappable.SnapRotation(snappable.snapRotationAngles);
                    if (snappable.snapLocalScale) snappable.SnapLocalScale(snappable.snapScaleValue);
                }
            }
        }

        public bool ShouldSnap()
        {
            return camInfo.snap && camInfo.cam.orthographic;
        }

        public void UpdateRenderQuadPosition()
        {
            if (!ShouldSnap()) return;
            if (camSnappable == null) return;
            if (!camSnappable.isActiveAndEnabled) return;
            if (!camSnappable.snapPosition) return;

            camSnappable.StorePosition();
            UpdateRenderQuadPosition(HandleCamSnap());
            camSnappable.RestorePosition();
        }

        Vector3 HandleCamSnap()
        {
            var repeatSize = camInfo.stabilize ? uPixelator.ditherRepeatSize : 1;
            float camSnapSize = repeatSize * GetSnapSize();
            return camSnappable.SnapPosition(camInfo.cam.transform.rotation, camSnapSize);
        }

        void UpdateRenderQuadPosition(Vector3 camSnapDiff)
        {
            if (!camInfo.stabilize) return;

            // if (camSnapDiff == default) return;
            Vector3 localPosition = -camSnapDiff / camInfo.cam.orthographicSize;
            // NOTE: keep z, it is handled by the UPixelator based on depth
            localPosition.z = camInfo.renderQuad.localPosition.z;
            camInfo.renderQuad.localPosition = localPosition;

            // if (camInfo.stabilize && camInfo.cam.orthographic) {}
            // else renderQuad.localPosition = Vector3.zero;
        }

        void HandleUnsnap()
        {
            if (!ShouldSnap()) return;

            foreach (UPixelatorSnappable snappable in uPixelator.snappables)
            {
                if (snappable == null) continue;
                if (!snappable.isActiveAndEnabled) continue;

                if (snappable.snapPosition) snappable.RestorePosition();
                if (snappable.snapRotation) snappable.RestoreRotation();
                if (snappable.snapLocalScale) snappable.RestoreLocalScale();
            }
        }

        void OnEnable()
        {
            // NOTE: it is important that OnBeginCameraRendering is called before any other
            // tool that needs the camera because this is where Snappables snap is applied
            // if this is not first then other tools will get the wrong camera position

        #if UNITY_PIPELINE_URP
            Utils.AddCallbackToStart<Action<ScriptableRenderContext, Camera>>(typeof(RenderPipelineManager), "beginCameraRendering", new Action<ScriptableRenderContext, Camera>(OnBeginCameraRendering));
            RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
            RenderPipelineManager.endCameraRendering += OnEndCameraRendering;
        #else
            Utils.AddCallbackToStart<Camera.CameraCallback>(typeof(Camera), "onPreRender", new Camera.CameraCallback(PreRender));
            Camera.onPostRender -= PostRender;
            Camera.onPostRender += PostRender;
        #endif
        }

        void OnDisable()
        {
            if (camInfo?.cam != null) camInfo.cam.targetTexture = null;

        #if UNITY_PIPELINE_URP
            RenderPipelineManager.beginCameraRendering -= OnBeginCameraRendering;
            RenderPipelineManager.endCameraRendering -= OnEndCameraRendering;
        #else
            Camera.onPreRender -= PreRender;
            Camera.onPostRender -= PostRender;

            if (mirrorOnRenderImage)
            {
                mirrorOnRenderImage.renderImageCallback -= RenderImage;
                DestroyImmediate(mirrorOnRenderImage);
            }
        #endif
            camSnappable = null;
        }

    #if UNITY_PIPELINE_URP
        void OnBeginCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            PreRender(camera);
        }

        void OnEndCameraRendering(ScriptableRenderContext context, Camera camera)
        {
            PostRender(camera);
        }
    #else
        void RenderImage(RenderTexture src, RenderTexture dest)
        {
            Graphics.Blit(src, camInfo.renderTexture);
            // NOTE: this suppresses a warning that dest was not written into
            RenderTexture.active = dest;
        }
    #endif

        float GetVerticalOrthoSizeCorrection(float origSize)
        {
            float correction = origSize / Screen.height;

            float extraVerticalPixels = uPixelator.GetRenderTexturePadding().y * uPixelator.pixelMultiplier;
            // NOTE: ensures that Unity does not permanently break the camera component...
            if (extraVerticalPixels / 2 > Screen.height) extraVerticalPixels = Screen.height;

            return origSize + (extraVerticalPixels * correction);
        }

        void PreRender(Camera camera)
        {
            if (camera != camInfo.cam) return;

            HandleSnap();

        #if UNITY_PIPELINE_URP
        #else
            Rect pixelRect = camInfo.cam.pixelRect;
        #endif

            if (camInfo.renderTexture.width < 1 || camInfo.renderTexture.height < 1)
            {
                Debug.LogError("RenderTexture's width and height must be greater than 0");
                return;
            }

            origOrthoSize = camInfo.cam.orthographicSize;
            float newOrthoSize = GetVerticalOrthoSizeCorrection(origOrthoSize);
            // NOTE: ensures that Unity does not permanently break the camera component...
            if (newOrthoSize < 0.5) newOrthoSize = 0.5f;
            camInfo.cam.orthographicSize = newOrthoSize;

            camInfo.cam.targetTexture = camInfo.renderTexture;

        #if UNITY_PIPELINE_URP
        #else
            camInfo.cam.pixelRect = pixelRect;
        #endif
        }

        void PostRender(Camera camera)
        {
            if (camera != camInfo.cam) return;

            HandleUnsnap();

            // NOTE: this is needed or else the Screen size is set to renderTexture size
            //  and events like Input.mousePosition that return pixel values are wrong
            camInfo.cam.targetTexture = null;

            camInfo.cam.orthographicSize = origOrthoSize;
        }
    }
}
