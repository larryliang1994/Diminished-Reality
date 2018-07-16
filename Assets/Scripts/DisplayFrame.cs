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

    private int currentFrameCount = 0;

    int m_frameCounter = 0;
    float m_timeCounter = 0.0f;
    float m_lastFramerate = 0.0f;
    public float m_refreshTime = 0.5f;

    private float algorithmFrameRate = 0.0f;

    System.Diagnostics.Stopwatch timer;

    // Use this for initialization
    void Start()
    {
        SetTargetFrameRate();
    }
	
	// Update is called once per frame
	void Update () 
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

            if (timer != null)
            {
                algorithmFrameRate = (float)(1000.0 / timer.Elapsed.TotalMilliseconds);
            }
        }

        if (!DRUtil.Instance.loseTracked && currentFrameCount < DRUtil.Instance.currentFrameCount)
        {
            if (background == null || texture == null || pixels == null)
            {
                background = transform.GetChild(0).gameObject;
                texture = background.GetComponent<Renderer>().material.mainTexture as Texture2D;

                copyTexture = duplicateTexture(texture);

                pixels = copyTexture.GetPixels32();

                pixelsHandle.Free();
                pixelsHandle = GCHandle.Alloc(pixels, GCHandleType.Pinned);

                background.GetComponent<Renderer>().material.mainTexture = copyTexture;

                Debug.Log("Copy now.");
            }

            timer = new System.Diagnostics.Stopwatch();
            timer.Start();
            DRUtil.tempFourPointsInpainting(pixelsHandle.AddrOfPinnedObject(), DRUtil.Instance.image.Pixels, DRUtil.Instance.currentBoundingPoint2ds, DRUtil.Instance.currentControlPoint2ds, DRUtil.useIlluminationAdaptation, DRUtil.useSurroundingRandomisation);
            timer.Stop();

            //Debug.Log("Time Taken: " + timer.Elapsed.TotalMilliseconds);

            copyTexture.SetPixels32(pixels);
            copyTexture.Apply();

            //pixelsHandle.Free();

            currentFrameCount++;
        }
        else if (DRUtil.Instance.loseTracked)
        {
            currentFrameCount = 0;

            background = null;
            texture = null;
            pixels = null;
            pixelsHandle.Free();
        }
	}

    Texture2D duplicateTexture(Texture2D source)
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

    public void TempFourPointsInpainting()
    {
        //DRUtil.readImage(DRUtil.Instance.frame0.Height, DRUtil.Instance.frame0.Width, 1, DRUtil.Instance.frame0Pixels);

        //DRUtil.tempFourPointsInpainting(pixelsHandle.AddrOfPinnedObject(), texture.height, texture.width, CameraInitialisation.channels,
                                        //DRUtil.Instance.frame0Pixels, DRUtil.Instance.frame0Point2ds, DRUtil.Instance.image.Pixels, DRUtil.Instance.currentPoint2ds);
    }

    private void OnGUI()
    {
        //fpsText.text = "FPS:" + DRUtil.add(8, 9).ToString();
        fpsText.text = "FPS:" + ((int)m_lastFramerate).ToString() + "/" + ((int)algorithmFrameRate).ToString();

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
            targetFPS = VuforiaRenderer.Instance.GetRecommendedFps(VuforiaRenderer.FpsHint.NONE);
        }
        // Note: if you use vsync in your quality settings, you should also set
        // your QualitySettings.vSyncCount according to the value returned above.
        // e.g. if targetFPS > 50 --> vSyncCount = 1; else vSyncCount = 2;
        //targetFPS = 100;
        Debug.Log("targetFPS = " + targetFPS);
        Application.targetFrameRate = targetFPS;
    }
}
