using UnityEngine;
using System.Threading;
using System.Collections;

// 하나의 파노라마 이미지에 대한 큐브맵 변환을 수행하는 클래스
// Input : SaveFileName, PanoramaTexture, PanoramaWidth, PanoramaHeight
// Output : Cubemap Texture
public class SkyboxRenderer : MonoBehaviour
{
    private const int FACE_FRONT = 0;
    private const int FACE_BACK = 1;
    private const int FACE_LEFT = 2;
    private const int FACE_RIGHT = 3;
    private const int FACE_UP = 4;
    private const int FACE_DOWN = 5;

    private float m_direction = 0.0f;
    private int[] m_textureSize = { 64, 128, 256, 512, 1024, 2048 };
    private int m_textureSizeIndex = 4;

    private static Texture2D panoramaTexture;
    public static Texture2D PanoramaTexture
    {
        get
        {
            return panoramaTexture;
        }

        set
        {
            panoramaTexture = value;
        }
    }

    private static Color[] colorSource;

    private static int panoramaWidth;
    private static int panoramaHeight;

    private static Texture2D[] cubeTexture = new Texture2D[6];

    public delegate void SkyboxRenderComplete();

    public static bool Init()
    {
        for (int i = 0; i < 6; i++)
        {
            if(cubeTexture[i] == null)
                cubeTexture[i] = new Texture2D(512, 512, TextureFormat.RGB24, false);
        }

        panoramaWidth = PanoramaTexture.width;
        panoramaHeight = PanoramaTexture.height;

        colorSource = panoramaTexture.GetPixels();
        return true;
    }

    public static void Render(string fileName)
    {
        if(panoramaTexture == null)
        {
            Debug.LogError("Panorama Texture is null");
            return;
        }

        Init();

        ConvertPanoramaToCubemap(fileName);
    }

    private static void ConvertPanoramaToCubemap(string fileName)
    {
        int texSize = 512;
        
        Debug.Log("Threading Start");    
        
        CreateCubemapTexture(texSize, FACE_FRONT, fileName + "_front.png");
        CreateCubemapTexture(texSize, FACE_BACK, fileName + "_back.png");
        CreateCubemapTexture(texSize, FACE_LEFT, fileName + "_left.png");
        CreateCubemapTexture(texSize, FACE_RIGHT, fileName + "_right.png");
        CreateCubemapTexture(texSize, FACE_UP, fileName + "_up.png");
        CreateCubemapTexture(texSize, FACE_DOWN, fileName + "_down.png");
        
        Debug.Log("Threading Done");
    }

    private static void CreateCubemapTexture(int texSize, int faceIndex, string fileName = null)
    {
        //Texture2D tex = new Texture2D(texSize, texSize, TextureFormat.RGB24, false);
  
        Vector3[] vDirA = new Vector3[4];
        switch (faceIndex)
        {
            case FACE_FRONT:
                vDirA[0] = new Vector3(-1.0f, -1.0f, -1.0f);
                vDirA[1] = new Vector3(1.0f, -1.0f, -1.0f);
                vDirA[2] = new Vector3(-1.0f, 1.0f, -1.0f);
                vDirA[3] = new Vector3(1.0f, 1.0f, -1.0f);
                break;
            case FACE_BACK:
                vDirA[0] = new Vector3(1.0f, -1.0f, 1.0f);
                vDirA[1] = new Vector3(-1.0f, -1.0f, 1.0f);
                vDirA[2] = new Vector3(1.0f, 1.0f, 1.0f);
                vDirA[3] = new Vector3(-1.0f, 1.0f, 1.0f);
                break;
            case FACE_LEFT:
                vDirA[0] = new Vector3(1.0f, -1.0f, -1.0f);
                vDirA[1] = new Vector3(1.0f, -1.0f, 1.0f);
                vDirA[2] = new Vector3(1.0f, 1.0f, -1.0f);
                vDirA[3] = new Vector3(1.0f, 1.0f, 1.0f);
                break;
            case FACE_RIGHT:
                vDirA[0] = new Vector3(-1.0f, -1.0f, 1.0f);
                vDirA[1] = new Vector3(-1.0f, -1.0f, -1.0f);
                vDirA[2] = new Vector3(-1.0f, 1.0f, 1.0f);
                vDirA[3] = new Vector3(-1.0f, 1.0f, -1.0f);
                break;
            case FACE_UP:
                vDirA[0] = new Vector3(-1.0f, 1.0f, -1.0f);
                vDirA[1] = new Vector3(1.0f, 1.0f, -1.0f);
                vDirA[2] = new Vector3(-1.0f, 1.0f, 1.0f);
                vDirA[3] = new Vector3(1.0f, 1.0f, 1.0f);
                break;
            case FACE_DOWN:
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
            cubeTexture[faceIndex].SetPixels(0, y, texSize, 1, cols);
            fy += dy;
        }
        cubeTexture[faceIndex].wrapMode = TextureWrapMode.Clamp;
        cubeTexture[faceIndex].Apply();

        //cubeTexture[faceIndex] = tex;
    }



    private static Color CalcProjectionSpherical(Vector3 vDir)
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
        int px = (int)(dx * panoramaWidth);
        if (px < 0) px = 0;
        if (px >= panoramaWidth) px = panoramaWidth - 1;
        int py = (int)(dy * panoramaHeight);
        if (py < 0) py = 0;
        if (py >= panoramaHeight) py = panoramaHeight - 1;

        //Color col = PanoramaTexture.GetPixel(px, panoramaHeight - py - 1);
        //Color col = colorSource[px, (panoramaHeight - py - 1)]
        Color col = colorSource[((panoramaHeight - py - 1) * panoramaWidth) + px];
        return col;
    }

    public static Material CreateSkyboxMaterial(SkyboxManifest manifest)
    {
        Material result = new Material(Shader.Find("RenderFX/Skybox"));
        result.SetTexture("_FrontTex", manifest.textures[0]);
        result.SetTexture("_BackTex", manifest.textures[1]);
        result.SetTexture("_LeftTex", manifest.textures[2]);
        result.SetTexture("_RightTex", manifest.textures[3]);
        result.SetTexture("_UpTex", manifest.textures[4]);
        result.SetTexture("_DownTex", manifest.textures[5]);
        return result;
    }

	public static Material CreateSkyboxMaterial(Texture2D front, Texture2D back, Texture2D left, Texture2D right, Texture2D up, Texture2D down )
	{
		Material result = new Material(Shader.Find("RenderFX/Skybox"));
		result.SetTexture("_FrontTex", front);
		result.SetTexture("_BackTex", back);
		result.SetTexture("_LeftTex", left);
		result.SetTexture("_RightTex", right);
		result.SetTexture("_UpTex", up);
		result.SetTexture("_DownTex", down);
		return result;
	}


    public Texture2D[] textures;
    void OnEnable()
    {
        SkyboxManifest manifest = new SkyboxManifest(textures[0], textures[1], textures[2], textures[3], textures[4], textures[5]);
        Material material = CreateSkyboxMaterial(manifest);
        SetSkybox(material);
        //enabled = false;
    }

    void SetSkybox(Material material)
    {
        GameObject camera = Camera.main.gameObject;
        Skybox skybox = camera.GetComponent<Skybox>();
        if (skybox == null)
            skybox = camera.AddComponent<Skybox>();
        skybox.material = material;
    }


    public struct SkyboxManifest
    {
        public Texture2D[] textures;

        public SkyboxManifest(Texture2D front, Texture2D back, Texture2D left, Texture2D right, Texture2D up, Texture2D down)
        {
            textures = new Texture2D[6]
            {
                 front,
                 back,
                 left,
                 right,
                 up,
                 down
             };
        }
    }

}

