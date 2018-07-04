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
    public static extern void tempFourPointsInpainting(IntPtr outputData, int height, int width, int channels, byte[] inpaintedImageData, byte[] maskData, Point2d[] frame0Points, byte[] currentImageData, Point2d[] current0Points);

    [DllImport("DRDll")]
    public static extern void initFourPointsInpainting(byte[] resultData, byte[] inpaintedData, byte[] maskData, int height, int width, int channels, byte[] frame0ImageData, Point2d[] frame0Points, int method, int parameter);

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
    public byte[] result;
    public byte[] inpainted;
    public byte[] mask;

    public Point2d[] currentPoint2ds = { new Point2d { x = 0, y = 0 }, new Point2d { x = 0, y = 0 },
        new Point2d { x = 0, y = 0 }, new Point2d { x = 0, y = 0 } };
    
    public Point2d[] frame0Point2ds = { new Point2d { x = 0, y = 0 }, new Point2d { x = 0, y = 0 },
        new Point2d { x = 0, y = 0 }, new Point2d { x = 0, y = 0 } };

    private DRUtil() {}

    public static DRUtil Instance{ get { return _instance; } }

    public void SetCurrentRect(double x, double y, double width, double height)
    {
        CameraDevice.VideoModeData vmd;
        vmd = CameraDevice.Instance.GetVideoMode(CameraDevice.CameraDeviceMode.MODE_OPTIMIZE_QUALITY);
        Debug.Log("Dimensions: " + vmd.width + "x" + vmd.height);

        guiRect = new Rect2d
        {
            x = x,
            y = Screen.height - y,
            width = width,
            height = height
        };

        cameraRect = GUIRect2CameraRect(guiRect);

        guiRect = CameraRect2GUIRect(cameraRect);

        image = CameraDevice.Instance.GetCameraImage(CameraInitialisation.pixelFormat);

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

    public void InpaintWithFourPoints(Vector2[] points)
    {
        //CameraDevice.VideoModeData vmd;
        //vmd = CameraDevice.Instance.GetVideoMode(CameraDevice.CameraDeviceMode.MODE_OPTIMIZE_QUALITY);
        //Debug.Log("Dimensions: " + vmd.width + "x" + vmd.height);

        frame0 = CameraDevice.Instance.GetCameraImage(CameraInitialisation.pixelFormat);

        //readImage(frame0.Height, frame0.Width, 1, frame0.Pixels);

        if (frame0 != null)
        {
            byte[] pixels = frame0.Pixels;

            frame0Pixels = new byte[pixels.Length];

            pixels.CopyTo(frame0Pixels, 0);

            Point2d point0 = new Point2d { x = (int)points[0].x, y = Screen.height - (int)points[0].y };
            Point2d point1 = new Point2d { x = (int)points[1].x, y = Screen.height - (int)points[1].y };
            Point2d point2 = new Point2d { x = (int)points[2].x, y = Screen.height - (int)points[2].y };
            Point2d point3 = new Point2d { x = (int)points[3].x, y = Screen.height - (int)points[3].y };

            point0 = GUIPoint2CameraPoint(point0);
            point1 = GUIPoint2CameraPoint(point1);
            point2 = GUIPoint2CameraPoint(point2);
            point3 = GUIPoint2CameraPoint(point3);

            frame0Point2ds[0] = point0;
            frame0Point2ds[1] = point1;
            frame0Point2ds[2] = point2;
            frame0Point2ds[3] = point3;

            result = new byte[pixels.Length];
            inpainted = new byte[pixels.Length];
            mask = new byte[pixels.Length];
            initFourPointsInpainting(result, inpainted, mask, frame0.Height, frame0.Width, CameraInitialisation.channels, frame0Pixels, frame0Point2ds, 3, 16);

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

                Point2d point0 = new Point2d { x = (int)CreateFourPoints.currentPoints[0].x, y = Screen.height - (int)CreateFourPoints.currentPoints[0].y };
                Point2d point1 = new Point2d { x = (int)CreateFourPoints.currentPoints[1].x, y = Screen.height - (int)CreateFourPoints.currentPoints[1].y };
                Point2d point2 = new Point2d { x = (int)CreateFourPoints.currentPoints[2].x, y = Screen.height - (int)CreateFourPoints.currentPoints[2].y };
                Point2d point3 = new Point2d { x = (int)CreateFourPoints.currentPoints[3].x, y = Screen.height - (int)CreateFourPoints.currentPoints[3].y };

                point0 = GUIPoint2CameraPoint(point0);
                point1 = GUIPoint2CameraPoint(point1);
                point2 = GUIPoint2CameraPoint(point2);
                point3 = GUIPoint2CameraPoint(point3);

                currentPoint2ds[0] = point0;
                currentPoint2ds[1] = point1;
                currentPoint2ds[2] = point2;
                currentPoint2ds[3] = point3;

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

    private Point2d GUIPoint2CameraPoint(Point2d myGUIPoint)
    {
        int x;
        int y;
       
        CameraDevice.VideoModeData vmd;
        vmd = CameraDevice.Instance.GetVideoMode(CameraDevice.CameraDeviceMode.MODE_OPTIMIZE_QUALITY);

        if (1.0 * vmd.width / vmd.height < 1.0 * Screen.width / Screen.height)
        {
            //int visibleWidth = vmd.width;
            int visibleHeight = (int)(Screen.height * (vmd.width * 1.0f / Screen.width));

            x = (int)(1.0 * myGUIPoint.x / Screen.width * vmd.width);

            y = (int)(1.0 * myGUIPoint.y / Screen.height * visibleHeight + (vmd.height - visibleHeight) / 2);
        }
        else
        {
            //int visibleHeight = vmd.height;
            int visibleWidth = (int)(Screen.width * (vmd.height * 1.0f / Screen.height));

            x = (int)(1.0 * myGUIPoint.x / Screen.width * visibleWidth + (vmd.width - visibleWidth) / 2);

            y = (int)(1.0 * myGUIPoint.y / Screen.height * vmd.height);
        }

        return new Point2d
        {
            x = x,
            y = y
        };
    }

    private Rect2d GUIRect2CameraRect(Rect2d myGUIRect)
    {
        double x;
        double y;
        double width;
        double height;

        CameraDevice.VideoModeData vmd;
        vmd = CameraDevice.Instance.GetVideoMode(CameraDevice.CameraDeviceMode.MODE_OPTIMIZE_QUALITY);

        if (1.0 * vmd.width / vmd.height < 1.0 * Screen.width / Screen.height)
        {
            //int visibleWidth = vmd.width;
            int visibleHeight = (int)(Screen.height * (vmd.width * 1.0f / Screen.width));

            x = myGUIRect.x / Screen.width * vmd.width;

            y = myGUIRect.y / Screen.height * visibleHeight + (vmd.height - visibleHeight) / 2;

            width = myGUIRect.width / Screen.width * vmd.width;

            height = myGUIRect.height / Screen.height * visibleHeight;
        }
        else
        {
            //int visibleHeight = vmd.height;
            int visibleWidth = (int)(Screen.width * (vmd.height * 1.0f / Screen.height));

            x = myGUIRect.x / Screen.width * visibleWidth + (vmd.width - visibleWidth) / 2;

            y = myGUIRect.y / Screen.height * vmd.height;

            width = myGUIRect.width / Screen.width * visibleWidth;

            height = myGUIRect.height / Screen.height * vmd.height;
        }

        return new Rect2d
        {
            x = x,
            y = y,
            width = width,
            height = height
        };
    }

    private Rect2d CameraRect2GUIRect(Rect2d myCameraRect)
    {
        double x;
        double y;
        double width;
        double height;

        CameraDevice.VideoModeData vmd;
        vmd = CameraDevice.Instance.GetVideoMode(CameraDevice.CameraDeviceMode.MODE_OPTIMIZE_QUALITY);

        if (1.0 * vmd.width / vmd.height < 1.0 * Screen.width / Screen.height)
        {
            //int visibleWidth = vmd.width;
            int visibleHeight = (int)(Screen.height * (vmd.width * 1.0f / Screen.width));

            x = myCameraRect.x / vmd.width * Screen.width;

            y = (myCameraRect.y - (vmd.height - visibleHeight) / 2) / visibleHeight * Screen.height;

            width = myCameraRect.width / vmd.width * Screen.width;

            height = myCameraRect.height / visibleHeight * Screen.height;
        }
        else
        {
            //int visibleHeight = vmd.height;
            int visibleWidth = (int)(Screen.width * (vmd.height * 1.0f / Screen.height));

            x = (myCameraRect.x - (vmd.width - visibleWidth) / 2) / visibleWidth * Screen.width;

            y = myCameraRect.y / vmd.height * Screen.height;

            width = myCameraRect.width / visibleWidth * Screen.width;

            height = myCameraRect.height / vmd.height * Screen.height;
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
