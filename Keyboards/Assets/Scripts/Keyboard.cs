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
    int min_area = 150000;

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
    public Mat Approx;
    private int[] TrackWindow;
    public Mat Contour;
    #endregion


    #region Methods
    public Keyboard() { }
    public Keyboard(int[] hsvBoundaryX)
    {
        hsvBoundary = hsvBoundaryX;
        HueLower = hsvBoundaryX[0];
        SatLower = hsvBoundaryX[1];
        ValLower = hsvBoundaryX[2];
        HueUpper = hsvBoundaryX[3];
        SatUpper = hsvBoundaryX[4];
        ValUpper = hsvBoundaryX[5];
    }
    public bool GetPosition(Mat frame, bool isKeyboardFound)
    {
        Mat frameProc = new Mat(); //frame.rows(), frame.cols(), CvType.CV_16UC3
        Mat frameMask = new Mat();
        Mat hierarchy = new Mat();
        Imgproc.cvtColor(frame, frameProc, Imgproc.COLOR_BGR2HSV);
        Scalar lowerB = new Scalar(HueLower, SatLower, ValLower);
        Scalar upperB = new Scalar(HueUpper, SatUpper, ValUpper);
        Core.inRange(frameProc, lowerB, upperB, frameMask);
        Core.bitwise_and(frame, frame, frameProc, frameMask);
        //Imgproc.bilateralFilter(frameProc, frameProc, 9, 50, 100);
        Imgproc.morphologyEx(frameProc, frameProc, 2, Mat.ones(5, 5, CvType.CV_8U));//
        Imgproc.dilate(frameProc, frameProc, Mat.ones(5, 5, CvType.CV_8U));//Mat.ones(5, 5, CvType.CV_8U), anchor: new Point(-1, -1), iteration:2
        Imgproc.cvtColor(frameProc, frameProc, Imgproc.COLOR_BGR2GRAY);

        List<MatOfPoint> contoursList = new List<MatOfPoint>();
        Imgproc.findContours(frameProc, contoursList, hierarchy, Imgproc.RETR_TREE, Imgproc.CHAIN_APPROX_SIMPLE);

        int count = 0;
        foreach (MatOfPoint contour in contoursList)
        {
            MatOfPoint2f approx = new MatOfPoint2f();
            MatOfPoint2f contourf = new MatOfPoint2f(contour.toArray());
            Imgproc.approxPolyDP(contourf, approx, 0.01 * Imgproc.arcLength(contourf, true), true);
            //print(approx.dump());
                if (approx.rows() == 4 && Imgproc.contourArea(contour) >= min_area)
                {
                    count++;
                    if (count >= 2) continue;
                    else
                    {
                        OpenCVForUnity.CoreModule.Rect track_win = Imgproc.boundingRect(approx);
                        TrackWindow = new int[] { track_win.x, track_win.y, track_win.width, track_win.height };
                        if (frame.height() - 5 < TrackWindow[0] + TrackWindow[2]
                            && TrackWindow[0] + TrackWindow[2] <= frame.height()
                            || 0 <= TrackWindow[0] && TrackWindow[0] < 5
                            || frame.width() - 5 < TrackWindow[1] + TrackWindow[3]
                            && TrackWindow[1] + TrackWindow[3] <= frame.width()
                            || 0 <= TrackWindow[1] && TrackWindow[1] < 5) continue;
                        else
                        {
                            Approx = approx;
                            Contour = contour;
                            return isKeyboardFound = true;
                        }
                    }
                }
        }
        return isKeyboardFound = false;
    }

    public Mat PerspectiveTrans(Mat frame, Mat dst)
    {
        
        MatOfPoint2f pts1 = new MatOfPoint2f(Approx);
        print("unsorted: " + pts1.dump());
        sort(ref pts1);
        print("sorted: " + pts1.dump());
        List<Point> points = new List<Point>();
        points.Add(new Point(0, 0));
        points.Add(new Point(880, 0));
        points.Add(new Point(880, 260));
        points.Add(new Point(0, 260));
        MatOfPoint2f pts2 = new MatOfPoint2f();
        pts2.fromList(points);
        print("frameStructure: " + pts2.dump());
        Mat matrix = Imgproc.getPerspectiveTransform(pts1, pts2);
        Imgproc.warpPerspective(frame, dst, matrix, new Size(880, 260), Imgproc.INTER_LINEAR);
        return dst;
    }

    public Mat TranslucentFingers(Mat frame, Mat frame1, ref Mat dst, int transparency = 4)
    {
        //Mat hsv = new Mat();
        //Mat mask = new Mat();
        //Mat cleared = new Mat();
        //Imgproc.cvtColor(frame, hsv, Imgproc.COLOR_BGR2HSV);
        //Scalar lowerB = new Scalar(HueLower, SatLower, ValLower);
        //Scalar upperB = new Scalar(HueUpper, SatUpper, ValUpper);
        //Core.inRange(hsv, lowerB, upperB, mask);
        //Core.bitwise_and(frame, frame, cleared, mask);
        double a = transparency == 5 ? 0.8 : transparency == 4 ? 0.7 : transparency == 3 ? 0.6 : transparency == 2 ? 0.5 : transparency == 1 ? 0.4 : transparency == 0 ? 0.3 : 0.2;
        double b = transparency == 5 ? 0.2 : transparency == 4 ? 0.3 : transparency == 3 ? 0.4 : transparency == 2 ? 0.5 : transparency == 1 ? 0.6 : transparency == 0 ? 0.7 : 0.8;
        //Mat hands = new Mat();
        //Core.bitwise_and(frame, cleared, hands);
        Core.addWeighted(frame1, a, frame, b, 0, dst);
        return dst;
    }

    private void sort(ref MatOfPoint2f fourPoints)
    {
        // the argument needs to contain 4 points precisely
        // sort the bounding box to (topleft, topright, bottomright, bottomleft
        double[] p1, p2, p3, p4;
        List<Point> points = new List<Point>();
        p1 = new double[2] { fourPoints.get(0, 0)[0], fourPoints.get(0, 0)[1] };
        p2 = new double[2] { fourPoints.get(1, 0)[0], fourPoints.get(1, 0)[1] };
        p3 = new double[2] { fourPoints.get(2, 0)[0], fourPoints.get(2, 0)[1] };
        p4 = new double[2] { fourPoints.get(3, 0)[0], fourPoints.get(3, 0)[1] };
        print("p1: " + (char)p1[0] + "," + (char)p1[1]);
        print("p2: " + (char)p2[0] + "," + (char)p2[1]);
        print("p3: " + (char)p3[0] + "," + (char)p3[1]);
        print("p4: " + (char)p4[0] + "," + (char)p4[1]);

        if (p1[0] < p2[0] && p1[0] < p3[0] || p1[0] < p4[0] && p1[0] < p3[0] || p1[0] < p2[0] && p1[0] < p4[0])
        {
            if (p1[1] < p2[1] && p1[1] < p3[1] || p1[1] < p4[1] && p1[1] < p3[1] || p1[1] < p2[1] && p1[1] < p4[1])
            {
                points.Add(new Point(p1[0], p1[1]));
                if (p2[1] < p3[1] && p2[1] < p4[1])
                {
                    points.Add(new Point(p2[0], p2[1]));
                    if (p3[0] < p4[0])
                    {
                        points.Add(new Point(p4[0], p4[1]));
                        points.Add(new Point(p3[0], p3[1]));
                    }
                    else
                    {
                        points.Add(new Point(p3[0], p3[1]));
                        points.Add(new Point(p4[0], p4[1]));
                    }
                }
                else if (p3[1] < p2[1] && p3[1] < p4[1])
                {
                    points.Add(new Point(p3[0], p3[1]));
                    if (p2[0] < p4[0])
                    {
                        points.Add(new Point(p4[0], p4[1]));
                        points.Add(new Point(p2[0], p2[1]));
                    }
                    else
                    {
                        points.Add(new Point(p2[0], p2[1]));
                        points.Add(new Point(p4[0], p4[1]));
                    }
                }
                else if (p4[1] < p2[1] && p4[1] < p3[1])
                {
                    points.Add(new Point(p4[0], p4[1]));
                    if (p2[0] < p3[0])
                    {
                        points.Add(new Point(p3[0], p3[1]));
                        points.Add(new Point(p2[0], p2[1]));
                    }
                    else
                    {
                        points.Add(new Point(p2[0], p2[1]));
                        points.Add(new Point(p3[0], p3[1]));
                    }
                }

            }
            else if (p2[0] < p3[0] && p2[0] < p4[0])
            {
                points.Add(new Point(p2[0], p2[1]));
                if (p3[1] < p4[1])
                {
                    points.Add(new Point(p3[0], p3[1]));
                    points.Add(new Point(p4[0], p4[1]));
                }
                else
                {
                    points.Add(new Point(p4[0], p4[1]));
                    points.Add(new Point(p3[0], p3[1]));
                }
                points.Add(new Point(p1[0], p1[1]));
            }
            else if (p3[0] < p2[0] && p3[0] < p4[0])
            {
                points.Add(new Point(p3[0], p3[1]));
                if (p2[1] < p4[1])
                {
                    points.Add(new Point(p2[0], p2[1]));
                    points.Add(new Point(p4[0], p4[1]));
                }
                else
                {
                    points.Add(new Point(p4[0], p4[1]));
                    points.Add(new Point(p2[0], p2[1]));
                }
                points.Add(new Point(p1[0], p1[1]));
            }
            else if (p4[0] < p3[0] && p4[0] < p2[0])
            {
                points.Add(new Point(p4[0], p4[1]));
                if (p3[1] < p2[1])
                {
                    points.Add(new Point(p3[0], p3[1]));
                    points.Add(new Point(p2[0], p2[1]));
                }
                else
                {
                    points.Add(new Point(p2[0], p2[1]));
                    points.Add(new Point(p3[0], p3[1]));
                }
                points.Add(new Point(p1[0], p1[1]));
            }
        }
        else if (p2[0] < p1[0] && p2[0] < p3[0] || p2[0] < p4[0] && p2[0] < p3[0] || p2[0] < p1[0] && p2[0] < p4[0])
        {
            if (p2[1] < p1[1] && p2[1] < p3[1] || p2[1] < p4[1] && p2[1] < p3[1] || p2[1] < p1[1] && p2[1] < p4[1])
            {
                points.Add(new Point(p2[0], p2[1]));
                if (p1[1] < p3[1] && p1[1] < p4[1])
                {
                    points.Add(new Point(p1[0], p1[1]));
                    if (p3[0] < p4[0])
                    {
                        points.Add(new Point(p4[0], p4[1]));
                        points.Add(new Point(p3[0], p3[1]));
                    }
                    else
                    {
                        points.Add(new Point(p3[0], p3[1]));
                        points.Add(new Point(p4[0], p4[1]));
                    }
                }
                else if (p3[1] < p1[1] && p3[1] < p4[1])
                {
                    points.Add(new Point(p3[0], p3[1]));
                    if (p1[0] < p4[0])
                    {
                        points.Add(new Point(p4[0], p4[1]));
                        points.Add(new Point(p1[0], p1[1]));
                    }
                    else
                    {
                        points.Add(new Point(p1[0], p1[1]));
                        points.Add(new Point(p4[0], p4[1]));
                    }
                }
                else if (p4[1] < p1[1] && p4[1] < p3[1])
                {
                    points.Add(new Point(p4[0], p4[1]));
                    if (p1[0] < p3[0])
                    {
                        points.Add(new Point(p3[0], p3[1]));
                        points.Add(new Point(p1[0], p1[1]));
                    }
                    else
                    {
                        points.Add(new Point(p1[0], p1[1]));
                        points.Add(new Point(p3[0], p3[1]));
                    }
                }

            }
            else if (p1[0] < p3[0] && p1[0] < p4[0])
            {
                points.Add(new Point(p1[0], p1[1]));
                if (p3[1] < p4[1])
                {
                    points.Add(new Point(p3[0], p3[1]));
                    points.Add(new Point(p4[0], p4[1]));
                }
                else
                {
                    points.Add(new Point(p4[0], p4[1]));
                    points.Add(new Point(p3[0], p3[1]));
                }
                points.Add(new Point(p2[0], p2[1]));
            }
            else if (p3[0] < p1[0] && p3[0] < p4[0])
            {
                points.Add(new Point(p3[0], p3[1]));
                if (p1[1] < p4[1])
                {
                    points.Add(new Point(p1[0], p1[1]));
                    points.Add(new Point(p4[0], p4[1]));
                }
                else
                {
                    points.Add(new Point(p4[0], p4[1]));
                    points.Add(new Point(p1[0], p1[1]));
                }
                points.Add(new Point(p2[0], p2[1]));
            }
            else if (p4[0] < p3[0] && p4[0] < p1[0])
            {
                points.Add(new Point(p4[0], p4[1]));
                if (p3[1] < p1[1])
                {
                    points.Add(new Point(p3[0], p3[1]));
                    points.Add(new Point(p1[0], p1[1]));
                }
                else
                {
                    points.Add(new Point(p1[0], p1[1]));
                    points.Add(new Point(p3[0], p3[1]));
                }
                points.Add(new Point(p2[0], p2[1]));
            }
        }
        else if (p3[0] < p2[0] && p3[0] < p1[0] || p3[0] < p4[0] && p3[0] < p1[0] || p3[0] < p2[0] && p3[0] < p4[0])
        {
            if (p3[1] < p2[1] && p3[1] < p1[1] || p3[1] < p4[1] && p3[1] < p1[1] || p3[1] < p2[1] && p3[1] < p4[1])
            {
                points.Add(new Point(p3[0], p3[1]));
                if (p2[1] < p1[1] && p2[1] < p4[1])
                {
                    points.Add(new Point(p2[0], p2[1]));
                    if (p1[0] < p4[0])
                    {
                        points.Add(new Point(p4[0], p4[1]));
                        points.Add(new Point(p1[0], p1[1]));
                    }
                    else
                    {
                        points.Add(new Point(p1[0], p1[1]));
                        points.Add(new Point(p4[0], p4[1]));
                    }
                }
                else if (p1[1] < p2[1] && p1[1] < p4[1])
                {
                    points.Add(new Point(p1[0], p1[1]));
                    if (p2[0] < p4[0])
                    {
                        points.Add(new Point(p4[0], p4[1]));
                        points.Add(new Point(p2[0], p2[1]));
                    }
                    else
                    {
                        points.Add(new Point(p2[0], p2[1]));
                        points.Add(new Point(p4[0], p4[1]));
                    }
                }
                else if (p4[1] < p2[1] && p4[1] < p1[1])
                {
                    points.Add(new Point(p4[0], p4[1]));
                    if (p2[0] < p1[0])
                    {
                        points.Add(new Point(p1[0], p1[1]));
                        points.Add(new Point(p2[0], p2[1]));
                    }
                    else
                    {
                        points.Add(new Point(p2[0], p2[1]));
                        points.Add(new Point(p1[0], p1[1]));
                    }
                }

            }
            else if (p2[0] < p1[0] && p2[0] < p4[0])
            {
                points.Add(new Point(p2[0], p2[1]));
                if (p1[1] < p4[1])
                {
                    points.Add(new Point(p1[0], p1[1]));
                    points.Add(new Point(p4[0], p4[1]));
                }
                else
                {
                    points.Add(new Point(p4[0], p4[1]));
                    points.Add(new Point(p1[0], p1[1]));
                }
                points.Add(new Point(p3[0], p3[1]));
            }
            else if (p1[0] < p2[0] && p1[0] < p4[0])
            {
                points.Add(new Point(p1[0], p1[1]));
                if (p2[1] < p4[1])
                {
                    points.Add(new Point(p2[0], p2[1]));
                    points.Add(new Point(p4[0], p4[1]));
                }
                else
                {
                    points.Add(new Point(p4[0], p4[1]));
                    points.Add(new Point(p2[0], p2[1]));
                }
                points.Add(new Point(p3[0], p3[1]));
            }
            else if (p4[0] < p1[0] && p4[0] < p2[0])
            {
                points.Add(new Point(p4[0], p4[1]));
                if (p1[1] < p2[1])
                {
                    points.Add(new Point(p1[0], p1[1]));
                    points.Add(new Point(p2[0], p2[1]));
                }
                else
                {
                    points.Add(new Point(p2[0], p2[1]));
                    points.Add(new Point(p1[0], p1[1]));
                }
                points.Add(new Point(p3[0], p3[1]));
            }
        }
        else if (p4[0] < p2[0] && p4[0] < p3[0] || p4[0] < p1[0] && p4[0] < p3[0] || p4[0] < p2[0] && p4[0] < p1[0])
        {
            if (p4[1] < p2[1] && p4[1] < p3[1] || p4[1] < p1[1] && p4[1] < p3[1] || p4[1] < p2[1] && p4[1] < p1[1])
            {
                points.Add(new Point(p4[0], p4[1]));
                if (p2[1] < p3[1] && p2[1] < p1[1])
                {
                    points.Add(new Point(p2[0], p2[1]));
                    if (p3[0] < p1[0])
                    {
                        points.Add(new Point(p1[0], p1[1]));
                        points.Add(new Point(p3[0], p3[1]));
                    }
                    else
                    {
                        points.Add(new Point(p3[0], p3[1]));
                        points.Add(new Point(p1[0], p1[1]));
                    }
                }
                else if (p3[1] < p2[1] && p3[1] < p1[1])
                {
                    points.Add(new Point(p3[0], p3[1]));
                    if (p2[0] < p1[0])
                    {
                        points.Add(new Point(p1[0], p1[1]));
                        points.Add(new Point(p2[0], p2[1]));
                    }
                    else
                    {
                        points.Add(new Point(p2[0], p2[1]));
                        points.Add(new Point(p1[0], p1[1]));
                    }
                }
                else if (p1[1] < p2[1] && p1[1] < p3[1])
                {
                    points.Add(new Point(p1[0], p1[1]));
                    if (p2[0] < p3[0])
                    {
                        points.Add(new Point(p3[0], p3[1]));
                        points.Add(new Point(p2[0], p2[1]));
                    }
                    else
                    {
                        points.Add(new Point(p2[0], p2[1]));
                        points.Add(new Point(p3[0], p3[1]));
                    }
                }

            }
            else if (p2[0] < p3[0] && p2[0] < p1[0])
            {
                points.Add(new Point(p2[0], p2[1]));
                if (p3[1] < p1[1])
                {
                    points.Add(new Point(p3[0], p3[1]));
                    points.Add(new Point(p1[0], p1[1]));
                }
                else
                {
                    points.Add(new Point(p1[0], p1[1]));
                    points.Add(new Point(p3[0], p3[1]));
                }
                points.Add(new Point(p4[0], p4[1]));
            }
            else if (p3[0] < p2[0] && p3[0] < p1[0])
            {
                points.Add(new Point(p3[0], p3[1]));
                if (p2[1] < p1[1])
                {
                    points.Add(new Point(p2[0], p2[1]));
                    points.Add(new Point(p1[0], p1[1]));
                }
                else
                {
                    points.Add(new Point(p1[0], p1[1]));
                    points.Add(new Point(p2[0], p2[1]));
                }
                points.Add(new Point(p4[0], p4[1]));
            }
            else if (p1[0] < p3[0] && p1[0] < p2[0])
            {
                points.Add(new Point(p1[0], p1[1]));
                if (p3[1] < p2[1])
                {
                    points.Add(new Point(p3[0], p3[1]));
                    points.Add(new Point(p2[0], p2[1]));
                }
                else
                {
                    points.Add(new Point(p2[0], p2[1]));
                    points.Add(new Point(p3[0], p3[1]));
                }
                points.Add(new Point(p4[0], p4[1]));
            }
        }
        // MatOfPoint2f pts2 = new MatOfPoint2f();
        fourPoints = new MatOfPoint2f();
        fourPoints.release();
        fourPoints.fromList(points);
    }
    #endregion
}
