using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using Vuforia;

public class DisplayFrame : MonoBehaviour 
{
    public Text fpsText;
    public Text foundText;

    private GameObject background;
    private Texture2D texture;
    private Color32[] pixels;
    private GCHandle pixelsHandle;

    private Texture2D copyTexture;

    int m_frameCounter = 0;
    float m_timeCounter = 0.0f;
    float m_lastFramerate = 0.0f;
    public float m_refreshTime = 0.5f;

    private float algorithmFrameRate = 0.0f;
    private float algorithmShowFrameRate = 0.0f;

    private bool isInpainting = false;

    private ControlPointsManager controlPointsManager;
    private BoundingPointsManager boundingPointsManager;

    System.Diagnostics.Stopwatch timer;

    void Start()
    {
        SetTargetFrameRate();

        timer = new System.Diagnostics.Stopwatch();
    }
	
	void Update () 
    {
        if (!isInpainting)
        {
            if (m_timeCounter < m_refreshTime)
            {
                m_timeCounter += Time.deltaTime;
                m_frameCounter++;
            }
            else
            {
                m_lastFramerate = (float)m_frameCounter / m_timeCounter;
                m_frameCounter = 0;
                m_timeCounter = 0.0f;

                algorithmShowFrameRate = algorithmFrameRate;
            }
        }
	}

    public void StartInpainting()
    {
        controlPointsManager = GameObject.Find("Control Points").GetComponent<ControlPointsManager>();
        boundingPointsManager = GameObject.Find("Bounding Points").GetComponent<BoundingPointsManager>();

        VuforiaARController.Instance.RegisterTrackablesUpdatedCallback(OnTrackablesUpdated);
    }

    private void OnTrackablesUpdated()
    {
        Vuforia.Image image = CameraDevice.Instance.GetCameraImage(CameraInitialisation.pixelFormat);

        if (image != null)
        {
            isInpainting = true;

            if (m_timeCounter < m_refreshTime)
            {
                m_timeCounter += Time.deltaTime;
                m_frameCounter++;
            }
            else
            {
                m_lastFramerate = (float)m_frameCounter / m_timeCounter;
                m_frameCounter = 0;
                m_timeCounter = 0.0f;

                algorithmShowFrameRate = algorithmFrameRate;
            }

            // bounding points
            Vector2[] currentBoundingPointsPos = boundingPointsManager.GetCurrentBoundingPointsPos();

            DRUtil.Point2d boundingPoint0 = new DRUtil.Point2d { x = (int)currentBoundingPointsPos[0].x, y = Screen.height - (int)currentBoundingPointsPos[0].y };
            DRUtil.Point2d boundingPoint1 = new DRUtil.Point2d { x = (int)currentBoundingPointsPos[1].x, y = Screen.height - (int)currentBoundingPointsPos[1].y };
            DRUtil.Point2d boundingPoint2 = new DRUtil.Point2d { x = (int)currentBoundingPointsPos[2].x, y = Screen.height - (int)currentBoundingPointsPos[2].y };
            DRUtil.Point2d boundingPoint3 = new DRUtil.Point2d { x = (int)currentBoundingPointsPos[3].x, y = Screen.height - (int)currentBoundingPointsPos[3].y };

            DRUtil.Point2d[] currentBoundingPoint2ds = new DRUtil.Point2d[4];

            currentBoundingPoint2ds[0] = DRUtil.GUIPoint2CameraPoint(boundingPoint0, image.Width, image.Height);
            currentBoundingPoint2ds[1] = DRUtil.GUIPoint2CameraPoint(boundingPoint1, image.Width, image.Height);
            currentBoundingPoint2ds[2] = DRUtil.GUIPoint2CameraPoint(boundingPoint2, image.Width, image.Height);
            currentBoundingPoint2ds[3] = DRUtil.GUIPoint2CameraPoint(boundingPoint3, image.Width, image.Height);

            // control points
            Vector2[] currentControlPointsPos = controlPointsManager.GetCurrentControlPointsPos();

            DRUtil.Point2d[] currentControlPoint2ds = new DRUtil.Point2d[controlPointsManager.controlPointSize];;
            for (int i = 0; i < controlPointsManager.controlPointSize; i++)
            {
                DRUtil.Point2d controlPoint = new DRUtil.Point2d { x = (int)currentControlPointsPos[i].x, y = Screen.height - (int)currentControlPointsPos[i].y };
                currentControlPoint2ds[i] = DRUtil.GUIPoint2CameraPoint(controlPoint, image.Width, image.Height);
            }

            if (background == null || texture == null || pixels == null)
            {
                background = transform.GetChild(0).gameObject;
                texture = background.GetComponent<Renderer>().material.mainTexture as Texture2D;

                copyTexture = DuplicateTexture(texture);

                pixels = copyTexture.GetPixels32();

                if (pixelsHandle.IsAllocated)
                {
                    pixelsHandle.Free();
                }
                pixelsHandle = GCHandle.Alloc(pixels, GCHandleType.Pinned);

                background.GetComponent<Renderer>().material.mainTexture = copyTexture;

                Debug.Log("Copy now.");
            }

            timer.Start();
            DRUtil.currentFrameInpainting(
                pixelsHandle.AddrOfPinnedObject(), image.Pixels,
                currentBoundingPoint2ds, currentControlPoint2ds,
                DRUtil.useIlluminationAdaptation, DRUtil.useSurroundingRandomisation);
            timer.Stop();

            algorithmFrameRate = (float)timer.Elapsed.TotalMilliseconds;

            timer.Reset();

            copyTexture.SetPixels32(pixels);
            copyTexture.Apply();
        }
        else
        {
            Debug.Log("image == null");

            VuforiaARController.Instance.UnregisterTrackablesUpdatedCallback(OnTrackablesUpdated);

            isInpainting = false;

            background = null;
            texture = null;
            pixels = null;

            if (pixelsHandle.IsAllocated)
            {
                pixelsHandle.Free();
            }
        }
    }

    Texture2D DuplicateTexture(Texture2D source)
    {
        RenderTexture renderTex = RenderTexture.GetTemporary(
                    source.width,
                    source.height,
                    0,
                    RenderTextureFormat.Default,
                    RenderTextureReadWrite.Linear);

        Graphics.Blit(source, renderTex);
        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = renderTex;
        Texture2D readableText = new Texture2D(source.width, source.height);
        readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
        readableText.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(renderTex);
        return readableText;
    }

    private void OnGUI()
    {
        //float msec = deltaTime * 1000.0f;
        //float fps = 1.0f / deltaTime;

        //fpsText.text = "FPS:" + DRUtil.add(8, 9).ToString();
        fpsText.text = "FPS:" + ((int)m_lastFramerate).ToString() + "/ Time:" + ((int)algorithmShowFrameRate).ToString();

        if (WorldTrackableEventHandler.found) 
        {
            foundText.text = "Found Marker";
        }
        else
        {
            foundText.text = "Marker Not Found";
        }
    }

    private static Texture2D _staticRectTexture;
    private static GUIStyle _staticRectStyle;

    // Note that this function is only meant to be called from OnGUI() functions.
    public static void GUIDrawRect(Rect position, Color color)
    {
        if (_staticRectTexture == null)
        {
            _staticRectTexture = new Texture2D(1, 1);
        }

        if (_staticRectStyle == null)
        {
            _staticRectStyle = new GUIStyle();
        }

        _staticRectTexture.SetPixel(0, 0, color);
        _staticRectTexture.Apply();

        _staticRectStyle.normal.background = _staticRectTexture;

        GUI.Box(position, GUIContent.none, _staticRectStyle);
    }

    private void SetTargetFrameRate()
    {
        int targetFPS = 0;
        EyewearDevice eyewearDevice = Device.Instance as EyewearDevice;
        if ((eyewearDevice != null) && eyewearDevice.IsSeeThru())
        {
            // In see-through devices, there is no video background to render
            targetFPS = VuforiaRenderer.Instance.GetRecommendedFps(VuforiaRenderer.FpsHint.NO_VIDEOBACKGROUND);
        }
        else
        {
            // Query Vuforia for AR target frame rate and set it in Unity:
            targetFPS = VuforiaRenderer.Instance.GetRecommendedFps(VuforiaRenderer.FpsHint.FAST);
        }
        // Note: if you use vsync in your quality settings, you should also set
        // your QualitySettings.vSyncCount according to the value returned above.
        // e.g. if targetFPS > 50 --> vSyncCount = 1; else vSyncCount = 2;
        //targetFPS = 100;
        Debug.Log("targetFPS = " + targetFPS);
        Application.targetFrameRate = targetFPS;
    }
}
