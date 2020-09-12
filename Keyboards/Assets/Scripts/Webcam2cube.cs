using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using OpenCVForUnity.CoreModule;
using OpenCVForUnity.ImgprocModule;
using OpenCVForUnity.UnityUtils;

public class Webcam2cube : MonoBehaviour
{
    WebCamTexture webCamTexture;
    WebCamDevice webCamDevice;
    Mat rgbaMat;
    Mat secondScreen;
    Mat screenIni;
    Mat frameInitial;
    Color32[] colors;
    Color32[] newColors;
    Texture2D texture;
    Texture2D newTexture;
    Keyboard keyboard = new Keyboard(new int[] {0, 40, 60, 30, 190, 190});
    bool isKeyboardFound = false;
    int frameCount = 0;
    public int transparency = 4;
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
        print(keyboard.HueLower);
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
        gameObject.transform.localScale = new Vector3(webCamTexture.width, webCamTexture.height, webCamTexture.height);
        //Debug.Log("Screen.width " + Screen.width + " Screen.height " + Screen.height + " Screen.orientation " + Screen.orientation);
        /*
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
        */
    }

        // Update is called once per frame
        void Update()
    {
        Utils.webCamTextureToMat(webCamTexture, rgbaMat, colors);
        Core.flip(rgbaMat, rgbaMat, -1);
        frameCount++;
        if (frameCount <= 5)
        {
            frameInitial = rgbaMat.clone();
        }
        else
        {
            if (!isKeyboardFound)
            {
                isKeyboardFound = keyboard.GetPosition(rgbaMat, isKeyboardFound);
                Utils.matToTexture2D(rgbaMat, texture, colors);

                print("FINDING KEYBOARD");
                try
                {
                    print("data for keyboard");
                    print(keyboard.Approx);
                    print(keyboard.Contour);
                    print(keyboard.Approx.dump());

                }
                catch { print("no such value"); }
            }
            else
            {
                print("Found Keyboard!");
                secondScreen = new Mat();
                secondScreen = keyboard.PerspectiveTrans(rgbaMat, secondScreen);
                screenIni = new Mat();
                screenIni = keyboard.PerspectiveTrans(frameInitial, screenIni);
                secondScreen = keyboard.TranslucentFingers(secondScreen, screenIni, ref secondScreen, transparency);
                if (newTexture == null) newTexture = new Texture2D(secondScreen.width(), secondScreen.height(), TextureFormat.RGBA32, false);
                if (newColors == null) newColors = new Color32[secondScreen.width() * secondScreen.height()];
                Utils.matToTexture2D(secondScreen, newTexture, newColors);
                gameObject.transform.localScale = new Vector3(newTexture.width, newTexture.height, newTexture.height);
                gameObject.GetComponent<Renderer>().material.mainTexture = newTexture;

            }
        }
    }
}
