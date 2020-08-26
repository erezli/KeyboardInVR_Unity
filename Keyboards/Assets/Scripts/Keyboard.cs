using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;

public class Keyboard : MonoBehaviour
{
    #region Field
    [SerializeField]
    int max_area = 15000, min_area = 5000;

    private int[] hsvBoundary;
    #endregion

    #region Properties
    //public int HueLower { get { return hsvBoundary[0]; } set { hsvBoundary[0] = value; } }
    //public int SatLower { get { return hsvBoundary[1]; } set { hsvBoundary[1] = value; } }
    //public int ValLower { get { return hsvBoundary[2]; } set { hsvBoundary[2] = value; } }
    //public int HueUpper { get { return hsvBoundary[3]; } set { hsvBoundary[3] = value; } }
    //public int SatUpper { get { return hsvBoundary[4]; } set { hsvBoundary[4] = value; } }
    //public int ValUpper { get { return hsvBoundary[5]; } set { hsvBoundary[5] = value; } }
    public int HueLower;
    public int SatLower;
    public int ValLower;
    public int HueUpper;
    public int SatUpper;
    public int ValUpper;
    public int[,] Approx; 
    private int[] TrackWindow;
    #endregion


    #region Methods
    public Keyboard() {}
    public Keyboard(int[] hsvBoundaryX)
    {
        hsvBoundary = hsvBoundaryX;
    }
    public void GetPosition(Mat frame)
    {
        Mat frameHSV = new Mat(frame.rows(), frame.cols(), CvType.CV_16UC3);
        Mat frameMask = new Mat(frame.rows(), frame.cols(), CvType.CV_16UC3);
        Mat frameClr = new Mat(frame.rows(), frame.cols(), CvType.CV_16UC3);
        Mat frameBlur = new Mat(frame.rows(), frame.cols(), CvType.CV_16UC3);
        Mat frameMorph = new Mat(frame.rows(), frame.cols(), CvType.CV_16UC3);
        Mat frameGrey = new Mat(frame.rows(), frame.cols(), CvType.CV_16UC3);
        Imgproc.cvtColor(frame, frameHSV, Imgproc.COLOR_BGR2HSV);
        Scalar lowerB = new Scalar(HueLower, SatLower, ValLower);
        Scalar upperB = new Scalar(HueUpper, SatUpper, ValUpper);
        Core.inRange(frameHSV, lowerB, upperB, frameMask);
        Core.bitwise_and(frame, frame, frameClr, frameMask);
        Imgproc.bilateralFilter(frameClr, frameBlur, 9, 50, 100);
        Imgproc.morphologyEx(frameBlur, frameMorph, 2, Mat.ones(5, 5, CvType.CV_8U));
        Imgproc.dilate(frameMorph, frameMorph, Mat.ones(5, 5, CvType.CV_8U), new Point(-1, -1), 2);
        Imgproc.cvtColor(frameMorph, frameGrey, Imgproc.COLOR_BGR2GRAY);

        List<MatOfPoint> contoursList = new List<MatOfPoint>();
        Imgproc.findContours(frameGrey, contoursList, null, Imgproc.RETR_TREE, Imgproc.CHAIN_APPROX_SIMPLE);

        

    }
    #endregion





    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
