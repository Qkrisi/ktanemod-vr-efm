using System;
using System.Drawing;
using System.IO;
using System.Net;
using System.Web;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Windows.Speech;
using PdfiumViewer;

public class VrEfmService : MonoBehaviour
{
    private Dictionary<string, LinkedList<Texture2D>> ManualCache = new Dictionary<string, LinkedList<Texture2D>>();
    private LinkedListNode<Texture2D> _CurrentManual = null;
    private LinkedListNode<Texture2D> CurrentManual
    {
        get
        {
            return _CurrentManual;
        }
        set
        {
            if (value != null)
            {
                _CurrentManual = value;
                ManualObject.GetComponent<Renderer>().material.mainTexture = _CurrentManual.Value;
            }
        }
    }

    private readonly Dictionary<string, string> ModuleOverrides = new Dictionary<string, string>()
    {
        {"A>N<D", "A_N_D"},
        {"...?", "Punctuation Marks"}
    };

    private readonly string path = Path.Combine(Application.persistentDataPath, "VrEfm");

    private List<ModuleNote> Notes = new List<ModuleNote>();
    public ModuleNote CurrentNote = null;

    public static VrEfmService instance = null;

    public EdgeworkHandler Edgework = new EdgeworkHandler();

    [HideInInspector]
    public GameObject ManualObject;
    [HideInInspector]
    public GameObject NoteObject;
    [HideInInspector]
    public GameObject EdgeworkObject;

    private DictationRecognizer recognizer;

    #region ManualActions
    public void MovePage(MoveDirection direction)
    {
        if (CurrentManual != null) CurrentManual = direction == MoveDirection.Previous ? CurrentManual.Previous : CurrentManual.Next;
    }

    public void Open(string module)
    {
        ManualCache.TryGetValue(module, out LinkedList<Texture2D> manual);
        CurrentManual = manual.First;
    }
    #endregion

    #region TextUpdates
    public void UpdateText(GameObject obj, string text)
    {
        obj.GetComponentInChildren<TextMesh>(true).text = text;
    }

    public void ChangeNoteText(string text)
    {
        UpdateText(NoteObject, text);
    }

    public void UpdateEdgework()
    {
        UpdateText(EdgeworkObject, Edgework.GetText());
    }
    #endregion

    public void Awake()
    {
        instance = this;
    }

    public void Start()
    {
        if (!Directory.Exists(path)) Directory.CreateDirectory(path);
        GetComponent<KMGameInfo>().OnStateChange += (state) =>
        {
            if (state == KMGameInfo.State.Gameplay)
            {
                Edgework.Clear();
                recognizer.Start();
            }
            else { recognizer.Stop(); }
        };
        var storage = GetComponent<Storage>();
        ManualObject = Instantiate(storage.ManualPrefab);
        NoteObject = Instantiate(storage.NotesPrefab);
        EdgeworkObject = Instantiate(storage.EdgeworkPrefab);
        CurrentNote = new ModuleNote();
        Notes.Add(CurrentNote);
        recognizer = new DictationRecognizer(ConfidenceLevel.Medium);
        recognizer.DictationResult += (text, confidence) => Root.ProcessCommand(text, typeof(Root));
        recognizer.DictationError += (error, hresult) =>
        {
            recognizer.Stop();
            CurrentNote.Text = $"An error occurred while setting up voice commands.\nMake sure dictation is enabled on your device (Settings->Privacy->Speech, inking & typing)\nIf it's not, please enable it and restart the bomb!\nIf it is, please contact my developer!\nError message: {error}";
        };
        recognizer.Start();
    }

    public void OnApplicationQuit()
    {
        recognizer.Stop();
        recognizer.Dispose();
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