using System;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Linq;
using UnityEngine;

namespace Abiogenesis3d
{
    [Serializable]
    public class MultiCameraEventsCameraInfo {
        public Camera cam;
        [HideInInspector]
        public GameObject raycastBlocker;
        public LayerMask layers = 1 << 0;
        [Tooltip("0 = cam.farClipPlane")]
        public float raycastDistance = 200;
    }

    [ExecuteInEditMode]
    public class MultiCameraEvents : MonoBehaviour
    {
        public static string raycastBlockerName = "RaycastBlocker";

        public bool blockedByUI = true;

        [Header("To ignore a camera add MultiCameraEventsIgnore component to it.")]
        public MultiCameraEventsCameraInfo[] cameraInfos;

        public GameObject raycastBlockerPrefab;

        GameObject lastColliderGO;
        GameObject lastMouseDownColliderGO;
        SendMessageOptions msgOpts = SendMessageOptions.DontRequireReceiver;
        Ray ray;

        public RaycastHit raycastHit;

        Vector3 lastMousePosition;

        float lastHandleInits;
        float handleInitsEvery = 0.1f;

        void OnValidate()
        {
            lastHandleInits = 0;
        }

        void CheckForInstances()
        {
            var existingInstances = FindObjectsOfType<MultiCameraEvents>();
            if (existingInstances.Length > 1)
            {
                Debug.Log($"MultiCameraEvents: There should only be one active instance in the scene. Deactivating: {name}");
                enabled = false;
                return;
            }
        }

        void OnEnable()
        {
            CheckForInstances();
        }

        void Start()
        {
            lastMousePosition = Input.mousePosition;
        }

        void HandleInits()
        {
            // TODO: randomize this to not create processing spikes
            if (Time.time - lastHandleInits > handleInitsEvery)
            {
                lastHandleInits = Time.time;

                AutoDetectCameras();
                cameraInfos = cameraInfos.Where(c => c.cam).OrderBy(c => c.cam.depth).ToArray();
            }
        }

        private void CreateRaycastBlocker(MultiCameraEventsCameraInfo camInfo)
        {
            GameObject raycastBlocker = Instantiate(raycastBlockerPrefab);
            raycastBlocker.transform.SetParent(camInfo.cam.transform);
            raycastBlocker.name = MultiCameraEvents.raycastBlockerName;
            HandleResize(raycastBlocker, camInfo.cam);
            camInfo.raycastBlocker = raycastBlocker;
        }

        void HandleResize(GameObject plane, Camera cam)
        {
            float planeWidth;
            float planeHeight;

            if (cam.orthographic)
            {
                planeHeight = cam.orthographicSize * 2f;
                planeWidth = planeHeight * cam.aspect;
            }
            else
            {
                planeHeight = 2f * Mathf.Tan(cam.fieldOfView * 0.5f * Mathf.Deg2Rad) * cam.nearClipPlane;
                planeWidth = planeHeight * cam.aspect;
            }

            plane.transform.localPosition = new Vector3(0, 0, cam.nearClipPlane + 0.01f);
            plane.transform.rotation = cam.transform.rotation;
            plane.transform.localScale = new Vector3(planeWidth, planeHeight, 1);
        }

        Type GetIgnoredType()
        {
            return typeof(MultiCameraEventsIgnore);
        }

        void AutoDetectCameras()
        {
            var allCameras = FindObjectsOfType<Camera>();

            foreach(var cam in allCameras)
            {
                var ignoreTag = cam.GetComponent(GetIgnoredType());
                var camInfo = cameraInfos.FirstOrDefault(c => c.cam == cam);

                if (camInfo == null)
                {
                    if (ignoreTag == null)
                    {
                        camInfo = new MultiCameraEventsCameraInfo {cam = cam};
                        cameraInfos = cameraInfos.Concat(new[] {camInfo}).ToArray();
                    }
                }
                else
                {
                    if (ignoreTag != null)
                        cameraInfos = cameraInfos.Where(c => c.cam != cam).ToArray();
                }
            }
        }

        bool IsCamInfoDisabled(MultiCameraEventsCameraInfo camInfo)
        {
            return camInfo.cam == null || !camInfo.cam.gameObject.activeInHierarchy;
        }

        void Update()
        {
            HandleInits();
            if (!Application.isPlaying) return;

            foreach (var camInfo in cameraInfos)
                HandleRaycastBlocker(camInfo);

            SynthesizeEvents();
        }

        void HandleRaycastBlocker(MultiCameraEventsCameraInfo camInfo)
        {
            if (IsCamInfoDisabled(camInfo))
            {
                if (camInfo.raycastBlocker != null)
                    DestroyImmediate(camInfo.raycastBlocker);
                return;
            }

            if (camInfo.raycastBlocker == null)
                CreateRaycastBlocker(camInfo);

            HandleResize(camInfo.raycastBlocker, camInfo.cam);
        }

        bool IsPointerOverUIObject()
        {
            PointerEventData eventDataCurrentPosition = new PointerEventData(EventSystem.current);
            eventDataCurrentPosition.position = new Vector2(Input.mousePosition.x, Input.mousePosition.y);

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventDataCurrentPosition, results);

            results.RemoveAll(r => r.gameObject.GetComponent(GetIgnoredType()) != null);

            return results.Count > 0;
        }

        void SynthesizeEvents()
        {
            if (blockedByUI)
            {
                // var pointerOverGO = EventSystem.current?.IsPointerOverGameObject() ?? false;
                var pointerOverGO = IsPointerOverUIObject();
                if (pointerOverGO)
                {
                    if (lastColliderGO != null)
                        lastColliderGO.SendMessageUpwards("OnMouseExit", msgOpts);

                    lastColliderGO = null;
                    raycastHit = default;

                    return;
                }
            }

            raycastHit = default;
            // reverse cameras order, last camera is first to hit
            foreach (var camInfo in cameraInfos.Reverse())
            {
                if (IsCamInfoDisabled(camInfo)) continue;

                bool didHit = false;
                ray = camInfo.cam.ScreenPointToRay(Input.mousePosition);
                var raycastDistance = camInfo.raycastDistance != 0 ? camInfo.raycastDistance : camInfo.cam.farClipPlane;
                var hits = Physics.RaycastAll(ray, camInfo.raycastDistance, camInfo.layers).OrderBy(h => h.distance).ToArray();
                foreach (var hit in hits)
                {
                    if (hit.collider.name.StartsWith(MultiCameraEvents.raycastBlockerName)) continue;
                    raycastHit = hit;

                    didHit = true;

                    // changing to a new target
                    if (hit.collider.gameObject != lastColliderGO)
                    {
                        // exiting previous target
                        if (lastColliderGO != null)
                            lastColliderGO.SendMessageUpwards("OnMouseExit", msgOpts);

                        // entering new target
                        hit.collider.SendMessageUpwards("OnMouseEnter", msgOpts);
                        lastColliderGO = hit.collider.gameObject;
                    }
                    // staying on the same target
                    else hit.collider.SendMessageUpwards("OnMouseOver", msgOpts);

                    // clicks
                    for (var i = 0; i < 3; i++) {
                        if (Input.GetMouseButtonDown(i))
                        {
                            lastMouseDownColliderGO = hit.collider.gameObject;
                            hit.collider.SendMessageUpwards("OnMouseDown", msgOpts);
                        }
                        if (Input.GetMouseButtonUp(i))
                        {
                            if (lastMouseDownColliderGO == hit.collider.gameObject)
                                hit.collider.SendMessageUpwards("OnMouseUpAsButton", msgOpts);
                            hit.collider.SendMessageUpwards("OnMouseUp", msgOpts);
                        }
                    }

                    // move
                    var mouseDelta = Input.mousePosition - lastMousePosition;
                    if (mouseDelta != Vector3.zero) hit.collider.SendMessageUpwards("OnMouseMove", msgOpts);
                    lastMousePosition = Input.mousePosition;

                    if (didHit) break;
                }
                if (didHit) break;
            }
        }
    }
}
