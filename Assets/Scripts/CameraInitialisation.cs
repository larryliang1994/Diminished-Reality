using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Vuforia;

public class CameraInitialisation : MonoBehaviour {
    
    public static Image.PIXEL_FORMAT pixelFormat = Image.PIXEL_FORMAT.UNKNOWN_FORMAT;
    public static int channels = 1;

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
    }

    private void OnPaused(bool paused)
    {
        if (!paused) // resumed
        {
            // Set again autofocus mode when app is resumed
            CameraDevice.Instance.SetFocusMode(CameraDevice.FocusMode.FOCUS_MODE_CONTINUOUSAUTO);
        }
    }
}
