using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;

public class GetWebcam2Cube : MonoBehaviour
{
    public int hl = 0, sl = 37, vl = 60, hu = 30, su = 255, vu = 255;
    WebCamTexture webCamTexture;
    WebCamDevice webCamDevice;
    Mat rgbaMat;
    Color32[] colors;
    Texture2D texture;
    Mat newFrame;
    Mat frameIni;
    int frameCount = 0;
    // Start is called before the first frame update
    void Start()
    {
        var devices = WebCamTexture.devices;
        webCamDevice = devices[0];
        webCamTexture = new WebCamTexture(webCamDevice.name);
        webCamTexture.Play();

        while (true)
        {
            if (webCamTexture.didUpdateThisFrame)
            {
                OnInited();
                break;
            }
        }

        List<Point> points = new List<Point>();
        points.Add(new Point(0, 260));
        points.Add(new Point(880, 260));
        points.Add(new Point(880, 0));
        points.Add(new Point(0, 0));
        MatOfPoint2f pts2 = new MatOfPoint2f();
        pts2.fromList(points);
        double[] p1_1 = pts2.get(0, 0);
        print(string.Join(" ", p1_1));
        double[] p3_1 = pts2.get(2, 0);
        print(string.Join(" ", p3_1));
        double[] p2_1 = pts2.get(1, 0);
        print(string.Join(" ", p2_1));
    }

    private void OnInited()
    {
        if (colors == null || colors.Length != webCamTexture.width * webCamTexture.height)
            colors = new Color32[webCamTexture.width * webCamTexture.height];
        if (texture == null || texture.width != webCamTexture.width || texture.height != webCamTexture.height)
            texture = new Texture2D(webCamTexture.width, webCamTexture.height, TextureFormat.RGBA32, false);

        rgbaMat = new Mat(webCamTexture.height, webCamTexture.width, CvType.CV_8UC4, new Scalar(0, 0, 0, 255));
        Utils.matToTexture2D(rgbaMat, texture, colors);
        

        gameObject.GetComponent<Renderer>().material.mainTexture = texture;

        //gameObject.transform.localScale = new Vector3(webCamTexture.width, webCamTexture.height, 1);
        //Debug.Log("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation);

        float width = rgbaMat.width();
        float height = rgbaMat.height();

        float widthScale = (float)Screen.width / width;
        float heightScale = (float)Screen.height / height;
        if (widthScale < heightScale)
        {
            Camera.main.orthographicSize = (width * (float)Screen.height / (float)Screen.width) / 2;
        }
        else
        {
            Camera.main.orthographicSize = height / 2;
        }
    }
    // Update is called once per frame
    void Update()
    {
        Utils.webCamTextureToMat(webCamTexture, rgbaMat, colors);
        Core.flip(rgbaMat, rgbaMat, -1);
        frameCount++;
        if (frameCount==10)
        {
            frameIni = rgbaMat.clone();
            print("first frame captured.");
        }
        Mat frameProc = new Mat(); //frame.rows(), frame.cols(), CvType.CV_16UC3
        Mat frameMask = new Mat();
        Mat hierarchy = new Mat();
        Imgproc.cvtColor(rgbaMat, frameProc, Imgproc.COLOR_BGR2HSV);
        Scalar lowerB = new Scalar(hl, sl, vl);
        Scalar upperB = new Scalar(hu, su, vu);
        Core.inRange(frameProc, lowerB, upperB, frameMask);

        //Core.bitwise_and(rgbaMat, rgbaMat, frameProc, frameMask);

        //Imgproc.bilateralFilter(frameProc, frameProc, 9, 50, 100);
        //Imgproc.morphologyEx(frameProc, frameProc, 2, Mat.ones(5, 5, CvType.CV_8U));//
        //Imgproc.dilate(frameProc, frameProc, Mat.ones(5, 5, CvType.CV_8U));//Mat.ones(5, 5, CvType.CV_8U), anchor: new Point(-1, -1), iteration:2
        //Imgproc.cvtColor(frameProc, frameProc, Imgproc.COLOR_BGR2GRAY);

        //List<MatOfPoint> contoursList = new List<MatOfPoint>();
        //Imgproc.findContours(frameProc, contoursList, hierarchy, Imgproc.RETR_TREE, Imgproc.CHAIN_APPROX_SIMPLE);
        //print(contoursList);
        //Imgproc.drawContours(rgbaMat, contoursList, -1, new Scalar(100, 24, 56), 3);

        //foreach (MatOfPoint contour in contoursList)
        //{
        //    MatOfPoint2f approx = new MatOfPoint2f();
        //    MatOfPoint2f contourf = new MatOfPoint2f(contour.toArray());
        //    Imgproc.approxPolyDP(contourf, approx, 0.01 * Imgproc.arcLength(contourf, true), true);
        //    print(approx); 
        //}

        //Core.addWeighted(rgbaMat, 0.4, frameIni, 0.6, 0, rgbaMat);

        Utils.matToTexture2D(frameMask, texture, colors);       
        //gameObject.GetComponent<Renderer>().material.mainTexture = texture;
    }
}
