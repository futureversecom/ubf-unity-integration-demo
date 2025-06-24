using System;
using System.Collections;
using System.Collections.Generic;
using Futureverse.UBF.Runtime.Execution;
using SFB;
using UnityEngine;
using System.IO;
using System.Linq;
using Futureverse.UBF.Runtime;
using Futureverse.UBF.Runtime.Builtin;
using Futureverse.UBF.Runtime.Resources;
using GLTFast;
using GLTFast.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using UnityEngine.Networking;

public class DemoSceneController : MonoBehaviour
{
    public UBFRuntimeController RuntimeController;
    public string path;

    private DemoArtifactProvider _artifactProvider;
    
    [ContextMenu("Select instance")]
    private void SelectGraphFile()
    {
        _artifactProvider = new DemoArtifactProvider();
        
        var paths = StandaloneFileBrowser.OpenFilePanel("Select Blueprint", "", "ubp.json", false);
        if (paths == null || paths.Length == 0)
        {
            Debug.LogError("Select root folder: No folder selected");
            return;
        }
        
        path = paths[0];

        if (string.IsNullOrEmpty(path) || !File.Exists(path))
        {
            Debug.LogError("No file at path " + path);
            return;
        }

        if (!_artifactProvider.TryRegisterBlueprint(File.ReadAllText(path)))
        {
            Debug.LogError("Failed to register blueprint " + path);
            return;
        }
        
        var dirName = Path.GetDirectoryName(Path.GetFullPath(path));
        if (string.IsNullOrEmpty(dirName))
        {
            Debug.LogError("Invalid directory path");
            return;
        }        
        var currentDir = new DirectoryInfo(dirName);
        if (!currentDir.Name.Equals("asset", StringComparison.OrdinalIgnoreCase))
        {
            Debug.LogError("Asset not located in asset folder. Instance directory: " + currentDir.Name);
            return;
        }
        currentDir = currentDir.Parent;
        if (!currentDir.Name.Equals(".export", StringComparison.OrdinalIgnoreCase))
        {
            Debug.LogError("Parent directory is not .export. Parent directory: " + currentDir.Name);
            return;
        }        
        
        var catalogDir = Path.Combine(currentDir.FullName, "catalog");
        if (!Directory.Exists(catalogDir))
        {
            Debug.LogError(".export folder does not contain catalog folder");
            return;
        }
        _artifactProvider.PopulateCatalog(catalogDir);
    }

    [ContextMenu("Run")]
    private void Run()
    {
        StartCoroutine(RuntimeController.Execute("root", _artifactProvider, new List<IBlueprintInstanceData>(),
            onComplete: Debug.Log));
    }
}

[JsonObject]
public class ExportCatalog
{
    [JsonObject]
    public class ExportResource
    {
        [JsonProperty("id")]
        public string Id;
        [JsonProperty("uri")]
        public string Uri;
        [JsonProperty("hash")]
        public string Hash;
        [JsonProperty("metadata")] 
        public JObject ImportSettings;
    }

    [JsonProperty("resources")]
    public List<ExportResource> Resources;

    [JsonIgnore]
    public string ZipPath;
    [JsonIgnore]
    public bool IsZip;
    [JsonIgnore] 
    public string CacheLocation;
        
    public ExportResource GetResource(string id)
    {
        return Resources.FirstOrDefault(x => x.Id == id);
    }

    public string GetResourceUri(string id)
    {
        var r = GetResource(id);
        return r?.Uri;
    }
}

public class DemoArtifactProvider : IArtifactProvider
{
    private Blueprint _blueprint;
    private ExportCatalog _catalog;

    public bool TryRegisterBlueprint(string json)
    {
        if (Blueprint.TryLoad("root", json, out var blueprint))
        {
            _blueprint = blueprint;
            return true;
        }
        
        Debug.LogError("Failed to load blueprint");
        return false;
    }
    
    public void PopulateCatalog(string catalogDirectory)
    {
        _catalog = new ExportCatalog
        {
            Resources = new List<ExportCatalog.ExportResource>()
        };

        var catalogFiles = Directory.GetFiles(catalogDirectory, "*.json", SearchOption.TopDirectoryOnly);
        Debug.Log($"{catalogFiles.Length} files found in catalog directory");
        foreach (var file in catalogFiles)
        {
            var text = File.ReadAllText(file);
            try
            {
                var catalog = JsonConvert.DeserializeObject<ExportCatalog>(text);
                foreach (var resource in catalog.Resources)
                {
                    if (!_catalog.Resources.Any(x => x.Id == resource.Id)) // Only add resource if it doesnt exist
                    {
                        _catalog.Resources.Add(resource);
                    }
                }
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to deserialize catalog: " + file);
                Debug.LogException(e);
            }
        }
        
        Debug.Log($"Catalog created with {_catalog.Resources.Count} resources");
    }
    
    public IEnumerator GetTextureResource(ResourceId resourceId, TextureImportSettings settings, Action<Texture2D, TextureAssetImportSettings> onComplete)
    {
        if (_catalog == null)
        {
            Debug.LogError($"Cannot find catalog for resource {resourceId}");
            onComplete?.Invoke(null, null);
            yield break;
        }
			
        var uri = _catalog.GetResourceUri(resourceId.Value);
        if (string.IsNullOrEmpty(uri))
        {
            Debug.LogError("Invalid resource URI. Please check that resource exists, and that it contains a valid path");
            onComplete?.Invoke(null, null);
            yield break;
        }
			
        Texture2D tex = null;
        yield return LoadResource(
            uri,
            (success, obj) =>
            {
                if (success)
                {
                    tex = obj as Texture2D;
                }
            }
        );
        
        if (tex == null)
        {
            onComplete?.Invoke(null, null);
            yield break;
        }
			
        if (settings != null && !settings.UseSrgb)
        {
            var linearTexture = new Texture2D(tex.width, tex.height, tex.format, tex.mipmapCount > 1, !settings.UseSrgb);
            linearTexture.SetPixels(tex.GetPixels());
            linearTexture.Apply();
            onComplete?.Invoke(linearTexture, null); // TODO implement a constructor for TextureAssetImportSettings here?
            yield break;
        }
        onComplete?.Invoke(tex, null);
    }

    public IEnumerator GetBlueprintResource(ResourceId resourceId, string instanceId, Action<Blueprint, BlueprintAssetImportSettings> onComplete)
	{
        if (resourceId.Value == "root") // This is the root graph with manual instance id
        {
            onComplete?.Invoke(_blueprint, new BlueprintAssetImportSettings());
            yield break;
        }
        if (_catalog == null)
        {
            Debug.LogError($"Cannot find catalog for resource {resourceId}");
            onComplete?.Invoke(null, null);
            yield break;
        }
			
        var uri = _catalog.GetResourceUri(resourceId.Value);
        if (string.IsNullOrEmpty(uri) || !File.Exists(uri))
        {
            Debug.LogError($"Invalid resource URI. Please check that resource exists, and that it contains a valid path.\nResource: {resourceId.Value}\nURI: {uri}");
            onComplete?.Invoke(null, null);
            yield break;
        }

        string text = File.ReadAllText(uri);
		Blueprint.TryLoad(instanceId, text, out var bp);
		Debug.Log($"Providing resource for graph {resourceId.Value}");
		onComplete?.Invoke(bp, new BlueprintAssetImportSettings());
		yield break;
	}

	public IEnumerator GetMeshResource(ResourceId resourceId, Action<GltfImport, MeshAssetImportSettings> onComplete)
	{
		if (_catalog == null)
		{
			Debug.LogError($"Cannot find catalog for resource {resourceId}");
			onComplete?.Invoke(null, null);
			yield break;
		}
		
		var resource = _catalog.GetResource(resourceId.Value);
		var uri = resource.Uri;
		if (string.IsNullOrEmpty(uri))
		{
			Debug.LogError("Invalid resource URI. Please check that resource exists, and that it contains a valid path");
			onComplete?.Invoke(null, null);
			yield break;
		}
		
		GltfImport gltf = null;
		
		yield return LoadResource(
			uri,
			(success, obj) =>
			{
				if (success)
				{
					gltf = obj as GltfImport;
				}
			}
		);
		
		if (gltf == null)
		{
			onComplete?.Invoke(null, null);
			yield break;
		}
		
		MeshAssetImportSettings importSettings = null;
		if (resource?.ImportSettings?.ToString() != "{}")
		{
			importSettings = resource.ImportSettings?.ToObject<MeshAssetImportSettings>();
		}
		onComplete?.Invoke(gltf, importSettings);
	}
    
    private IEnumerator LoadResource(string uri, Action<bool, object> onComplete)
    {
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
#else 
			uri = "file://" + uri;
#endif
        var ext = Path.GetExtension(uri);
        switch (ext)
        {
            case ".glb":
            {
                var request = UnityWebRequest.Get(uri);
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError(
                        $"Failed to download mesh.\nURI: {uri}\nResult: {request.responseCode} - {request.result}"
                    );

                    onComplete?.Invoke(false, null);
                    yield break;
                }

                var gltf = new GltfImport(deferAgent: new UninterruptedDeferAgent(), logger: new ConsoleLogger());
                var task = gltf.Load(request.downloadHandler.data);
                while (!task.IsCompleted)
                {
                    yield return null;
                }

                onComplete?.Invoke(true, gltf);
                yield break;
            }
            case ".png":
            {
                var request = UnityWebRequestTexture.GetTexture(uri);
                yield return request.SendWebRequest();

                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError(
                        $"Failed to download texture.\nURI: {uri}\nResult: {request.responseCode} - {request.result}"
                    );

                    onComplete?.Invoke(false, null);
                    yield break;
                }

                var tex = DownloadHandlerTexture.GetContent(request);
                onComplete?.Invoke(true, tex);
                yield break;
            }
            default:
                onComplete(false, null);
                break;
        }
    }
}