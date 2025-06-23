using System;
using System.Collections;
using System.Collections.Generic;
using Futureverse.UBF.Runtime.Execution;
using SFB;
using UnityEngine;
using System.IO;
using System.Linq;
using Futureverse.UBF.Runtime;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

public class DemoSceneController : MonoBehaviour
{
    private UBFRuntimeController RuntimeController;

    public string path;
    private Blueprint _blueprint;

    private ExportCatalog Catalog;
    
    [ContextMenu("Select instance")]
    private void SelectGraphFile()
    {
        var paths = StandaloneFileBrowser.OpenFilePanel("Select Blueprint", "", "ubp.json", false);
        if (paths == null || paths.Length == 0)
        {
            Debug.LogError("Select root folder: No folder selected");
            return;
        }
        
        path = paths[0];

        if (TryValidateBlueprint(out var bp))
        {
            _blueprint = bp;
        }

        if (!TryFormCombinedCatalog())
        {
            Debug.LogError("Failed to form catalog");
        }
    }

    private bool TryValidateBlueprint(out Blueprint blueprint)
    {
        var text = File.ReadAllText(path);
        if (Blueprint.TryLoad("", text, out blueprint)) return true;
        
        Debug.LogError("Failed to load blueprint");
        return false;
    }

    private bool TryFormCombinedCatalog()
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path))
            return false;

        var dirName = Path.GetDirectoryName(Path.GetFullPath(path));
        if (string.IsNullOrEmpty(dirName))
            return false;
        
        var currentDir = new DirectoryInfo(dirName);
        if (!currentDir.Name.Equals("assets", StringComparison.OrdinalIgnoreCase))
            return false;

        currentDir = currentDir.Parent;
        if (!currentDir.Name.Equals(".export", StringComparison.OrdinalIgnoreCase))
            return false;
        
        
        var catalogDir = Path.Combine(currentDir.FullName, "catalog");
        if (!Directory.Exists(catalogDir))
            return false;

        Catalog = new ExportCatalog();
        
        var catalogFiles = Directory.GetFiles(catalogDir, "*.json", SearchOption.TopDirectoryOnly);
        foreach (var file in catalogFiles)
        {
            var text = File.ReadAllText(file);
            try
            {
                var catalog = JsonConvert.DeserializeObject<ExportCatalog>(text);
                foreach (var resource in catalog.Resources)
                {
                    Catalog.Resources.Add(resource);
                }
            }
            catch
            {
                Debug.LogError("Failed to deserialize catalog: " + file);
            }
        }
        return true;
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
