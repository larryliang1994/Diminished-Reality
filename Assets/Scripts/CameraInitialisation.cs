using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class CameraInitialisation : MonoBehaviour 
{
    public static Image.PIXEL_FORMAT pixelFormat = Image.PIXEL_FORMAT.UNKNOWN_FORMAT;
    public static int channels = 1;

    PositionalDeviceTracker positionalDeviceTracker;

    //public static int width;
    //public static int height;

    void Start()
    {
#if UNITY_EDITOR
        pixelFormat = Image.PIXEL_FORMAT.GRAYSCALE; // Need Grayscale for Editor
        channels = 1;
#else
        pixelFormat = Image.PIXEL_FORMAT.RGB888; // Use RGB888 for mobile
        channels = 3;
#endif

        // Register Vuforia life-cycle callbacks:
        VuforiaARController.Instance.RegisterVuforiaStartedCallback(OnVuforiaStarted);
        VuforiaARController.Instance.RegisterOnPauseCallback(OnPaused);
    }

    void OnVuforiaStarted()
    {
        // Try register camera image format
        if (CameraDevice.Instance.SetFrameFormat(pixelFormat, true))
        {
            Debug.Log("Successfully registered pixel format " + pixelFormat.ToString());
        }
        else
        {
            Debug.LogError(
                "\nFailed to register pixel format: " + pixelFormat.ToString() +
                "\nThe format may be unsupported by your device." +
                "\nConsider using a different pixel format.\n");
        }

        CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);

        CameraDevice.VideoModeData vmd;
        vmd = CameraDevice.Instance.GetVideoMode(CameraDevice.CameraDeviceMode.MODE_OPTIMIZE_SPEED);

        //Screen.SetResolution(vmd.width, vmd.height, Screen.fullScreen);

        //width = 640;
        //height = 480;

        //Debug.Log("CameraDevice Dimensions: " + CameraInitialisation.width + "x" + CameraInitialisation.height);

        positionalDeviceTracker = TrackerManager.Instance.GetTracker<PositionalDeviceTracker>();

        if (positionalDeviceTracker == null)
        {
            Debug.Log("Ground Plane is not supported.");
        }
        else
        {
            Debug.Log("Ground Plane starts successfully.");
        }
    }

    private void OnPaused(bool paused)
    {
        if (!paused) // resumed
        {
            // Set again autofocus mode when app is resumed
            CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
        }
    }

    public void SetInpaintingMethod(int method)
    {
        DRUtil.inpaintingMethod = method;

        Debug.Log("Using Inpainting Method: " + DRUtil.inpaintingMethod);
    }

    public void SetResolution(int which)
    {
        if (which == 0)
        {
            DRUtil.inpaintingWidth = 1280;
            DRUtil.inpaintingHeight = 720;
            DRUtil.inpaintingPatchSize = 21;
        }
        else if (which == 1)
        {
            DRUtil.inpaintingWidth = 640;
            DRUtil.inpaintingHeight = 480;
            DRUtil.inpaintingPatchSize = 11;
        }

        Debug.Log("Using Resolution: " + DRUtil.inpaintingWidth + "x" + DRUtil.inpaintingHeight);
    }

    public void SetSurroundingRandomisation(bool on)
    {
        DRUtil.useSurroundingRandomisation = on;

        Debug.Log("Using Surrounding Randomisation: " + DRUtil.useSurroundingRandomisation);
    }

    public void SetIlluminationAdaptation(bool on)
    {
        DRUtil.useIlluminationAdaptation = on;

        Debug.Log("Using Illumination Adaptation: " + DRUtil.useIlluminationAdaptation);
    }

    //public void SetSurroundingRandomisationOff()
    //{
    //    DRUtil.useSurroundingRandomisation = false;
    //}
}
