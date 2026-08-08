using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;
using System.Linq;

// ANTI-STUTTER Texture Loader - Smooth 60 FPS!
public class LazyTextureLoader : MonoBehaviour
{
    [Header("Texture Settings")]
    public string textureName = "";
    public string textureFolder = "MuseumTextures";

    [Header("Performance Settings")]
    [Range(512, 4096)]
    public int maxTextureSize = 1024;

    [Range(10f, 100f)]
    public float loadDistance = 40f;

    [Range(50f, 200f)]
    public float unloadDistance = 80f;

    [Header("Debug")]
    public bool showDebugLogs = false;

    private Renderer myRenderer;
    private Material sharedMaterial;
    private string materialKey;
    private bool isTextureLoaded = false;
    private float lastCheckTime = 0f;
    private float checkInterval = 0.5f;

    private static Transform mainCamera;
    private static TextureLoadManager loadManager;

    // Global cache
    private static Dictionary<string, Texture2D> textureCache = new Dictionary<string, Texture2D>();
    private static Dictionary<string, int> refCount = new Dictionary<string, int>();

    void Awake()
    {
        // Init load manager (singleton)
        if (loadManager == null)
        {
            GameObject managerObj = new GameObject("_TextureLoadManager");
            loadManager = managerObj.AddComponent<TextureLoadManager>();
            DontDestroyOnLoad(managerObj);
        }
    }

    void Start()
    {
        if (!gameObject.activeInHierarchy)
        {
            enabled = false;
            return;
        }

        myRenderer = GetComponent<Renderer>();

        if (myRenderer == null || myRenderer.sharedMaterial == null)
        {
            enabled = false;
            return;
        }

        materialKey = myRenderer.sharedMaterial.GetInstanceID().ToString();
        sharedMaterial = myRenderer.sharedMaterial;

        // Auto-detect texture name
        if (string.IsNullOrEmpty(textureName))
        {
            if (sharedMaterial.mainTexture != null)
            {
                textureName = sharedMaterial.mainTexture.name;
            }
            else
            {
                textureName = gameObject.name;
            }
        }

        // Cari main camera
        if (mainCamera == null)
        {
            if (Camera.main != null)
            {
                mainCamera = Camera.main.transform;
            }
            else
            {
                Camera cam = FindObjectOfType<Camera>();
                if (cam != null) mainCamera = cam.transform;
            }
        }
    }

    void Update()
    {
        if (mainCamera == null) return;

        // Throttle checking
        if (Time.time - lastCheckTime < checkInterval) return;
        lastCheckTime = Time.time;

        float distance = Vector3.Distance(transform.position, mainCamera.position);

        // Request load kalau deket
        if (distance <= loadDistance && !isTextureLoaded)
        {
            RequestLoad(distance);
        }
        // Unload kalau jauh
        else if (distance > unloadDistance && isTextureLoaded)
        {
            UnloadTexture();
        }
    }

    void RequestLoad(float distance)
    {
        // Cek cache dulu
        if (textureCache.ContainsKey(textureName))
        {
            ApplyFromCache();
            return;
        }

        // Queue ke load manager dengan priority
        loadManager.QueueLoad(new LoadRequest
        {
            loader = this,
            textureName = textureName,
            textureFolder = textureFolder,
            maxSize = maxTextureSize,
            priority = 1f / (distance + 1f), // Makin deket makin priority
            showLogs = showDebugLogs
        });
    }

    void ApplyFromCache()
    {
        if (sharedMaterial != null && textureCache.ContainsKey(textureName))
        {
            sharedMaterial.mainTexture = textureCache[textureName];

            if (!refCount.ContainsKey(textureName))
                refCount[textureName] = 0;
            refCount[textureName]++;

            isTextureLoaded = true;
        }
    }

    public void OnTextureLoaded(Texture2D texture)
    {
        if (sharedMaterial != null && texture != null)
        {
            sharedMaterial.mainTexture = texture;
            isTextureLoaded = true;

            if (!refCount.ContainsKey(textureName))
                refCount[textureName] = 0;
            refCount[textureName]++;
        }
    }

    void UnloadTexture()
    {
        if (!isTextureLoaded) return;

        if (refCount.ContainsKey(textureName))
        {
            refCount[textureName]--;

            if (refCount[textureName] <= 0)
            {
                if (textureCache.ContainsKey(textureName))
                {
                    Destroy(textureCache[textureName]);
                    textureCache.Remove(textureName);
                }
                refCount.Remove(textureName);
            }
        }

        isTextureLoaded = false;
    }

    void OnDestroy()
    {
        if (isTextureLoaded)
        {
            UnloadTexture();
        }
    }

    void OnDisable()
    {
        if (isTextureLoaded)
        {
            UnloadTexture();
        }
    }

    // Static method untuk akses cache
    public static Dictionary<string, Texture2D> GetCache()
    {
        return textureCache;
    }
}

// ============================================================================
// LOAD MANAGER - Handle loading dengan frame budget
// ============================================================================
public class TextureLoadManager : MonoBehaviour
{
    private Queue<LoadRequest> loadQueue = new Queue<LoadRequest>();
    private HashSet<string> currentlyLoading = new HashSet<string>();

    [Header("Performance")]
    [Tooltip("Max texture yang di-load per frame")]
    public int maxLoadsPerFrame = 1;

    [Tooltip("Max waktu loading per frame (ms)")]
    public float frameBudgetMs = 8f;

    private Dictionary<string, Texture2D> cache;

    void Start()
    {
        cache = LazyTextureLoader.GetCache();
    }

    void Update()
    {
        ProcessQueue();
    }

    public void QueueLoad(LoadRequest request)
    {
        // Skip kalau udah ada di cache
        if (cache.ContainsKey(request.textureName))
        {
            request.loader.OnTextureLoaded(cache[request.textureName]);
            return;
        }

        // Skip kalau sedang loading
        if (currentlyLoading.Contains(request.textureName))
        {
            return;
        }

        // Cek apakah sudah ada di queue
        if (loadQueue.Any(r => r.textureName == request.textureName))
        {
            return;
        }

        loadQueue.Enqueue(request);
    }

    void ProcessQueue()
    {
        if (loadQueue.Count == 0) return;

        float frameStartTime = Time.realtimeSinceStartup * 1000f;
        int loadsThisFrame = 0;

        // Sort by priority (makin deket kamera makin duluan)
        var sortedQueue = loadQueue.OrderByDescending(r => r.priority).ToList();
        loadQueue.Clear();

        foreach (var request in sortedQueue)
        {
            // Check frame budget
            float elapsed = (Time.realtimeSinceStartup * 1000f) - frameStartTime;
            if (elapsed > frameBudgetMs || loadsThisFrame >= maxLoadsPerFrame)
            {
                // Queue ulang sisanya
                loadQueue.Enqueue(request);
                continue;
            }

            // Skip kalau udah di cache
            if (cache.ContainsKey(request.textureName))
            {
                request.loader.OnTextureLoaded(cache[request.textureName]);
                continue;
            }

            // Load texture
            if (!currentlyLoading.Contains(request.textureName))
            {
                StartCoroutine(LoadTextureCoroutine(request));
                loadsThisFrame++;
            }
        }
    }

    IEnumerator LoadTextureCoroutine(LoadRequest request)
    {
        currentlyLoading.Add(request.textureName);

        string[] extensions = { ".png", ".jpg", ".jpeg" };
        Texture2D loadedTex = null;

        foreach (string ext in extensions)
        {
            string fileName = request.textureName + ext;
            string filePath = Path.Combine(Application.streamingAssetsPath, request.textureFolder, fileName);

            if (Application.platform == RuntimePlatform.Android)
            {
                using (UnityWebRequest www = UnityWebRequestTexture.GetTexture(filePath))
                {
                    yield return www.SendWebRequest();

                    if (www.result == UnityWebRequest.Result.Success)
                    {
                        loadedTex = DownloadHandlerTexture.GetContent(www);
                        break;
                    }
                }
            }
            else
            {
                if (File.Exists(filePath))
                {
                    byte[] fileData = File.ReadAllBytes(filePath);

                    // Yield untuk spread load
                    yield return null;

                    loadedTex = new Texture2D(2, 2);
                    loadedTex.LoadImage(fileData);

                    // Resize kalau perlu
                    if (loadedTex.width > request.maxSize || loadedTex.height > request.maxSize)
                    {
                        loadedTex = ResizeTexture(loadedTex, request.maxSize);
                        yield return null;
                    }

                    // Compress (spread over frames)
                    loadedTex.Compress(true);
                    yield return null;

                    break;
                }
            }
        }

        if (loadedTex != null)
        {
            cache[request.textureName] = loadedTex;
            request.loader.OnTextureLoaded(loadedTex);

            if (request.showLogs)
            {
                Debug.Log($"✓ Loaded: {request.textureName} ({loadedTex.width}x{loadedTex.height})");
            }
        }
        else
        {
            if (request.showLogs)
            {
                Debug.LogWarning($"⚠️ Not found: {request.textureName}");
            }
        }

        currentlyLoading.Remove(request.textureName);
    }

    Texture2D ResizeTexture(Texture2D source, int maxSize)
    {
        int width = source.width;
        int height = source.height;

        if (width <= maxSize && height <= maxSize) return source;

        float ratio = Mathf.Min((float)maxSize / width, (float)maxSize / height);
        int newWidth = Mathf.RoundToInt(width * ratio);
        int newHeight = Mathf.RoundToInt(height * ratio);

        RenderTexture rt = RenderTexture.GetTemporary(newWidth, newHeight);
        RenderTexture.active = rt;

        Graphics.Blit(source, rt);

        Texture2D result = new Texture2D(newWidth, newHeight, TextureFormat.RGB24, false);
        result.ReadPixels(new Rect(0, 0, newWidth, newHeight), 0, 0);
        result.Apply();

        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);

        Destroy(source);

        return result;
    }
}

// ============================================================================
// LOAD REQUEST DATA
// ============================================================================
public class LoadRequest
{
    public LazyTextureLoader loader;
    public string textureName;
    public string textureFolder;
    public int maxSize;
    public float priority;
    public bool showLogs;
}