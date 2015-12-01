#define ENABLE_CACHE

using UnityEngine;
using System.Collections;
using System.Collections.Generic;

using System;
using System.IO; // For File Class
using System.Threading;
using System.Text;

using LitJson; // http://lbv.github.io/litjson/

// 0. 지역의 메타 데이터 정보는 캐싱 여부와 상관없이 확인할 필요가 있음
// 1. 다운로드된 파노라마 및 큐브맵 이미지 확인
// 2-1. 기존 파노라마가 없으면 기존 방식대로 다운로드
// 2-2. 기존 파노라마가 있으면 큐브맵을 기존 텍스쳐를 로딩하는 방식으로 대체한다.

public class StreetViewRenderer : MonoBehaviour
{ 
    bool isRendering = false; // 현재 파노라마에서 스트리트뷰 변환 작업을 진행중인가

    static private StreetViewRenderer instance;
    public static StreetViewRenderer Instance()
    {
        if (!instance)
        {
            instance = FindObjectOfType(typeof(StreetViewRenderer)) as StreetViewRenderer;
            if (!instance)
                Debug.LogError("There needs to be one active StreetViewRenderer script on a GameObject in your scene.");
        }

        return instance;
    }

    public string panoramaID = "";
    public string defaultID = "IUmXlW6pRu1w9QnUdQk4vw"; // 성산일출봉
    // "LbmCZ1nt-bgAAAQINlMIVQ"; // 경희궁
    // "zMrHSTO0GCYAAAQINlCkXg"; // 경희궁
    // 2C2MIWjhrZUAAAQfCVLQkw

    public static Texture2D panoramaTexture; // for merged street view
    public string saveTextureFileName;
    public Material skyboxMaterial;

    // DRAW Buttons
    public GameObject buttonModel;
    private GameObject[] buttonModelList;

    // GOOGLE RETRIVE VARIABLES
    private string cbkURL = "http://maps.google.com/cbk?";
    private int imageWidth = 0;
    private int imageHeight = 0;
    private int tileWidth = 0;
    private int tileHeight = 0;
    private int zoomLevels;

    private string description;
    private string country;
    private string region;

    // SCRIPT & CALCULATE VARIABLES
    private int textureWidth = 0; // imageWidth / (( zoomLevels - zoom ) * 2)
    private int textureHeight = 0;// imageHeight / (( zoomLevels - zoom ) * 2)
    private int rowTilesNum = 0;
    private int colTilesNum = 0;
    private int totalTilesNum; // rowTileNums * colTileNums;
    
    public float pivotYaw = 0.0f; // Panorama Pivot Yaw Degree ; GetMetaData

    // CBK CUSTOM VARIABLES
    private const int zoom = 3; // Default Panorama Zoom Size
    private Texture2D[,] tiles;
    private int downloadedTilesCount;

    // NEED TO INITIATE VALUE BEFORE START RENDERING
    private bool metadataExists = false;
    private int retryCount = 0;

    // Panorama To Cubemap
    private const int FRONT = 0;
    private const int BACK = 1;
    private const int LEFT = 2;
    private const int RIGHT = 3;
    private const int UP = 4;
    private const int DOWN = 5;

    private float m_direction = 0.0f;
    private int[] m_textureSize = { 64, 128, 256, 512, 1024, 2048 };
    private int m_textureSizeIndex = 4;

    public Texture2D[] cubeTexture = new Texture2D[6];
    
	// CACHE
    Queue<string> queue;

    static bool enableStackTrace = false;
    static Stack<string> panoIDStack = null;

    private const int MAX_RETRY = 10;

    StreetViewManager SVManager;
    LoadingScreen loadingScreen;

    void Start()
    {
        SVManager = StreetViewManager.Instance;
        loadingScreen = LoadingScreen.Instance();
        
        StartRenderStreetView();
    }

    void InitStackTrace()
    {
        if(panoIDStack == null)
            panoIDStack = new Stack<string>();
        else
            panoIDStack.Clear();
    }

    void InitAutoImageCache()
    {
        if (SVManager.enableAutoImageCache)
        {
            if (queue == null)
                queue = new Queue<string>();
            else
                queue.Clear();

            queue.Enqueue(SVManager.PlaceID);
        }
    }
    
    public void StartRenderStreetView()
    {
        if (isRendering) return;

        InitStackTrace();
        InitAutoImageCache();
        /* 스트리트뷰 정보 확인 */
        print("StreetViewer Start");

        if( SVManager.enableAutoImageCache )
        {
            StartCoroutine(AutoImageCaching());
        }
        else 
        {   
            StartCoroutine(RenderStreetView());
        }
		//StartCoroutine(RenderStreetView());

        print("StreetViewer Start End");
    }

    private void GetCachedImageFromID(string id)
    {
        LoadTexture(Utility.cacheFolderPath + "/" + id + "_front.png", ref cubeTexture[FRONT]);
		LoadTexture(Utility.cacheFolderPath + "/" + id + "_back.png", ref cubeTexture[BACK]);
		LoadTexture(Utility.cacheFolderPath + "/" + id + "_left.png", ref cubeTexture[LEFT]);
		LoadTexture(Utility.cacheFolderPath + "/" + id + "_right.png", ref cubeTexture[RIGHT]);
		LoadTexture(Utility.cacheFolderPath + "/" + id + "_up.png", ref cubeTexture[UP]);
        LoadTexture(Utility.cacheFolderPath + "/" + id + "_down.png", ref cubeTexture[DOWN]);
    }

    void Initialize()
    {
        loadingScreen.Progress = 0; // 진행률 초기화

        panoramaID = SVManager.PlaceID; // 파노라마 아이디 설정
        print("Panorama ID : " + panoramaID);

        if (panoramaID == null)
            panoramaID = defaultID;

        saveTextureFileName = panoramaID;
        
        cubeTexture[FRONT] = new Texture2D(512, 512);
        cubeTexture[BACK] = new Texture2D(512, 512);
        cubeTexture[LEFT] = new Texture2D(512, 512);
        cubeTexture[RIGHT] = new Texture2D(512, 512);
        cubeTexture[UP] = new Texture2D(512, 512);
        cubeTexture[DOWN] = new Texture2D(512, 512);
        
        metadataExists = false;
        retryCount = 0; // 메타데이터 재시도 횟수
        downloadedTilesCount = 0;
    }

    // Step 1
    IEnumerator RenderStreetView() // INIT VARIABLES AND DOWNLOAD START
    {
        /* 파노라마 렌더링 시작 */
        // 파노라마 아이디를 통해 파노라마 이미지에 대한 부가 정보를 받는다.
        Initialize();
        loadingScreen.Show();
        isRendering = true;

        if (enableStackTrace)
        {
            panoIDStack.Push(panoramaID);
        }

        if(SVManager.enableAutoImageCache)
        {
            if (queue.Count == 0)
            {
                print("queue empty");
                yield break;
            }
            print("dequeue : " + panoramaID);
            panoramaID = queue.Dequeue();
        }

        string[] stackList = panoIDStack.ToArray();
        for(int i=0; i<stackList.Length; i++)
        {
            Debug.LogWarning("stack " + i.ToString() + " : " + stackList[i]);
        }
        
        loadingScreen.Comment = "메타데이터 받는중";

        Debug.Log("ID -> META DATA");
        do
        {
            yield return StartCoroutine(GetMetaData());
            retryCount++;
        } while (metadataExists == false && retryCount < MAX_RETRY);
        Debug.Log("GetMetaData End : Retry Count is " + retryCount.ToString());
        
        if(retryCount == MAX_RETRY)
        {
            Debug.LogError("Get Meta Data Failed");
            retryCount = 0;
        }

        if(Utility.FindCachedImageFromID(panoramaID)) // 해당 데이터가 캐시폴더에 있을 경우
        {
            Debug.Log("Image data is exist");
            // 6방향 이미지를 모두 로드하고
            GetCachedImageFromID(panoramaID);
                yield return new WaitForSeconds(1.0f); // 1초간 대기
            loadingScreen.Progress = 100;
        }
        else
        {
            loadingScreen.Comment = "이미지 타일 받는 중";
            yield return StartCoroutine(GetPanoramaImage(panoramaID, textureWidth, textureHeight));
            loadingScreen.Comment = "이미지 합치는 중";
            yield return StartCoroutine(MergeTiles());
            loadingScreen.Comment = "큐브맵 생성 중";
            yield return StartCoroutine(ConvertPanoramaToCubemap());
        }

        // 메타데이터 받고
        // 기존에 다운로드 받은 큐브맵이 있는지 확인한다.
        // 있다면 바로 스카이박스에 적용시키고 끝낸다.

        SetSkybox();
        DrawButtons();

        if (SVManager.enableAutoImageCache)
        {
            for (int i = 0; i < SVManager.nextIDs.Length; i++)
            {
                if (!Utility.FindCachedImageFromID(SVManager.nextIDs[i]))
                {
                    print("enque : " + SVManager.nextIDs[i]);
                    queue.Enqueue(SVManager.nextIDs[i]);
                }
            }
        }


        loadingScreen.Hide();
        isRendering = false;
    }

    IEnumerator AutoImageCaching()
    {
        while(queue.Count != 0)
        {
            yield return StartCoroutine(RenderStreetView());
        }
        Application.LoadLevel("SceneEarth");
    }

    // Step 2
    IEnumerator GetMetaData()
    {
        string metaURL = cbkURL + "output=json" + "&panoid=" + panoramaID;
        WWW www = new WWW(metaURL);
        yield return www;

        if (!string.IsNullOrEmpty(www.error))
        {
            Debug.Log("WWW Error [Meta Data] : " + www.error);
            metadataExists = false;
            yield break;
        }
        else
        {
            metadataExists = true;
        }

        JsonData json = JsonMapper.ToObject(www.text);
        JsonData data = json["Data"];

        imageWidth = Convert.ToInt32(data["image_width"].ToString());
        imageHeight = Convert.ToInt32(data["image_height"].ToString());
        tileWidth = Convert.ToInt32(data["tile_width"].ToString());
        tileHeight = Convert.ToInt32(data["tile_height"].ToString());

        print("image width : " + imageWidth + " image height : " + imageHeight);
        print("tile width : " + tileWidth + " tile height : " + tileHeight);

        JsonData projection = json["Projection"];
        pivotYaw = Convert.ToSingle(projection["pano_yaw_deg"].ToString());

        JsonData location = json["Location"];
        zoomLevels = Convert.ToInt32(location["zoomLevels"].ToString());

        string locationText = "";
        if (location.Keys.Contains("description"))
        {
            description = location["description"].ToString();
            locationText += description;
        }
        if (location.Keys.Contains("country"))
        {
            country = location["country"].ToString();
            locationText += ", " + country;
        }
        if (location.Keys.Contains("region"))
        {
            region = location["region"].ToString();
            locationText += ", " + region;
        }

        loadingScreen.PlaceText = locationText;
        // 현재 파노라마 위치에서 갈 수 있는 방향 및 파노라마 ID 정보를 파싱한다.
        JsonData links = json["Links"];
        int linkCount = links.Count;

        SVManager.nextIDs = new string[linkCount];
        SVManager.nextDegrees = new string[linkCount];

        for (int i = 0; i < linkCount; i++)
        {
            JsonData item = links[i];
            string linkID = item["panoId"].ToString();
            SVManager.nextIDs[i] = linkID;
            string yawDeg = item["yawDeg"].ToString();
            SVManager.nextDegrees[i] = yawDeg;

            print("Link ID : " + linkID + " yawDeg : " + yawDeg);
        }

        textureWidth = imageWidth / ((zoomLevels - zoom) * 2);
        textureHeight = imageHeight / ((zoomLevels - zoom) * 2);
    }

    // Step 3
    IEnumerator GetPanoramaImage(string pano_id, int width, int height)
    {
        string output = "tile";

        if (panoramaTexture != null)
        {
            DestroyImmediate(panoramaTexture);
            panoramaTexture = null;
            
            GC.Collect();
            GC.WaitForPendingFinalizers();
        }

        // 파노라마 이미지의 크기만큼 담을 수 있는 빈 텍스쳐 공간을 메모리에 할당한다.
        panoramaTexture = new Texture2D(width, height);
        downloadedTilesCount = 0;

        rowTilesNum = height / tileHeight;
        if ((height % tileHeight) > 0)
            rowTilesNum += 1;

        colTilesNum = width / tileWidth;
        if ((width % tileHeight) > 0)
            colTilesNum += 1;

        totalTilesNum = rowTilesNum * colTilesNum;

        tiles = new Texture2D[rowTilesNum, colTilesNum];

        for (int y = 0; y < rowTilesNum; y++)
        {
            for (int x = 0; x < colTilesNum; x++)
            {
                yield return StartCoroutine(GoogleStreetViewTiled(output, panoramaID, zoom, x, y));
            }
        }

        if (downloadedTilesCount == totalTilesNum)
            Debug.Log("All tiles downloaded!");
        else
            Debug.LogWarning("Some tiles missing");
    }

    // 전체 파노라마 타일중 x, y 좌표에 위치한 타일 하나를 받아오는 함수 
    // http://cbk0.google.com/cbk?output=tile&panoid=Q_7cCDOIMymvWZcLQoOTjQ&zoom=3&x=2&y=1
    IEnumerator GoogleStreetViewTiled(string output, string pano_id, int zum, int x, int y)
    {
        string url = cbkURL
            + "output=" + output
            + "&panoid=" + pano_id
            + "&zoom=" + zum
            + "&x=" + x
            + "&y=" + y;

        WWW www = new WWW(url);
        yield return www;

        if (!string.IsNullOrEmpty(www.error))
            Debug.LogError("Panorama Download Error : " + www.error);
        else
            Debug.Log("Panorama Download : " + downloadedTilesCount);

        tiles[y, x] = www.texture;
        downloadedTilesCount++;
        loadingScreen.IncreaseProgress(2);
    }

    IEnumerator MergeTiles()
    {
        int penPosX;
        int penPosY;

        for (int i = 0; i < rowTilesNum; i++)
        {
            for (int j = 0; j < colTilesNum; j++)
            {
                if (tiles[i, j] == null)
                {
                    print("tile is null " + i + "," + j);
                    continue;
                }

                for (int y = 0; y < tiles[i, j].height; y++)
                {
                    for (int x = 0; x < tiles[i, j].width; x++)
                    {
                        penPosX = (tileWidth * j) + x;
                        penPosY = textureHeight - (tileHeight * (i + 1)) + y;
                        if (penPosX < textureWidth && penPosY > 0)
                            panoramaTexture.SetPixel(penPosX, penPosY, tiles[i, j].GetPixel(x, y));
                    }
                }

                DestroyImmediate(tiles[i, j]);
            }
        }

        panoramaTexture.Apply();
        loadingScreen.IncreaseProgress(6);
        yield return null;
    }

    void DrawButtons()
    {
        // 만약 기존의 화살표 프리팹이 있다면 정리해준다.
        if (buttonModelList != null && buttonModelList.Length > 0)
        {
            for (int i = 0; i < buttonModelList.Length; i++)
            {
                Destroy(buttonModelList[i]);
            }
        }

        buttonModelList = new GameObject[SVManager.nextDegrees.Length];
        for (int i = 0; i < SVManager.nextDegrees.Length; i++)
        {
            buttonModelList[i] = Instantiate(buttonModel, transform.position, Quaternion.identity) as GameObject;
            buttonModelList[i].GetComponent<Button>().SetDegree(Convert.ToSingle(SVManager.nextDegrees[i]) - pivotYaw);
            buttonModelList[i].GetComponent<Button>().SetPanoramaID(SVManager.nextIDs[i]);
        }
    }

    bool isConverted = false;
    int textureCount = 0;
    private IEnumerator ConvertPanoramaToCubemap()
    {
        int texSize = GetCubemapTextureSize();
        isConverted = false;
        textureCount = 0;

        //StartCoroutine(CreateCubemapTexture(texSize, FRONT, panoramaID + "_front.png"));
        //StartCoroutine(CreateCubemapTexture(texSize, BACK, panoramaID + "_back.png"));
        //StartCoroutine(CreateCubemapTexture(texSize, LEFT, panoramaID + "_left.png"));
        //StartCoroutine(CreateCubemapTexture(texSize, RIGHT, panoramaID + "_right.png"));
        //StartCoroutine(CreateCubemapTexture(texSize, UP, panoramaID + "_up.png"));
        //StartCoroutine(CreateCubemapTexture(texSize, DOWN, panoramaID + "_down.png"));
        //while (textureCount < 6)
        //{
        //    //IEnumerator e = CreateCubemapTexture();
        //    Debug.Log("Waiting...");
        //    yield return null;
        //}
        //Debug.Log("Waiting End");


        yield return StartCoroutine(CreateCubemapTexture(texSize, FRONT, panoramaID + "_front.png"));
        loadingScreen.IncreaseProgress(6);
        yield return StartCoroutine(CreateCubemapTexture(texSize, BACK, panoramaID + "_back.png"));
        loadingScreen.IncreaseProgress(6);
        yield return StartCoroutine(CreateCubemapTexture(texSize, LEFT, panoramaID + "_left.png"));
        loadingScreen.IncreaseProgress(6);
        yield return StartCoroutine(CreateCubemapTexture(texSize, RIGHT, panoramaID + "_right.png"));
        loadingScreen.IncreaseProgress(6);
        yield return StartCoroutine(CreateCubemapTexture(texSize, UP, panoramaID + "_up.png"));
        loadingScreen.IncreaseProgress(6);
        yield return StartCoroutine(CreateCubemapTexture(texSize, DOWN, panoramaID + "_down.png"));
        loadingScreen.IncreaseProgress(6);

        yield return null;
    }

    private void SetSkybox()
    {
        skyboxMaterial = SkyboxRenderer.CreateSkyboxMaterial(cubeTexture[FRONT], cubeTexture[BACK], cubeTexture[LEFT], cubeTexture[RIGHT], cubeTexture[UP], cubeTexture[DOWN]);
        RenderSettings.skybox = skyboxMaterial;
    }

    private int GetCubemapTextureSize()
    {
        int size = 512;
        if (m_textureSize.Length > m_textureSizeIndex)
        {
            size = m_textureSize[m_textureSizeIndex];
        }
        return size;
    }

    
    IEnumerator CreateCubemapTexture(int texSize, int faceIndex, string fileName = null)
    {
        Texture2D tex = new Texture2D(texSize, texSize, TextureFormat.RGB24, false);

        // SHJO TODO : 만약 로컬에 해당하는 Panorama ID의 텍스쳐가 있다면 해당 리소스의 텍스쳐를 사용한다.
        print("Create Cubemap texture");
        Vector3[] vDirA = new Vector3[4];
        
        switch(faceIndex) {
            case FRONT:
                vDirA[0] = new Vector3(-1.0f, -1.0f, -1.0f);
                vDirA[1] = new Vector3(1.0f, -1.0f, -1.0f);
                vDirA[2] = new Vector3(-1.0f, 1.0f, -1.0f);
                vDirA[3] = new Vector3(1.0f, 1.0f, -1.0f);
                break;
            case BACK:
                vDirA[0] = new Vector3(1.0f, -1.0f, 1.0f);
                vDirA[1] = new Vector3(-1.0f, -1.0f, 1.0f);
                vDirA[2] = new Vector3(1.0f, 1.0f, 1.0f);
                vDirA[3] = new Vector3(-1.0f, 1.0f, 1.0f);
                break;
            case LEFT:
                vDirA[0] = new Vector3(1.0f, -1.0f, -1.0f);
                vDirA[1] = new Vector3(1.0f, -1.0f, 1.0f);
                vDirA[2] = new Vector3(1.0f, 1.0f, -1.0f);
                vDirA[3] = new Vector3(1.0f, 1.0f, 1.0f);
                break;
            case RIGHT:
                vDirA[0] = new Vector3(-1.0f, -1.0f, 1.0f);
                vDirA[1] = new Vector3(-1.0f, -1.0f, -1.0f);
                vDirA[2] = new Vector3(-1.0f, 1.0f, 1.0f);
                vDirA[3] = new Vector3(-1.0f, 1.0f, -1.0f);
                break;
            case UP:
                vDirA[0] = new Vector3(-1.0f, 1.0f, -1.0f);
                vDirA[1] = new Vector3(1.0f, 1.0f, -1.0f);
                vDirA[2] = new Vector3(-1.0f, 1.0f, 1.0f);
                vDirA[3] = new Vector3(1.0f, 1.0f, 1.0f);
                break;
            case DOWN:
                vDirA[0] = new Vector3(-1.0f, -1.0f, 1.0f);
                vDirA[1] = new Vector3(1.0f, -1.0f, 1.0f);
                vDirA[2] = new Vector3(-1.0f, -1.0f, -1.0f);
                vDirA[3] = new Vector3(1.0f, -1.0f, -1.0f);
                break;
        }

        Vector3 rotDX1 = (vDirA[1] - vDirA[0]) / texSize;
        Vector3 rotDX2 = (vDirA[3] - vDirA[2]) / texSize;

        float dy = 1.0f / texSize;
        float fy = 0.0f;

        Color[] cols = new Color[texSize];
        for (int y = 0; y < texSize; y++)
        {
            Vector3 xv1 = vDirA[0];
            Vector3 xv2 = vDirA[2];
            for (int x = 0; x < texSize; x++)
            {
                Vector3 v = ((xv2 - xv1) * fy) + xv1;
                v.Normalize();

                cols[x] = CalcProjectionSpherical(v);
                
                xv1 += rotDX1;
                xv2 += rotDX2;
            }
            tex.SetPixels(0, y, texSize, 1, cols);
            //yield return null;

            fy += dy;
        }

        tex.wrapMode = TextureWrapMode.Clamp;
        tex.Apply();

        if (totalTilesNum == downloadedTilesCount) // ** 다운로드 받은 타일수와 전체 타일수가 같을 경우만 큐브맵을 저장한다.
            SaveTexture(ref tex, fileName);

        cubeTexture[faceIndex] = tex;

        textureCount++;
        yield return null;
    }



    private Color CalcProjectionSpherical(Vector3 vDir)
    {
        float theta = Mathf.Atan2(vDir.z, vDir.x);		// -π ~ +π.
        float phi = Mathf.Acos(vDir.y);				//  0 ~ +π

        //theta += m_direction * Mathf.PI / 180.0f;
        theta += Mathf.PI / 180.0f;
        while (theta < -Mathf.PI) theta += Mathf.PI + Mathf.PI;
        while (theta > Mathf.PI) theta -= Mathf.PI + Mathf.PI;

        float dx = theta / Mathf.PI;		// -1.0 ~ +1.0.
        float dy = phi / Mathf.PI;			//  0.0 ~ +1.0.

        dx = dx * 0.5f + 0.5f;
        int px = (int)(dx * panoramaTexture.width);
        if (px < 0) px = 0;
        if (px >= panoramaTexture.width) px = panoramaTexture.width - 1;
        int py = (int)(dy * panoramaTexture.height);
        if (py < 0) py = 0;
        if (py >= panoramaTexture.height) py = panoramaTexture.height - 1;

        Color col = panoramaTexture.GetPixel(px, panoramaTexture.height - py - 1);
        
        return col;
    }

    public void SaveTexture(ref Texture2D tex, string saveFileName)
    {
        string realSavePath = Utility.cacheFolderPath + "/" + saveFileName;
        File.WriteAllBytes(realSavePath, tex.EncodeToPNG());
    }

    public bool LoadTexture(string filePath, ref Texture2D tex)
    {
        if (File.Exists(filePath))
        {
            bool res = tex.LoadImage(File.ReadAllBytes(filePath));
            tex.wrapMode = TextureWrapMode.Clamp;
            tex.Apply();

        }
        return false;
    }

}

/* JSON Format
   Data:{
   image_width:
   image_height:
   tile_width:
   tile_height:
   image_date
   }
   Projection:{
       "projection_type":"spherical","pano_yaw_deg":"302.11","tilt_yaw_deg":"-180","tilt_pitch_deg":"0"
   }
   Location:{
       panoId:
       level_id:
       zoomLevels:
       lat:
       lng:
       original_lat:
       original_lng:
       description:
       region:
       country:
   }
   Links:[{"yawDeg":"162.08","panoId":"lU_IGRhMM3oAAAQINlMI5w","road_argb":"0x80fdf872","description":"","scene":"1"},
           {"yawDeg":"358.9","panoId":"iP_znBxAS-IAAAQINlCqtA","road_argb":"0x80fdf872","description":"","scene":"1"},
           {"yawDeg":"230.09","panoId":"LbmCZ1nt-bgAAAQINlMIVQ","road_argb":"0x80fdf872","description":"","scene":"1"}]	
   }
    */
