using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using Vuforia;

public class DRUtil
{
    [DllImport("DRDll")]
    public static extern int add(int a, int b);

    [DllImport("DRDll")]
    public static extern void drawRect(int height, int width, int channels, byte[] imageData, Rect2d bbox);

    [DllImport("DRDll")]
    public static extern void initTracking(int height, int width, int channels, byte[] imageData, Rect2d bbox, int method);

    [DllImport("DRDll")]
    public static extern Rect2d tracking(int height, int width, int channels, byte[] imageData, Rect2d bbox);

    [DllImport("DRDll")]
    public static extern int readImage(int height, int width, int channels, byte[] imageData);

    [DllImport("DRDll")]
    public static extern void averageInpainting(IntPtr outputData, int height, int width, int channels, byte[] imageData, Rect2d bbox);

    [DllImport("DRDll")]
    public static extern void fourPointsInpainting(IntPtr outputData, int height, int width, int channels, byte[] imageData, Point2d[] points);

    [DllImport("DRDll")]
    public static extern void tempFourPointsInpainting(IntPtr outputData, byte[] currentImageData, Point2d[] currentBoundingPoint2ds, Point2d[] currentControlPoint2ds, bool useIlluminationAdaptation, bool useSurroundingRandomisation);

    [DllImport("DRDll")]
    public static extern void initFourPointsInpainting(byte[] frame0ImageData, Point2d[] frame0BoundingPoint2ds, Point2d[] frame0ControlPoint2ds, int method, int parameter, bool useNormalisation);

    [DllImport("DRDll")]
    public static extern void initParameters(int h, int w, int c, int dh, int dw, int cps, int ibs);

    [StructLayout(LayoutKind.Sequential)]  
    public struct Rect2d 
    {  
        public double x;  
        public double y;
        public double width;
        public double height;
    } 

    [StructLayout(LayoutKind.Sequential)]  
    public struct Point2d 
    {  
        public int x;  
        public int y;
    } 

    private static DRUtil _instance = new DRUtil();
    private bool startTracking = false;

    public Rect2d guiRect;
    public Rect2d cameraRect;

    public IntPtr currentPixels;
    public int currentFrameCount = 0;

    public Vuforia.Image image;
    public Vuforia.Image frame0;

    public byte[] frame0Pixels;

    public bool loseTracked = true;

    private GCHandle inpaintedHandle;

    private int controlPointSize;
    private int illuminationSize;

    public static int inpaintingMethod = 2;
    public static bool useSurroundingRandomisation = true;
    public static bool useIlluminationAdaptation = true;

    public Point2d[] currentBoundingPoint2ds = { 
        new Point2d { x = 0, y = 0 }, new Point2d { x = 0, y = 0 },
        new Point2d { x = 0, y = 0 }, new Point2d { x = 0, y = 0 } };
    
    public Point2d[] frame0BoundingPoint2ds = { 
        new Point2d { x = 0, y = 0 }, new Point2d { x = 0, y = 0 },
        new Point2d { x = 0, y = 0 }, new Point2d { x = 0, y = 0 } };

    public Point2d[] currentControlPoint2ds;

    public Point2d[] frame0ControlPoint2ds;

    private DRUtil() {}

    public static DRUtil Instance{ get { return _instance; } }

    public void SetCurrentRect(double x, double y, double width, double height)
    {
        //Debug.Log("Dimensions: " + CameraInitialisation.width + "x" + CameraInitialisation.height);

        guiRect = new Rect2d
        {
            x = x,
            y = Screen.height - y,
            width = width,
            height = height
        };

        image = CameraDevice.Instance.GetCameraImage(CameraInitialisation.pixelFormat);

        cameraRect = GUIRect2CameraRect(guiRect, image.Width, image.Height);

        guiRect = CameraRect2GUIRect(cameraRect, image.Width, image.Height);

        if (image != null)
        {
            byte[] pixels = image.Pixels;

            initTracking(image.Height, image.Width, CameraInitialisation.channels, pixels, cameraRect, 4);

            VuforiaARController.Instance.RegisterTrackablesUpdatedCallback(OnTrackablesUpdated);

            startTracking = true;

            currentFrameCount = 0;
        }
        else
        {
            Debug.Log("image == null");
        }
    }

    public void InpaintWithFourPoints()
    {
        Debug.Log("Screen Dimensions: " + Screen.width + "x" + Screen.height);

        //Debug.Log("CameraDevice Dimensions: " + CameraInitialisation.width + "x" + CameraInitialisation.height);

        frame0 = CameraDevice.Instance.GetCameraImage(CameraInitialisation.pixelFormat);

        Debug.Log("Frame0 Resolution: " + frame0.Width + "x" + frame0.Height);

        //readImage(frame0.Height, frame0.Width, 1, frame0.Pixels);

        if (frame0 != null)
        {
            controlPointSize = GameObject.Find("Control Points").GetComponent<ControlPointsManager>().controlPointSize;

            illuminationSize = GameObject.Find("Control Points").GetComponent<ControlPointsManager>().illuminationSize;

            initParameters(frame0.Height, frame0.Width, CameraInitialisation.channels, 480, 640, controlPointSize, illuminationSize);

            frame0ControlPoint2ds = new Point2d[controlPointSize];
            currentControlPoint2ds = new Point2d[controlPointSize];

            byte[] pixels = frame0.Pixels;

            frame0Pixels = new byte[pixels.Length];

            pixels.CopyTo(frame0Pixels, 0);

            // bounding points
            Point2d boundingPoint0 = new Point2d { x = (int)BoundingPointsManager.frame0BoundingPointsPos[0].x, y = Screen.height - (int)BoundingPointsManager.frame0BoundingPointsPos[0].y };
            Point2d boundingPoint1 = new Point2d { x = (int)BoundingPointsManager.frame0BoundingPointsPos[1].x, y = Screen.height - (int)BoundingPointsManager.frame0BoundingPointsPos[1].y };
            Point2d boundingPoint2 = new Point2d { x = (int)BoundingPointsManager.frame0BoundingPointsPos[2].x, y = Screen.height - (int)BoundingPointsManager.frame0BoundingPointsPos[2].y };
            Point2d boundingPoint3 = new Point2d { x = (int)BoundingPointsManager.frame0BoundingPointsPos[3].x, y = Screen.height - (int)BoundingPointsManager.frame0BoundingPointsPos[3].y };

            frame0BoundingPoint2ds[0] = GUIPoint2CameraPoint(boundingPoint0, frame0.Width, frame0.Height);
            frame0BoundingPoint2ds[1] = GUIPoint2CameraPoint(boundingPoint1, frame0.Width, frame0.Height);
            frame0BoundingPoint2ds[2] = GUIPoint2CameraPoint(boundingPoint2, frame0.Width, frame0.Height);
            frame0BoundingPoint2ds[3] = GUIPoint2CameraPoint(boundingPoint3, frame0.Width, frame0.Height);

            // control points
            for (int i = 0; i < controlPointSize; i++)
            {
                Point2d controlPoint = new Point2d { x = (int)ControlPointsManager.frame0ControlPointsPos[i].x, y = Screen.height - (int)ControlPointsManager.frame0ControlPointsPos[i].y };
                frame0ControlPoint2ds[i] = GUIPoint2CameraPoint(controlPoint, frame0.Width, frame0.Height);
            }

            //result = new byte[pixels.Length];
            //inpainted = new byte[pixels.Length];
            //mask = new byte[pixels.Length];

            if (inpaintingMethod == 2)
            {
                initFourPointsInpainting(frame0Pixels, frame0BoundingPoint2ds, frame0ControlPoint2ds, 2, 15, false);
            }
            else
            {
                initFourPointsInpainting(frame0Pixels, frame0BoundingPoint2ds, frame0ControlPoint2ds, 3, 16, false);
            }

            //Marshal.Copy(intPtr, inpainted, 0, pixels.Length);

            //Marshal.Release(intPtr);

            //readImage(frame0.Height, frame0.Width, CameraInitialisation.channels, inpainted);
            //readImage(frame0.Height, frame0.Width, CameraInitialisation.channels, mask);

            VuforiaARController.Instance.RegisterTrackablesUpdatedCallback(OnTrackablesUpdated);

            startTracking = true;

            currentFrameCount = 0;

            //fourPointsInpainting(image.Height, image.Width, CameraInitialisation.channels, pixels, point2ds);
        }
        else
        {
            Debug.Log("image == null");
        }
    }

    private void OnTrackablesUpdated()
    {
        if (startTracking)
        {
            image = CameraDevice.Instance.GetCameraImage(CameraInitialisation.pixelFormat);

            if (image != null)
            {
                byte[] pixels = image.Pixels;

                // bounding points
                Point2d boundingPoint0 = new Point2d { x = (int)BoundingPointsManager.currentBoundingPointsPos[0].x, y = Screen.height - (int)BoundingPointsManager.currentBoundingPointsPos[0].y };
                Point2d boundingPoint1 = new Point2d { x = (int)BoundingPointsManager.currentBoundingPointsPos[1].x, y = Screen.height - (int)BoundingPointsManager.currentBoundingPointsPos[1].y };
                Point2d boundingPoint2 = new Point2d { x = (int)BoundingPointsManager.currentBoundingPointsPos[2].x, y = Screen.height - (int)BoundingPointsManager.currentBoundingPointsPos[2].y };
                Point2d boundingPoint3 = new Point2d { x = (int)BoundingPointsManager.currentBoundingPointsPos[3].x, y = Screen.height - (int)BoundingPointsManager.currentBoundingPointsPos[3].y };

                currentBoundingPoint2ds[0] = GUIPoint2CameraPoint(boundingPoint0, image.Width, image.Height);
                currentBoundingPoint2ds[1] = GUIPoint2CameraPoint(boundingPoint1, image.Width, image.Height);
                currentBoundingPoint2ds[2] = GUIPoint2CameraPoint(boundingPoint2, image.Width, image.Height);
                currentBoundingPoint2ds[3] = GUIPoint2CameraPoint(boundingPoint3, image.Width, image.Height);

                // control points
                for (int i = 0; i < controlPointSize; i++)
                {
                    Point2d controlPoint = new Point2d { x = (int)ControlPointsManager.currentControlPointsPos[i].x, y = Screen.height - (int)ControlPointsManager.currentControlPointsPos[i].y };
                    currentControlPoint2ds[i] = GUIPoint2CameraPoint(controlPoint, frame0.Width, frame0.Height);
                }

                //cameraRect = tracking(image.Height, image.Width, CameraInitialisation.channels, pixels, cameraRect);

                //if ((int)cameraRect.x != -1)
                {
                    loseTracked = false;

                    //guiRect = CameraRect2GUIRect(cameraRect);

                    currentFrameCount++;
                }
                //else 
                //{
                //    Debug.Log("Lose track");

                //    startTracking = false;
                //    loseTracked = true;
                //    currentFrameCount = 0;

                //    VuforiaARController.Instance.UnregisterTrackablesUpdatedCallback(OnTrackablesUpdated);
                //}
            }
        }
    }

    private Point2d GUIPoint2CameraPoint(Point2d myGUIPoint, int cameraWidth, int cameraHeight)
    {
        int x;
        int y;

        if (1.0 * cameraWidth / cameraHeight < 1.0 * Screen.width / Screen.height)
        {
            //int visibleWidth = vmd.width;
            int visibleHeight = (int)(Screen.height * (cameraWidth * 1.0f / Screen.width));

            x = (int)(1.0 * myGUIPoint.x / Screen.width * cameraWidth);

            y = (int)(1.0 * myGUIPoint.y / Screen.height * visibleHeight + (cameraHeight - visibleHeight) / 2);
        }
        else
        {
            //int visibleHeight = vmd.height;
            int visibleWidth = (int)(Screen.width * (cameraHeight * 1.0f / Screen.height));

            x = (int)(1.0 * myGUIPoint.x / Screen.width * visibleWidth + (cameraWidth - visibleWidth) / 2);

            y = (int)(1.0 * myGUIPoint.y / Screen.height * cameraHeight);
        }

        return new Point2d
        {
            x = x,
            y = y
        };
    }

    private Rect2d GUIRect2CameraRect(Rect2d myGUIRect, int cameraWidth, int cameraHeight)
    {
        double x;
        double y;
        double width;
        double height;

        if (1.0 * cameraWidth / cameraHeight < 1.0 * Screen.width / Screen.height)
        {
            //int visibleWidth = vmd.width;
            int visibleHeight = (int)(Screen.height * (cameraWidth * 1.0f / Screen.width));

            x = myGUIRect.x / Screen.width * cameraWidth;

            y = myGUIRect.y / Screen.height * visibleHeight + (cameraHeight - visibleHeight) / 2;

            width = myGUIRect.width / Screen.width * cameraWidth;

            height = myGUIRect.height / Screen.height * visibleHeight;
        }
        else
        {
            //int visibleHeight = vmd.height;
            int visibleWidth = (int)(Screen.width * (cameraHeight * 1.0f / Screen.height));

            x = myGUIRect.x / Screen.width * visibleWidth + (cameraWidth - visibleWidth) / 2;

            y = myGUIRect.y / Screen.height * cameraHeight;

            width = myGUIRect.width / Screen.width * visibleWidth;

            height = myGUIRect.height / Screen.height * cameraHeight;
        }

        return new Rect2d
        {
            x = x,
            y = y,
            width = width,
            height = height
        };
    }

    private Rect2d CameraRect2GUIRect(Rect2d myCameraRect, int cameraWidth, int cameraHeight)
    {
        double x;
        double y;
        double width;
        double height;

        if (1.0 * cameraWidth / cameraHeight < 1.0 * Screen.width / Screen.height)
        {
            //int visibleWidth = vmd.width;
            int visibleHeight = (int)(Screen.height * (cameraWidth * 1.0f / Screen.width));

            x = myCameraRect.x / cameraWidth * Screen.width;

            y = (myCameraRect.y - (cameraHeight - visibleHeight) / 2) / visibleHeight * Screen.height;

            width = myCameraRect.width / cameraWidth * Screen.width;

            height = myCameraRect.height / visibleHeight * Screen.height;
        }
        else
        {
            //int visibleHeight = vmd.height;
            int visibleWidth = (int)(Screen.width * (cameraHeight * 1.0f / Screen.height));

            x = (myCameraRect.x - (cameraWidth - visibleWidth) / 2) / visibleWidth * Screen.width;

            y = myCameraRect.y / cameraHeight * Screen.height;

            width = myCameraRect.width / visibleWidth * Screen.width;

            height = myCameraRect.height / cameraHeight * Screen.height;
        }

        return new Rect2d
        {
            x = x,
            y = y,
            width = width,
            height = height
        };
    }
}
