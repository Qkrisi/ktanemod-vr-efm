using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Web;
using System.Collections.Generic;
using UnityEngine;
using PdfiumViewer;

public class VrEfmService : MonoBehaviour
{

    private Dictionary<string, LinkedList<Texture2D>> ManualCache = new Dictionary<string, LinkedList<Texture2D>>();
    private LinkedListNode<Texture2D> CurrentManual = null;

    private readonly Dictionary<string, string> ModuleOverrides = new Dictionary<string, string>()
    {
        {"A>N<D", "A_N_D"},
        {"...?", "Punctuation Marks"}
    };

    private readonly string path = Path.Combine(Application.persistentDataPath, "VrEfm");

    public ModuleNote CurrentNote = new ModuleNote();
    public static VrEfmService instance = null;
    

    public void Start()
    {
        instance = this;
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
    }

    private Texture2D HandleImage(Image image, SizeF size)
    {
        MemoryStream ms = new MemoryStream();
        image.Save(ms, image.RawFormat);
        var texture = new Texture2D((int)size.Width, (int)size.Height);
        texture.LoadImage(ms.ToArray());
        return texture;
    }

    private LinkedList<Texture2D> PrepareModule(string module)
    {
        ModuleOverrides.TryGetValue(module, ref module);
        if (!ManualCache.ContainsKey(module))
        {
            using (WebClient client = new WebClient())
            {
                try
                {
                    string FilePath = Path.Combine(path, $"{module}.pdf");
                    client.DownloadFile(HttpUtility.UrlEncode($"https://ktane.timwi.de/PDF/{module}.pdf"), FilePath);
                    using (var document = PdfDocument.Load(FilePath))
                    {
                        var l = new LinkedList<Texture2D>();
                        for(int i = 0;i<document.PageCount;i++) l.AddLast(HandleImage(document.Render(i, 300, 300, true), document.PageSizes[i]));
                        ManualCache.Add(module, l);
                    }
                    //File.Delete(FilePath);
                }
                catch(Exception e)
                {
                    Debug.LogFormat("An error occurred while trying to get manual: {0}", e.ToString());
                    return null;
                }
            }
        }
        return ManualCache[module];
    }

    private void HandleModule(string module)
    {
        var prepared = PrepareModule(module);
    }
}