using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using UnityEngine;
using Vuforia;

public class DRUtil
{
    [DllImport("DRDll")]
    public static extern void currentFrameInpainting(IntPtr outputData, byte[] currentImageData, Point2d[] currentBoundingPoint2ds, Point2d[] currentControlPoint2ds, bool useIlluminationAdaptation, bool useSurroundingRandomisation);

    [DllImport("DRDll")]
    public static extern void initInpainting(byte[] frame0ImageData, Point2d[] frame0BoundingPoint2ds, Point2d[] frame0ControlPoint2ds, int method, int parameter, bool useNormalisation);

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

    public Rect2d guiRect;
    public Rect2d cameraRect;

    public Vuforia.Image frame0;
    public byte[] frame0Pixels;

    public bool loseTracked = true;

    private GCHandle inpaintedHandle;

    private int controlPointSize;
    private int illuminationSize;

    public static int inpaintingMethod = 2;
    public static int inpaintingWidth = 1280;
    public static int inpaintingHeight = 720;
    public static int inpaintingPatchSize = 21;
    public static bool useSurroundingRandomisation = true;
    public static bool useIlluminationAdaptation = true;

    public Point2d[] frame0BoundingPoint2ds = new Point2d[4];

    public static string progressString;

    public Point2d[] frame0ControlPoint2ds;

    private ControlPointsManager controlPointsManager;
    private BoundingPointsManager boundingPointsManager;
    private ProgressBarManager progressBarManager;

    private DRUtil() {}

    public static DRUtil Instance{ get { return _instance; } }

    public void InpaintWithFourPoints(Vector2[] frame0BoundingPointsPos)
    {
        Debug.Log("Screen Dimensions: " + Screen.width + "x" + Screen.height);

        frame0 = CameraDevice.Instance.GetCameraImage(CameraInitialisation.pixelFormat);

        Debug.Log("Frame0 Resolution: " + frame0.Width + "x" + frame0.Height);

        if (frame0 != null)
        {
            controlPointsManager = GameObject.Find("Control Points").GetComponent<ControlPointsManager>();
            boundingPointsManager = GameObject.Find("Bounding Points").GetComponent<BoundingPointsManager>();
            progressBarManager = GameObject.Find("Background").GetComponent<ProgressBarManager>();

            progressBarManager.SetProgressVisible(true);
            progressString = "Raycasting Points...";

            controlPointSize = controlPointsManager.controlPointSize;

            illuminationSize = controlPointsManager.illuminationSize;

            initParameters(frame0.Height, frame0.Width, CameraInitialisation.channels, inpaintingHeight, inpaintingWidth, controlPointSize, illuminationSize);

            frame0ControlPoint2ds = new Point2d[controlPointSize];

            byte[] pixels = frame0.Pixels;

            frame0Pixels = new byte[pixels.Length];

            pixels.CopyTo(frame0Pixels, 0);

            // bounding points
            Point2d boundingPoint0 = new Point2d { x = (int)frame0BoundingPointsPos[0].x, y = Screen.height - (int)frame0BoundingPointsPos[0].y };
            Point2d boundingPoint1 = new Point2d { x = (int)frame0BoundingPointsPos[1].x, y = Screen.height - (int)frame0BoundingPointsPos[1].y };
            Point2d boundingPoint2 = new Point2d { x = (int)frame0BoundingPointsPos[2].x, y = Screen.height - (int)frame0BoundingPointsPos[2].y };
            Point2d boundingPoint3 = new Point2d { x = (int)frame0BoundingPointsPos[3].x, y = Screen.height - (int)frame0BoundingPointsPos[3].y };

            frame0BoundingPoint2ds[0] = GUIPoint2CameraPoint(boundingPoint0, frame0.Width, frame0.Height);
            frame0BoundingPoint2ds[1] = GUIPoint2CameraPoint(boundingPoint1, frame0.Width, frame0.Height);
            frame0BoundingPoint2ds[2] = GUIPoint2CameraPoint(boundingPoint2, frame0.Width, frame0.Height);
            frame0BoundingPoint2ds[3] = GUIPoint2CameraPoint(boundingPoint3, frame0.Width, frame0.Height);

            Vector2[] frame0ControlPointsPos = controlPointsManager.GetFrame0ControlPointsPos();

            // control points
            for (int i = 0; i < controlPointSize; i++)
            {
                Point2d controlPoint = new Point2d { x = (int)frame0ControlPointsPos[i].x, y = Screen.height - (int)frame0ControlPointsPos[i].y };
                frame0ControlPoint2ds[i] = GUIPoint2CameraPoint(controlPoint, frame0.Width, frame0.Height);
            }

            //InitInpainting();
            new Thread(InitInpainting).Start();
            new Thread(Progress).Start();
        }
        else
        {
            Debug.Log("image == null");
        }
    }

    private void InitInpainting()
    {
        if (inpaintingMethod == 2)
        {
            initInpainting(frame0Pixels, frame0BoundingPoint2ds, frame0ControlPoint2ds, 2, inpaintingPatchSize, false);
        }
        else
        {
            initInpainting(frame0Pixels, frame0BoundingPoint2ds, frame0ControlPoint2ds, 3, 16, false);
        }

        Debug.Log("Done InitInpainting");

        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            progressBarManager.SetProgressVisible(false);
            GameObject.Find("ARCamera").GetComponent<DisplayFrame>().StartInpainting();
        });
    }

    private void Progress()
    {
        progressString = "Creating Mask...";
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            progressBarManager.SetProgress(0.0f, 0.15f);
        });

        Thread.Sleep(3000);

        progressString = "Inpainting...";
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            progressBarManager.SetProgress(0.15f, 0.30f);
        });

        Thread.Sleep(3000);

        progressString = "Init Surrounding Randomisation...";
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            progressBarManager.SetProgress(0.30f, 0.50f);
        });

        Thread.Sleep(3000);

        progressString = "Init Illumination Adaptation...";
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            progressBarManager.SetProgress(0.50f, 0.75f);
        });

        Thread.Sleep(3000);

        progressString = "Finishing...";
        UnityMainThreadDispatcher.Instance().Enqueue(() =>
        {
            progressBarManager.SetProgress(0.75f, 1.0f);
        });
    }

    public static Point2d GUIPoint2CameraPoint(Point2d myGUIPoint, int cameraWidth, int cameraHeight)
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

    public static Rect2d GUIRect2CameraRect(Rect2d myGUIRect, int cameraWidth, int cameraHeight)
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

    public static Rect2d CameraRect2GUIRect(Rect2d myCameraRect, int cameraWidth, int cameraHeight)
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
