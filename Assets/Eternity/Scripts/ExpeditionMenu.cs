using UnityEngine;
using UnityEngine.SceneManagement;

namespace Eternity {
[DefaultExecutionOrder(-500)]
public sealed class ExpeditionMenu : MonoBehaviour {
    public static bool Visible { get; private set; }
    static bool restarting;
    bool started;
    Texture2D artwork;
    int selection;
    int inputFrame;
    public static bool BlocksInput { get { return Visible || (instance && instance.inputFrame == Time.frameCount); } }
    static ExpeditionMenu instance;
    static readonly string[] choices = {"START", "RESUME", "RESTART", "QUIT"};
    readonly Color gold = new Color(.94f,.74f,.40f);
    GUIStyle heading, chapter, button, subtitle;

    void Awake() {
        instance = this;
        artwork = Resources.Load<Texture2D>("Menu/TempleAtNight");
        started = restarting;
        restarting = false;
        SetMenu(!started);
    }
    void SetMenu(bool open) {
        Visible = open;
        Time.timeScale = open ? 0 : 1;
        AudioListener.pause = open;
        Cursor.lockState = open ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = open;
        selection = started ? 1 : 0;
        inputFrame = Time.frameCount;
    }
    void OnDestroy() {
        if(instance != this) return;
        instance = null;
        Visible = false;
        Time.timeScale = 1;
        AudioListener.pause = false;
    }
    bool Available(int index) { return index == 3 || (started ? index != 0 : index == 0); }
    void Update() {
        if(Input.GetKeyDown(KeyCode.Escape) && started) { SetMenu(!Visible); return; }
        if(!Visible) return;
        int direction = Input.GetKeyDown(KeyCode.DownArrow) ? 1 : Input.GetKeyDown(KeyCode.UpArrow) ? -1 : 0;
        if(direction != 0) do {selection = (selection + direction + 4) % 4;} while(!Available(selection));
        if(Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter)) Choose(selection);
    }
    void Choose(int index) {
        if(!Available(index)) return;
        if(index <= 1) { started = true; SetMenu(false); }
        else if(index == 2) { restarting = true; SceneManager.LoadScene(SceneManager.GetActiveScene().name); }
        else {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }
    }
    static void Fill(Rect area, Color color) {
        var previous = GUI.color; GUI.color = color;
        GUI.DrawTexture(area,Texture2D.whiteTexture); GUI.color = previous;
    }
    void OnGUI() {
        if(!Visible) return;
        GUI.depth = -1000;
        if(heading == null) {
            heading = new GUIStyle(GUI.skin.label){fontSize=35,fontStyle=FontStyle.Bold,wordWrap=false};
            chapter = new GUIStyle(GUI.skin.label){fontSize=21,wordWrap=false};
            chapter.normal.textColor = gold;
            button = new GUIStyle(GUI.skin.label){fontSize=19,fontStyle=FontStyle.Bold,alignment=TextAnchor.MiddleLeft};
            subtitle = new GUIStyle(GUI.skin.label){fontSize=15};
        }
        var full = new Rect(0,0,Screen.width,Screen.height);
        Fill(full,new Color(.02f,.04f,.05f));
        if(artwork) GUI.DrawTexture(full,artwork,ScaleMode.ScaleAndCrop);
        var saved = GUI.matrix;
        float scale = Mathf.Min(Screen.width/830f,Screen.height/580f,1.7f);
        GUI.matrix = Matrix4x4.TRS(new Vector3(Screen.width*.055f,(Screen.height-516*scale)*.5f),Quaternion.identity,Vector3.one*scale);
        Fill(new Rect(0,0,343,516),new Color(.024f,.055f,.057f,.93f));
        Fill(new Rect(0,0,343,3),gold);
        subtitle.normal.textColor=gold;
        GUI.Label(new Rect(26,20,300,25),started?"EXPEDITION  /  PAUSED":"EXPEDITION  /  TEMPLE RUINS",subtitle);
        GUI.Label(new Rect(23,50,294,45),"Heart of",heading);
        GUI.Label(new Rect(23,91,294,46),"Eternity:",heading);
        GUI.Label(new Rect(24,145,308,41),"Rise of the Buried Temple",chapter);
        for(int i=0;i<4;i++) {
            var rect = new Rect(26,199+63*i,291,49);
            bool enabled = Available(i);
            bool focus = enabled && (i == selection || rect.Contains(Event.current.mousePosition));
            Fill(rect,focus?new Color(.30f,.24f,.15f):new Color(.085f,.12f,.12f));
            if(focus) Fill(new Rect(rect.x,rect.y,3,rect.height),gold);
            button.normal.textColor=enabled?Color.white:new Color(.47f,.51f,.49f);
            GUI.Label(new Rect(rect.x+15,rect.y,240,rect.height),choices[i],button);
            if(enabled && GUI.Button(rect,GUIContent.none,GUIStyle.none)) Choose(i);
        }
        GUI.Label(new Rect(26,467,304,28),"Arrow keys to select   /   Enter to confirm",subtitle);
        GUI.matrix = saved;
    }
}
}


