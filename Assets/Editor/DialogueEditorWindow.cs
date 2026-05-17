using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public class DialogueEditorWindow : EditorWindow
{
    Dictionary<string, CharacterState>
previewCharacters =
    new Dictionary<string, CharacterState>();

    bool isPlaytesting;

    DialogueNode playtestNode;

    List<GraphComment> comments =
    new List<GraphComment>();

    bool isBoxSelecting;

    Vector2 selectionStart;
    Vector2 selectionEnd;

    List<int> selectedNodes =
        new List<int>();


    float leftPanelWidth = 250f;
    float rightPanelWidth = 320f;

    bool isResizingPanels;
    bool isResizingRightPanel;

    Stack<string> undoStack =
    new Stack<string>();

    Stack<string> redoStack =
        new Stack<string>();

    bool isDraggingConnection;

    string connectionStartID = "";
    int connectionChoiceIndex = -1;

    Vector2 currentConnectionMouse;

    float graphZoom = 1f;

    Vector2 graphOffset = Vector2.zero;
    Vector2 panStart;
    Vector2 dragOffset;

    bool isPanningGraph;


    bool showGraph = true;

    bool isDraggingNode;

    bool isDraggingComment;

    int draggedComment = -1;

    Vector2 commentDragOffset;

    string draggedNodeID;


    Dictionary<string, Rect> graphNodeRects =
    new Dictionary<string, Rect>();



    Vector2 leftScroll;
    Vector2 rightScroll;

    Texture2D backgroundPreview;
    Texture2D characterPreview;

    string searchText = "";

    Rect previewRect;

    List<DialogueNode> nodes =
        new List<DialogueNode>()
    {
        new DialogueNode()
        {
            id = "day1_start",
            speaker = "Sadako",
            text = "Добро пожаловать.",

            background = "school_gate",

            music = "school_theme",

            nextNodeID = "day1_hallway",

            setCharacters =
                new List<CharacterState>()
            {
                new CharacterState()
                {
                    name = "Sadako",
                    emotion = "neutral",
                    position = "center"
                }
            }
        },

        new DialogueNode()
        {
            id = "day1_hallway",
            speaker = "Sumiko",
            text = "Привет!",

            background = "hallway_day",

            music = "school_theme",

            nextNodeID = "day1_music_club",

            setCharacters =
                new List<CharacterState>()
            {
                new CharacterState()
                {
                    name = "Sumiko",
                    emotion = "happy",
                    position = "left"
                }
            }
        }
    };

    int selectedNode = 0;

    string currentFileName =
    "day1";

    [MenuItem("Tools/Dialogue Editor")]
    public static void ShowWindow()
    {
        GetWindow<DialogueEditorWindow>(
            "Dialogue Editor");
    }

    void OnGUI()
    {
        if (nodes.Count == 0)
            return;

        Undo.RegisterCompleteObjectUndo(
    this,
    "Dialogue Change");

        DrawTopBar();

        GUILayout.BeginHorizontal();

        // LEFT

        GUILayout.BeginVertical(
            GUILayout.Width(leftPanelWidth));

        DrawLeftPanel();

        GUILayout.EndVertical();

        // RESIZER

        Rect resizeRect =
            new Rect(
                leftPanelWidth,
                20,
                6,
                position.height);

        EditorGUIUtility.AddCursorRect(
            resizeRect,
            MouseCursor.ResizeHorizontal);

        EditorGUI.DrawRect(
            resizeRect,
            new Color(1, 1, 1, 0.1f));

        Event e = Event.current;

        if (e.type == EventType.MouseDown
            && resizeRect.Contains(e.mousePosition))
        {
            isResizingPanels = true;
        }

        if (isResizingPanels)
        {
            leftPanelWidth =
                Mathf.Clamp(
                    e.mousePosition.x,
                    180,
                    position.width - 400);

            Repaint();
        }

        if (e.type == EventType.MouseUp)
        {
            isResizingPanels = false;
        }

        // GRAPH

        if (showGraph)
        {
            GUILayout.BeginVertical(
                GUILayout.ExpandWidth(true),
                GUILayout.ExpandHeight(true));

            DrawMiniGraph();

            GUILayout.EndVertical();
        }

        // RIGHT RESIZER

        // RIGHT RESIZER

        if (showGraph)
        {
            Rect rightResizeRect =
                new Rect(
                    position.width - rightPanelWidth - 6,
                    20,
                    6,
                    position.height);

            EditorGUIUtility.AddCursorRect(
                rightResizeRect,
                MouseCursor.ResizeHorizontal);

            EditorGUI.DrawRect(
                rightResizeRect,
                new Color(1, 1, 1, 0.1f));

            if (e.type == EventType.MouseDown
                && rightResizeRect.Contains(e.mousePosition))
            {
                isResizingPanels = false;
                isResizingRightPanel = true;
            }

            if (isResizingRightPanel)
            {
                rightPanelWidth =
                    Mathf.Clamp(
                        position.width - e.mousePosition.x,
                        250,
                        700);

                Repaint();
            }

            if (e.type == EventType.MouseUp)
            {
                isResizingRightPanel = false;
            }
        }

        // RIGHT

        GUILayout.BeginVertical(
    showGraph
        ? GUILayout.Width(rightPanelWidth)
        : GUILayout.ExpandWidth(true));

        DrawRightPanel();

        GUILayout.EndVertical();

        GUILayout.EndHorizontal();


        if (e.type == EventType.KeyDown &&
            e.control &&
            e.keyCode == KeyCode.S)
        {
            SaveJSON();

            Repaint();
        }

    }

    void DrawTopBar()
    {
        GUILayout.BeginHorizontal(
            EditorStyles.toolbar);

        GUILayout.Label(
            "File:",
            GUILayout.Width(30));

        currentFileName =
            GUILayout.TextField(
                currentFileName,
                GUILayout.Width(150));

        if (GUILayout.Button(
            "Save",
            EditorStyles.toolbarButton,
            GUILayout.Width(60)))
        {
            SaveJSON();
        }

        if (GUILayout.Button(
            "Load",
            EditorStyles.toolbarButton,
            GUILayout.Width(60)))
        {
            LoadJSON();
        }

        if (GUILayout.Button(
    "Add Comment",
    EditorStyles.toolbarButton,
    GUILayout.Width(100)))
        {
            comments.Add(
                new GraphComment()
                {
                    position =
                        new Vector2(300, 300),

                    size =
                        new Vector2(300, 120),

                    text =
                        "TODO"
                });
        }

        if (GUILayout.Button(
    showGraph
    ? "Hide Graph"
    : "Show Graph",
    EditorStyles.toolbarButton,
    GUILayout.Width(100)))
        {
            showGraph = !showGraph;
        }

        GUILayout.FlexibleSpace();

        GUILayout.EndHorizontal();
    }


    // =========================
    // LEFT PANEL
    // =========================

    void DrawLeftPanel()
    {
        GUILayout.BeginVertical();

        GUILayout.Label(
            "Nodes",
            EditorStyles.boldLabel);

        GUILayout.Space(5);

        searchText =
            EditorGUILayout.TextField(
                "Search",
                searchText);

        GUILayout.Space(10);

        leftScroll =
            GUILayout.BeginScrollView(leftScroll);

        for (int i = 0; i < nodes.Count; i++)
        {

            DialogueNode node =
    nodes[i];

            bool matchesSearch =
                string.IsNullOrEmpty(searchText)
                ||
                node.id.ToLower().Contains(
                    searchText.ToLower())
                ||
                node.text.ToLower().Contains(
                    searchText.ToLower())
                ||
                node.speaker.ToLower().Contains(
                    searchText.ToLower());

            if (!matchesSearch)
            {
                continue;
            }

            if (GUILayout.Button(nodes[i].id))
            {
                selectedNode = i;

                previewCharacters.Clear();
            }
        }

        GUILayout.EndScrollView();

        if (GUILayout.Button("+ Add Node"))
        {
            SaveUndoState();

            DialogueNode previous =
    nodes[selectedNode];

            DialogueNode newNode =
                new DialogueNode()
                {
                    id = GenerateNodeID(),

                    editorPosition =
    previous.editorPosition
    + new Vector2(320, 0),

                    speaker = previous.speaker,

                    text = "...",

                    background = previous.background,

                    music = previous.music,

                    nextNodeID = "",

                    clearCharacters =
                    previous.clearCharacters,

                    hideCharacters =
                    new List<string>(
                        previous.hideCharacters),

                    setCharacters =
                    new List<CharacterState>()
                };

            foreach (CharacterState character
                     in previous.setCharacters)
            {
                newNode.setCharacters.Add(
                    new CharacterState()
                    {
                        name = character.name,
                        emotion = character.emotion,
                        position = character.position
                    });
            }

            nodes.Add(newNode);

            selectedNode =
                nodes.Count - 1;
        }
        // =========================
        // CREATE CONNECTED NODE
        // =========================

        GUI.backgroundColor =
            new Color(
                0.3f,
                0.7f,
                1f);

        if (GUILayout.Button(
            "+ Create Connected Node"))
        {
            DialogueNode previous =
                nodes[selectedNode];

            DialogueNode newNode =
                new DialogueNode()
                {
                    id =
                        "node_" +
                        nodes.Count,

                    editorPosition =
    previous.editorPosition
    + new Vector2(320, 0),

                    speaker =
                        previous.speaker,

                    text = "...",

                    background =
                        previous.background,

                    music =
                        previous.music,

                    nextNodeID = "",

                    clearCharacters =
                        previous.clearCharacters,

                    hideCharacters =
                        new List<string>(
                            previous.hideCharacters),

                    setCharacters =
                        new List<CharacterState>(),

                    choices =
                        new List<DialogueChoice>()
                };

            // copy characters

            foreach (CharacterState character
                     in previous.setCharacters)
            {
                newNode.setCharacters.Add(
                    new CharacterState()
                    {
                        name =
                            character.name,

                        emotion =
                            character.emotion,

                        position =
                            character.position
                    });
            }

            // AUTO LINK

            previous.nextNodeID =
                newNode.id;

            // add node

            nodes.Add(newNode);

            selectedNode =
                nodes.Count - 1;
        }

        GUI.backgroundColor =
            Color.white;
        GUILayout.EndVertical();
    }

    // =========================
    // RIGHT PANEL
    // =========================

    void DrawRightPanel()
    {
        DialogueNode node =
    isPlaytesting
    ? playtestNode
    : nodes[selectedNode];

        // =========================
        // SAFETY
        // =========================

        if (node.setCharacters == null)
        {
            node.setCharacters =
                new List<CharacterState>();
        }

        if (node.setCharacters.Count == 0)
        {
            node.setCharacters.Add(
                new CharacterState()
                {
                    name = node.speaker,
                    emotion = "neutral",
                    position = "center"
                });
        }

        GUILayout.BeginVertical();

        GUILayout.Label(
            "Node Editor",
            EditorStyles.boldLabel);

        rightScroll =
            GUILayout.BeginScrollView(rightScroll);

        // =========================
        // NODE ID
        // =========================

        GUILayout.Label("Node ID");

        node.id =
            EditorGUILayout.TextField(
                node.id);

        GUILayout.Space(10);

        // =========================
        // CHARACTERS
        // =========================

        node.speaker =
    EditorGUILayout.TextField(
        "Speaker",
        node.speaker);

        GUILayout.Space(10);

        GUILayout.Label(
            "Characters",
            EditorStyles.boldLabel);

        GUILayout.Space(5);

        for (int i = 0;
             i < node.setCharacters.Count;
             i++)
        {
            CharacterState character =
                node.setCharacters[i];

            GUILayout.BeginVertical("box");

            // =========================
            // CHARACTER DROPDOWN
            // =========================

            string charactersPath =
                "Assets/Resources/Sprites/Characters";

            string[] characterFolders =
                Directory.GetDirectories(
                    charactersPath);

            List<string> characterNames =
                new List<string>();

            foreach (string folder
                     in characterFolders)
            {
                characterNames.Add(
                    Path.GetFileName(folder));
            }

            int currentCharacterIndex =
                characterNames.IndexOf(
                    character.name);

            if (currentCharacterIndex < 0)
            {
                currentCharacterIndex = 0;
            }

            currentCharacterIndex =
                EditorGUILayout.Popup(
                    "Character",
                    currentCharacterIndex,
                    characterNames.ToArray());

            character.name =
                characterNames[currentCharacterIndex];

            // =========================
            // EMOTION DROPDOWN
            // =========================

            string emotionsPath =
                charactersPath + "/" +
                character.name;

            string[] emotionFiles =
                Directory.GetFiles(
                    emotionsPath,
                    "*.png");

            List<string> emotions =
                new List<string>();

            foreach (string file
                     in emotionFiles)
            {
                emotions.Add(
                    Path.GetFileNameWithoutExtension(
                        file));
            }

            int currentEmotionIndex =
                emotions.IndexOf(
                    character.emotion);

            if (currentEmotionIndex < 0)
            {
                currentEmotionIndex = 0;
            }

            currentEmotionIndex =
                EditorGUILayout.Popup(
                    "Emotion",
                    currentEmotionIndex,
                    emotions.ToArray());

            character.emotion =
                emotions[currentEmotionIndex];

            // =========================
            // POSITION DROPDOWN
            // =========================

            string[] positions =
            {
    "left",
    "center",
    "right"
};

            int currentPosition =
                System.Array.IndexOf(
                    positions,
                    character.position);

            if (currentPosition < 0)
            {
                currentPosition = 0;
            }

            currentPosition =
                EditorGUILayout.Popup(
                    "Position",
                    currentPosition,
                    positions);

            character.position =
                positions[currentPosition];

            GUILayout.Space(5);

            GUI.backgroundColor =
                Color.red;

            if (GUILayout.Button(
                "Remove Character"))
            {
                node.setCharacters.RemoveAt(i);

                break;
            }

            GUI.backgroundColor =
                Color.white;

            GUILayout.EndVertical();

            GUILayout.Space(5);
        }

        // =========================
        // ADD CHARACTER
        // =========================

        GUI.backgroundColor =
            Color.green;

        if (GUILayout.Button(
            "+ Add Character"))
        {
            node.setCharacters.Add(
                new CharacterState()
                {
                    name = "Sadako",
                    emotion = "neutral",
                    position = "center"
                });
        }

        GUI.backgroundColor =
            Color.white;

        GUILayout.Space(10);

        // =========================
        // TEXT
        // =========================

        GUILayout.Label("Dialogue");

        node.text =
            EditorGUILayout.TextArea(
                node.text,
                GUILayout.Height(150));

        // =========================
        // HIDE CHARACTERS
        // =========================

        GUILayout.Space(10);

        // =========================
        // CLEAR CHARACTERS
        // =========================

        node.clearCharacters =
            EditorGUILayout.Toggle(
                "Clear All Characters",
                node.clearCharacters);

        GUILayout.Space(10);

        GUILayout.Label(
            "Hide Characters",
            EditorStyles.boldLabel);

        if (node.hideCharacters == null)
        {
            node.hideCharacters =
                new List<string>();
        }

        // draw hidden characters

        for (int i = 0;
             i < node.hideCharacters.Count;
             i++)
        {
            GUILayout.BeginHorizontal();

            node.hideCharacters[i] =
                EditorGUILayout.TextField(
                    node.hideCharacters[i]);

            GUI.backgroundColor = Color.red;

            if (GUILayout.Button(
                "X",
                GUILayout.Width(30)))
            {
                node.hideCharacters.RemoveAt(i);

                break;
            }

            GUI.backgroundColor = Color.white;

            GUILayout.EndHorizontal();
        }

        // add hide character

        GUI.backgroundColor = Color.green;

        if (GUILayout.Button(
            "+ Hide Character"))
        {
            node.hideCharacters.Add("");
        }

        GUI.backgroundColor = Color.white;


        GUILayout.Space(10);
        // =========================
        // BACKGROUND
        // =========================

        string backgroundsPath =
    "Assets/Resources/Sprites/Backgrounds";

        string[] backgroundFiles =
            Directory.GetFiles(
                backgroundsPath,
                "*.png");

        List<string> backgrounds =
            new List<string>();

        foreach (string file in backgroundFiles)
        {
            backgrounds.Add(
                Path.GetFileNameWithoutExtension(
                    file));
        }

        int currentBackgroundIndex =
            backgrounds.IndexOf(
                node.background);

        if (currentBackgroundIndex < 0)
        {
            currentBackgroundIndex = 0;
        }

        currentBackgroundIndex =
            EditorGUILayout.Popup(
                "Background",
                currentBackgroundIndex,
                backgrounds.ToArray());

        node.background =
            backgrounds[currentBackgroundIndex];

        // =========================
        // MUSIC
        // =========================

        string musicPath =
            "Assets/Resources/Audio/Music";

        List<string> musicTracks =
            new List<string>();

        if (Directory.Exists(musicPath))
        {
            string[] musicFiles =
                Directory.GetFiles(
                    musicPath);

            foreach (string file in musicFiles)
            {
                string extension =
                    Path.GetExtension(file)
                    .ToLower();

                if (extension == ".mp3"
                    || extension == ".wav"
                    || extension == ".ogg")
                {
                    musicTracks.Add(
                        Path.GetFileNameWithoutExtension(
                            file));
                }
            }
        }

        // SAFETY

        if (musicTracks.Count == 0)
        {
            musicTracks.Add("None");
        }

        int currentMusicIndex =
            musicTracks.IndexOf(
                node.music);

        if (currentMusicIndex < 0)
        {
            currentMusicIndex = 0;
        }

        currentMusicIndex =
            EditorGUILayout.Popup(
                "Music",
                currentMusicIndex,
                musicTracks.ToArray());

        node.music =
            musicTracks[currentMusicIndex];


        // =========================
        // CHOICES
        // =========================

        GUILayout.Space(20);

        GUILayout.Label(
            "Choices",
            EditorStyles.boldLabel);

        // создать список если null

        if (node.choices == null)
        {
            node.choices =
                new List<DialogueChoice>();
        }

        // =========================
        // DRAW CHOICES
        // =========================

        for (int i = 0; i < node.choices.Count; i++)
        {
            DialogueChoice choice =
                node.choices[i];

            GUILayout.BeginVertical("box");

            GUILayout.Label(
                "Choice " + (i + 1),
                EditorStyles.boldLabel);

            // TEXT

            choice.text =
                EditorGUILayout.TextField(
                    "Text",
                    choice.text);

            // NEXT NODE

            choice.nextNodeID =
                EditorGUILayout.TextField(
                    "Next Node",
                    choice.nextNodeID);

            // =========================
            // CREATE CHOICE BRANCH
            // =========================

            GUI.backgroundColor =
                new Color(
                    0.3f,
                    0.6f,
                    1f);

            if (GUILayout.Button(
                "Create Branch"))
            {
                DialogueNode currentNode =
                    nodes[selectedNode];

                DialogueNode newNode =
                    new DialogueNode()
                    {
                        id = GenerateNodeID(),

                        editorPosition =
    currentNode.editorPosition
    + new Vector2(320, 160),

                        speaker =
                            currentNode.speaker,

                        text = "...",

                        background =
                            currentNode.background,

                        music =
                            currentNode.music,

                        nextNodeID = "",

                        clearCharacters =
                            currentNode.clearCharacters,

                        hideCharacters =
                            new List<string>(
                                currentNode.hideCharacters),

                        setCharacters =
                            new List<CharacterState>(),

                        choices =
                            new List<DialogueChoice>()
                    };

                // copy characters

                foreach (CharacterState character
                         in currentNode.setCharacters)
                {
                    newNode.setCharacters.Add(
                        new CharacterState()
                        {
                            name =
                                character.name,

                            emotion =
                                character.emotion,

                            position =
                                character.position
                        });
                }

                // AUTO CONNECT CHOICE

                choice.nextNodeID =
                    newNode.id;

                // add node

                nodes.Add(newNode);

                selectedNode =
                    nodes.Count - 1;
            }

            GUI.backgroundColor =
                Color.white;

            GUILayout.Space(5);

            // RELATIONSHIPS

            choice.sadakoChange =
                EditorGUILayout.IntField(
                    "Sadako Change",
                    choice.sadakoChange);

            choice.sumikoChange =
                EditorGUILayout.IntField(
                    "Sumiko Change",
                    choice.sumikoChange);

            choice.terukoChange =
                EditorGUILayout.IntField(
                    "Teruko Change",
                    choice.terukoChange);

            GUILayout.Space(10);

            GUI.backgroundColor = Color.red;

            if (GUILayout.Button("Delete Choice"))
            {
                node.choices.RemoveAt(i);

                break;
            }

            GUI.backgroundColor = Color.white;

            GUILayout.EndVertical();

            GUILayout.Space(10);
        }

        // =========================
        // ADD CHOICE
        // =========================

        GUI.backgroundColor = Color.green;

        if (GUILayout.Button("+ Add Choice"))
        {
            node.choices.Add(
                new DialogueChoice()
                {
                    text = "New Choice",
                    nextNodeID = ""
                });
        }

        GUI.backgroundColor = Color.white;

        // =========================
        // NEXT NODE DROPDOWN
        // =========================

        List<string> nodeIDs =
            new List<string>();

        nodeIDs.Add("");

        // собрать все id

        // только ноды после текущей

        for (int i = selectedNode + 1;
             i < nodes.Count;
             i++)
        {
            nodeIDs.Add(nodes[i].id);
        }

        // найти текущий index

        int currentIndex =
            nodeIDs.IndexOf(
                node.nextNodeID);

        if (currentIndex < 0)
        {
            currentIndex = 0;
        }

        // dropdown

        currentIndex =
            EditorGUILayout.Popup(
                "Next Node",
                currentIndex,
                nodeIDs.ToArray());

        // сохранить выбранный id

        node.nextNodeID =
            nodeIDs[currentIndex];

        // =========================
        // PREVIEW
        // =========================

        GUILayout.Space(20);

        GUILayout.Label(
            "Preview",
            EditorStyles.boldLabel);

        GUILayout.BeginHorizontal();

        if (!isPlaytesting)
        {
            if (GUILayout.Button("▶ Play"))
            {
                if (selectedNode >= 0
                    && selectedNode < nodes.Count)
                {
                    playtestNode =
                        nodes[selectedNode];

                    isPlaytesting = true;
                }
            }
        }
        else
        {
            if (GUILayout.Button("■ Stop"))
            {
                isPlaytesting = false;
            }
        }

        GUILayout.EndHorizontal();

        // =========================
        // LOAD BACKGROUND
        // =========================

        string bgPath =
            "Assets/Resources/Sprites/Backgrounds/" +
            node.background +
            ".png";

        backgroundPreview =
            AssetDatabase.LoadAssetAtPath<Texture2D>(
                bgPath);

        // =========================
        // LOAD CHARACTER
        // =========================

        characterPreview = null;

        if (node.setCharacters != null &&
            node.setCharacters.Count > 0)
        {
            CharacterState character =
                node.setCharacters[0];

            string spritePath =
                "Assets/Resources/Sprites/Characters/" +
                character.name + "/" +
                character.emotion +
                ".png";

            characterPreview =
                AssetDatabase.LoadAssetAtPath<Texture2D>(
                    spritePath);
        }

        // =========================
        // VN FRAME
        // =========================

        previewRect =
    GUILayoutUtility.GetRect(
        800,
        750);

        // background

        if (backgroundPreview != null)
        {
            GUI.DrawTexture(
                previewRect,
                backgroundPreview,
                ScaleMode.ScaleAndCrop);
        }

        // =========================
        // PREVIEW SCENE STATE
        // =========================

        // clear all

        if (node.clearCharacters)
        {
            previewCharacters.Clear();
        }

        // hide characters

        if (node.hideCharacters != null)
        {
            foreach (string hiddenCharacter
                     in node.hideCharacters)
            {
                if (previewCharacters.ContainsKey(
                        hiddenCharacter))
                {
                    previewCharacters.Remove(
                        hiddenCharacter);
                }
            }
        }

        // apply current node characters

        if (node.setCharacters != null)
        {
            foreach (CharacterState character
                     in node.setCharacters)
            {
                previewCharacters[character.name] =
                    character;
            }
        }

        // draw all active characters

        foreach (CharacterState character
                 in previewCharacters.Values)
        {
            string spritePath =
                "Assets/Resources/Sprites/Characters/" +
                character.name + "/" +
                character.emotion +
                ".png";

            Texture2D previewSprite =
                AssetDatabase.LoadAssetAtPath<Texture2D>(
                    spritePath);

            if (previewSprite == null)
                continue;

            float characterX =
                previewRect.x + 300;

            switch (character.position)
            {
                case "left":
                    characterX =
                        previewRect.x + 40;
                    break;

                case "center":
                    characterX =
                        previewRect.x + 300;
                    break;

                case "right":
                    characterX =
                        previewRect.x + 560;
                    break;
            }

            Rect characterRect =
                new Rect(
                    characterX,
                    previewRect.y + 40,
                    220,
                    360);

            GUI.DrawTexture(
                characterRect,
                previewSprite,
                ScaleMode.ScaleToFit,
                true);
        }

        // dialogue box

        Rect dialogueRect =
    new Rect(
        previewRect.x,
        previewRect.y + 450,
        previewRect.width,
        220);

        EditorGUI.DrawRect(
            dialogueRect,
            new Color(0, 0, 0, 0.85f));

        // speaker style

        GUIStyle speakerStyle =
            new GUIStyle(EditorStyles.boldLabel);

        speakerStyle.fontSize = 18;

        speakerStyle.normal.textColor =
            Color.white;

        // text style

        GUIStyle textStyle =
            new GUIStyle(EditorStyles.label);

        textStyle.wordWrap = true;

        textStyle.fontSize = 16;

        textStyle.normal.textColor =
            Color.white;

        // speaker

        GUI.Label(
            new Rect(
                dialogueRect.x + 20,
                dialogueRect.y + 10,
                300,
                30),
            node.speaker,
            speakerStyle);

        // text

        GUI.Label(
            new Rect(
                dialogueRect.x + 20,
                dialogueRect.y + 45,
                dialogueRect.width - 40,
                60),
            node.text,
            textStyle);

        // =========================
        // CHOICE PREVIEW
        // =========================

        if (node.choices != null &&
            node.choices.Count > 0)
        {
            float choiceY =
                dialogueRect.y + 140;

            foreach (DialogueChoice choice
                     in node.choices)
            {
                Rect choiceRect =
                    new Rect(
                        dialogueRect.x + 40,
                        choiceY,
                        dialogueRect.width - 80,
                        35);

                EditorGUI.DrawRect(
                    choiceRect,
                    new Color(
                        0,
                        0,
                        0,
                        0.8f));

                GUI.Label(
                    new Rect(
                        choiceRect.x + 10,
                        choiceRect.y + 8,
                        choiceRect.width - 20,
                        20),
                    "> " + choice.text,
                    textStyle);

                choiceY += 45;
            }
        }

        // =========================
        // PLAYTEST CHOICES
        // =========================

        if (isPlaytesting
            && node.choices != null)
        {
            GUILayout.Space(10);

            foreach (DialogueChoice choice
                     in node.choices)
            {
                if (GUILayout.Button(choice.text))
                {
                    DialogueNode next =
                        nodes.Find(
                            n => n.id
                            == choice.nextNodeID);

                    if (next != null)
                    {
                        playtestNode = next;
                    }
                }
            }
        }

        // =========================
        // PLAYTEST CONTINUE
        // =========================

        if (isPlaytesting
            && (node.choices == null
                || node.choices.Count == 0))
        {
            if (!string.IsNullOrEmpty(
                    node.nextNodeID))
            {
                GUILayout.Space(10);

                if (GUILayout.Button("Continue"))
                {
                    DialogueNode next =
                        nodes.Find(
                            n => n.id
                            == node.nextNodeID);

                    if (next != null)
                    {
                        playtestNode = next;
                    }
                }
            }
        }

        GUILayout.Space(20);
        // =========================
        // DUPLICATE NODE
        // =========================

        GUI.backgroundColor =
            new Color(
                0.3f,
                0.6f,
                1f);

        if (GUILayout.Button(
            "Duplicate Node",
            GUILayout.Height(35)))
        {
            DialogueNode original =
                nodes[selectedNode];

            DialogueNode duplicate =
                new DialogueNode()
                {
                    id = GenerateNodeID(),

                    speaker =
                        original.speaker,

                    text =
                        original.text,

                    background =
                        original.background,

                    music =
                        original.music,

                    nextNodeID =
                        original.nextNodeID,

                    clearCharacters =
                        original.clearCharacters,

                    hideCharacters =
                        new List<string>(
                            original.hideCharacters),

                    choices =
                        new List<DialogueChoice>(),

                    setCharacters =
                        new List<CharacterState>()
                };

            // copy characters

            foreach (CharacterState character
                     in original.setCharacters)
            {
                duplicate.setCharacters.Add(
                    new CharacterState()
                    {
                        name =
                            character.name,

                        emotion =
                            character.emotion,

                        position =
                            character.position
                    });
            }

            // copy choices

            foreach (DialogueChoice choice
                     in original.choices)
            {
                duplicate.choices.Add(
                    new DialogueChoice()
                    {
                        text =
                            choice.text,

                        nextNodeID =
                            choice.nextNodeID,

                        sadakoChange =
                            choice.sadakoChange,

                        sumikoChange =
                            choice.sumikoChange,

                        terukoChange =
                            choice.terukoChange
                    });
            }

            nodes.Add(duplicate);

            selectedNode =
                nodes.Count - 1;
        }

        GUI.backgroundColor =
            Color.white;
        // =========================
        // DELETE NODE
        // =========================

        GUILayout.Space(20);



        GUI.backgroundColor =
            Color.red;

        if (GUILayout.Button(
            "Delete Node",
            GUILayout.Height(40)))
        {
            if (EditorUtility.DisplayDialog(
                "Delete Node",
                "Удалить ноду?",
                "Delete",
                "Cancel"))
            {
                nodes.RemoveAt(selectedNode);

                if (nodes.Count == 0)
                {
                    AddEmptyNode();
                }

                selectedNode =
                    Mathf.Clamp(
                        selectedNode - 1,
                        0,
                        nodes.Count - 1);
            }
        }

        GUI.backgroundColor =
            Color.white;

        GUILayout.EndScrollView();

        GUILayout.EndVertical();
    }

    // =========================
    // SAVE JSON
    // =========================

    void SaveJSON()
    {
        DialogueContainer container =
            new DialogueContainer();

        container.nodes = nodes;

        string json =
            JsonUtility.ToJson(
                container,
                true);

        string folder =
            "Assets/Resources/Dialogues/";

        // создать папку если нет

        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        string path =
            folder +
            currentFileName +
            ".json";

        File.WriteAllText(
            path,
            json);

        AssetDatabase.Refresh();

        Debug.Log(
            "Saved JSON: " + path);
    }

    // =========================
    // LOAD JSON
    // =========================

    void LoadJSON()
    {
        string path =
            "Assets/Resources/Dialogues/" +
            currentFileName +
            ".json";

        if (!File.Exists(path))
        {
            Debug.LogError(
                "File not found: " + path);

            return;
        }

        string json =
            File.ReadAllText(path);

        DialogueContainer container =
            JsonUtility.FromJson<DialogueContainer>(
                json);

        nodes = container.nodes;

        selectedNode = 0;

        Debug.Log(
            "Loaded JSON: " + path);
    }

    void SaveUndoState()
    {
        DialogueContainer container =
            new DialogueContainer();

        container.nodes = nodes;

        string json =
            JsonUtility.ToJson(container);

        undoStack.Push(json);

        // clear redo after new action

        redoStack.Clear();



    }

    void PerformUndo()
    {
        if (undoStack.Count == 0)
            return;

        DialogueContainer current =
            new DialogueContainer();

        current.nodes = nodes;

        redoStack.Push(
            JsonUtility.ToJson(current));

        string json =
            undoStack.Pop();

        DialogueContainer container =
            JsonUtility.FromJson<DialogueContainer>(
                json);

        nodes = container.nodes;

        Repaint();
    }

    void PerformRedo()
    {
        if (redoStack.Count == 0)
            return;

        DialogueContainer current =
            new DialogueContainer();

        current.nodes = nodes;

        undoStack.Push(
            JsonUtility.ToJson(current));

        string json =
            redoStack.Pop();

        DialogueContainer container =
            JsonUtility.FromJson<DialogueContainer>(
                json);

        nodes = container.nodes;

        Repaint();
    }

    // =========================
    // ADD EMPTY NODE
    // =========================

    void AddEmptyNode()
    {
        nodes.Add(
            new DialogueNode()
            {
                id = GenerateNodeID(),

                speaker = "Narrator",

                text = "...",

                background = "",

                music = "",

                nextNodeID = "",

                setCharacters =
                    new List<CharacterState>()
                {
                new CharacterState()
                {
                    name = "Narrator",
                    emotion = "neutral",
                    position = "center"
                }
                }
            });
    }

    // =========================
    // MINI GRAPH VIEW
    // =========================

    void DrawMiniGraph()
    {
        GUILayout.BeginVertical(
    "box",
    GUILayout.ExpandWidth(true));



        GUILayout.Label(
            "Graph View",
            EditorStyles.boldLabel);

        Rect graphArea =
    GUILayoutUtility.GetRect(
        0,
        10000,
        0,
        10000,
        GUILayout.ExpandWidth(true),
        GUILayout.ExpandHeight(true));



        GUILayout.Space(10);

        GUI.BeginGroup(graphArea);

        // =========================
        // GRID BACKGROUND
        // =========================

        DrawGrid(
            graphArea,
            20,
            0.2f,
            new Color(0.3f, 0.3f, 0.3f));

        DrawGrid(
            graphArea,
            100,
            0.35f,
            new Color(0.4f, 0.4f, 0.4f));



        Event e = Event.current;

        // =========================
        // BOX SELECTION START
        // =========================

        if (e.type == EventType.MouseDown
            && e.button == 0)
        {
            bool clickedSomething = false;

            bool clickedConnectionPoint = false;

            foreach (Rect rect
         in graphNodeRects.Values)
            {
                if (rect.Contains(e.mousePosition))
                {
                    clickedSomething = true;
                    break;
                }
            }

            // CHECK CONNECTION POINTS

            foreach (DialogueNode node in nodes)
            {
                if (!graphNodeRects.ContainsKey(node.id))
                    continue;

                Rect nodeRect =
                    graphNodeRects[node.id];

                // MAIN CONNECTION

                Rect mainPoint =
                    new Rect(
                        nodeRect.xMax - 8 * graphZoom,
                        nodeRect.center.y - 6 * graphZoom,
                        12 * graphZoom,
                        12 * graphZoom);

                if (mainPoint.Contains(e.mousePosition))
                {
                    clickedConnectionPoint = true;
                    break;
                }

                // CHOICE CONNECTIONS

                if (node.choices != null)
                {
                    float yOffset = 75 * graphZoom;

                    foreach (DialogueChoice choice in node.choices)
                    {
                        float pointSize = 12 * graphZoom;
                        Rect choicePoint =
                            new Rect(
                                nodeRect.xMax - pointSize * 0.7f,
                                nodeRect.y + yOffset + 2,
                                pointSize,
                                pointSize);

                        if (choicePoint.Contains(e.mousePosition))
                        {
                            clickedConnectionPoint = true;
                            break;
                        }

                        yOffset += 20;
                    }
                }
            }

            // COMMENTS

            for (int i = 0; i < comments.Count; i++)
            {
                GraphComment comment =
                    comments[i];

                Vector2 commentScreenPos =
                    (comment.position * graphZoom)
                    + graphOffset;

                Rect commentRect =
                    new Rect(
                        commentScreenPos.x,
                        commentScreenPos.y,
                        comment.size.x * graphZoom,
                        comment.size.y * graphZoom);

                if (commentRect.Contains(e.mousePosition))
                {
                    clickedSomething = true;
                    break;
                }
            }

            if (!clickedSomething
    && !clickedConnectionPoint
    && !isDraggingConnection)
            {
                isBoxSelecting = true;

                selectionStart =
                    e.mousePosition;

                selectionEnd =
                    e.mousePosition;

                selectedNodes.Clear();
            }
        }

        Vector2 zoomMouse =
    (e.mousePosition - graphOffset)
    / graphZoom;

        if (isDraggingConnection)
        {
            currentConnectionMouse =
                e.mousePosition;

            Repaint();
        }

        // =========================
        // BOX SELECTION DRAG
        // =========================

        if (isBoxSelecting
            && e.type == EventType.MouseDrag)
        {
            selectionEnd =
                e.mousePosition;

            Repaint();
        }

        // =========================
        // GRAPH ZOOM
        // =========================

        if (e.type == EventType.ScrollWheel)
        {
            float oldZoom = graphZoom;

            graphZoom -= e.delta.y * 0.05f;

            graphZoom =
                Mathf.Clamp(
                    graphZoom,
                    0.4f,
                    2.0f);

            Vector2 mouse =
                e.mousePosition;

            Vector2 delta =
                (mouse - graphOffset) *
                (1 - graphZoom / oldZoom);

            graphOffset += delta;

            e.Use();

            Repaint();
        }

        // =========================
        // GRAPH PAN
        // =========================

        if (e.button == 2 &&
    e.type == EventType.MouseDown)
        {
            isPanningGraph = true;

            panStart = e.mousePosition;
        }

        if (isPanningGraph &&
            e.type == EventType.MouseDrag)
        {
            graphOffset +=
                e.mousePosition - panStart;

            panStart =
                e.mousePosition;

            Repaint();
        }

        if (e.button == 2 &&
            e.type == EventType.MouseUp)
        {
            isPanningGraph = false;
        }

        foreach (DialogueNode node in nodes)
        {
            if (node.editorPosition == Vector2.zero)
            {
                node.editorPosition =
                    new Vector2(
                        20,
                        40 + nodes.IndexOf(node) * 80);
            }

            Vector2 worldPos =
    node.editorPosition;

            Vector2 screenPos =
    (worldPos * graphZoom)
    + graphOffset;



            float nodeHeight = 40;

            if (node.choices != null)
            {
                nodeHeight +=
                    node.choices.Count * 20;
            }

            Rect nodeRect =
    new Rect(
        screenPos.x,
        screenPos.y,
        220 * graphZoom,
        nodeHeight * graphZoom);



            if (isDraggingNode
    && draggedNodeID == node.id
    && e.type == EventType.MouseDrag)
            {
                if (isDraggingNode)
                {
                    Vector2 newPos =
                        zoomMouse - dragOffset;

                    Vector2 delta =
                        newPos
                        - nodes[selectedNode]
                            .editorPosition;

                    // MOVE MAIN NODE

                    nodes[selectedNode]
                        .editorPosition =
                            newPos;

                    // MOVE MULTI-SELECTION

                    foreach (int index
                             in selectedNodes)
                    {
                        if (index == selectedNode)
                            continue;

                        nodes[index]
                            .editorPosition += delta;
                    }

                    Repaint();
                }

                Repaint();
            }

            // =========================
            // COMMENTS
            // =========================

            for (int i = 0;
                 i < comments.Count;
                 i++)
            {
                GraphComment comment =
                    comments[i];

                Vector2 commentScreenPos =
    (comment.position * graphZoom)
    + graphOffset;

                Rect rect =
                    new Rect(
                        commentScreenPos.x,
                        commentScreenPos.y,
                        comment.size.x * graphZoom,
                        comment.size.y * graphZoom);

                // COMMENT DRAG START

                Rect dragRect =
    new Rect(
        rect.x,
        rect.y,
        rect.width,
        24);

                EditorGUI.DrawRect(
                    dragRect,
                    new Color(
                        0f,
                        0f,
                        0f,
                        0.2f));

                GUI.Label(
                    new Rect(
                        dragRect.x + 8,
                        dragRect.y + 4,
                        100,
                        20),
                    "Comment");

                if (e.type == EventType.MouseDown
                    && e.button == 0
                    && dragRect.Contains(e.mousePosition))
                {
                    draggedComment = i;

                    isDraggingComment = true;

                    commentDragOffset =
                        zoomMouse
                        - comment.position;

                    e.Use();
                }

                EditorGUI.DrawRect(
                    rect,
                    new Color(
                        1f,
                        0.95f,
                        0.3f,
                        0.25f));

                GUI.Box(rect, "");

                Rect textRect =
    new Rect(
        rect.x + 4,
        rect.y + 28,
        rect.width - 8,
        rect.height - 32);

                GUILayout.BeginArea(textRect);

                comment.text =
                    EditorGUILayout.TextArea(
                        comment.text,
                        GUILayout.ExpandHeight(true));

                GUILayout.EndArea();

                // COMMENT DRAG

                if (isDraggingComment
                    && draggedComment == i
                    && e.type == EventType.MouseDrag)
                {
                    comments[i].position =
    zoomMouse
    - commentDragOffset;

                    Repaint();
                }
            }

            // =========================
            // NODE COLORS
            // =========================

            Color nodeColor =
                Color.gray;

            // START NODE

            if (node.id.ToLower().Contains("start"))
            {
                nodeColor =
                    new Color(
                        0.3f,
                        0.4f,
                        0.8f);
            }

            // END NODE

            else if (
                string.IsNullOrEmpty(
                    node.nextNodeID)
                &&
                (node.choices == null ||
                 node.choices.Count == 0))
            {
                nodeColor =
                    new Color(
                        0.8f,
                        0.4f,
                        0.4f);
            }

            // CHOICE NODE

            else if (
                node.choices != null &&
                node.choices.Count > 0)
            {
                nodeColor =
                    new Color(
                        1f,
                        0.8f,
                        0.1f);
            }

            // NORMAL NODE

            else
            {
                nodeColor =
                    new Color(
                        0.4f,
                        0.9f,
                        0.4f);
            }

            // SELECTED OUTLINE

            if (nodes.IndexOf(node) == selectedNode
    || selectedNodes.Contains(
        nodes.IndexOf(node)))
            {
                Rect outlineRect =
                    new Rect(
                        nodeRect.x - 3,
                        nodeRect.y - 3,
                        nodeRect.width + 6,
                        nodeRect.height + 6);

                EditorGUI.DrawRect(
                    outlineRect,
                    new Color(
                        1f,
                        1f,
                        1f,
                        0.9f));
            }

            // NODE BODY

            EditorGUI.DrawRect(
                nodeRect,
                nodeColor);

            GUIStyle zoomStyle =
    new GUIStyle(GUI.skin.box);

            zoomStyle.fontSize =
                Mathf.RoundToInt(
                    14 * graphZoom);

            GUI.Box(
    nodeRect,
    node.id,
    zoomStyle);

            // CONNECTION POINT

            if (node.choices == null
    || node.choices.Count == 0)
            {

                Rect connectionPoint =
                new Rect(
                    nodeRect.xMax - 8 * graphZoom,
                    nodeRect.center.y - 6 * graphZoom,
                    12 * graphZoom,
                    12 * graphZoom);

                if (connectionPoint.Contains(
                e.mousePosition))
                {
                    EditorGUI.DrawRect(
                        connectionPoint,
                        Color.cyan);
                }
                else
                {
                    EditorGUI.DrawRect(
                        connectionPoint,
                        new Color(
                            0.9f,
                            0.9f,
                            0.9f));
                }

                if (e.type == EventType.MouseDown
                && e.button == 0
                && connectionPoint.Contains(
                    e.mousePosition))
                {
                    isDraggingConnection = true;

                    isBoxSelecting = false;

                    connectionStartID = node.id;

                    currentConnectionMouse =
                        e.mousePosition;

                    e.Use();
                }
            }
            bool clickedChoicePoint = false;

            if (node.choices != null)
            {
                float pointOffset = 75 * graphZoom;

                for (int c = 0; c < node.choices.Count; c++)
                {
                    float pointSize = 12 * graphZoom;

                    Rect pointRect =
                        new Rect(
                            nodeRect.xMax - pointSize * 0.7f,
                            nodeRect.y + pointOffset + 2,
                            pointSize,
                            pointSize);

                    if (pointRect.Contains(
                            e.mousePosition))
                    {
                        clickedChoicePoint = true;
                        break;
                    }

                    pointOffset += 20 * graphZoom;
                }
            }

            if (!clickedChoicePoint
    && e.type == EventType.MouseDown
    && e.button == 0
    && nodeRect.Contains(e.mousePosition))
            {
                SaveUndoState();

                selectedNode =
                    nodes.IndexOf(node);
                previewCharacters.Clear();

                isDraggingNode = true;

                draggedNodeID = node.id;

                dragOffset =
    zoomMouse - worldPos;

                e.Use();
            }

            graphNodeRects[node.id] =
    nodeRect;

            graphNodeRects[node.id] =
    nodeRect;

            // =========================
            // MINI LABEL STYLE
            // =========================

            GUIStyle miniLabel =
                new GUIStyle(EditorStyles.label);

            miniLabel.fontSize =
                Mathf.RoundToInt(12 * graphZoom);

            miniLabel.normal.textColor =
                Color.white;

            // NEXT NODE LABEL

            if (!string.IsNullOrEmpty(node.nextNodeID))
            {
                GUI.Label(
                    new Rect(
                        nodeRect.x,
                        nodeRect.y + 45 * graphZoom,
                        220 * graphZoom,
                        20 * graphZoom),
                    "↓ " + node.nextNodeID,
                    miniLabel);
            }

            // CHOICES

            if (node.choices != null)
            {
                float yOffset = 75 * graphZoom;

                foreach (DialogueChoice choice
                         in node.choices)
                {
                    string relationshipPreview =
    "";

                    if (choice.sadakoChange != 0)
                    {
                        relationshipPreview +=
                            " ❤️" +
                            (choice.sadakoChange > 0 ? "+" : "") +
                            choice.sadakoChange;
                    }

                    if (choice.sumikoChange != 0)
                    {
                        relationshipPreview +=
                            " 💛" +
                            (choice.sumikoChange > 0 ? "+" : "") +
                            choice.sumikoChange;
                    }

                    if (choice.terukoChange != 0)
                    {
                        relationshipPreview +=
                            " 💜" +
                            (choice.terukoChange > 0 ? "+" : "") +
                            choice.terukoChange;
                    }

                    GUI.Label(
                        new Rect(
                            nodeRect.x,
                            nodeRect.y + yOffset,
                            220 * graphZoom,
                            20 * graphZoom),
                        "├─ " +
                        choice.text +
                        relationshipPreview +
                        " → " +
                        choice.nextNodeID,
                        miniLabel);

                    float pointSize = 12 * graphZoom;

                    Rect choicePoint =
                        new Rect(
                            nodeRect.xMax - pointSize * 0.7f,
                            nodeRect.y + yOffset + 2,
                            pointSize,
                            pointSize);

                    EditorGUI.DrawRect(
                        choicePoint,
                        Color.cyan);



                    if (e.type == EventType.MouseDown
    && e.button == 0
    && choicePoint.Contains(e.mousePosition))
                    {
                        isDraggingConnection = true;

                        connectionStartID = node.id;

                        connectionChoiceIndex =
                            node.choices.IndexOf(choice);

                        currentConnectionMouse =
                            e.mousePosition;

                        e.Use();
                    }

                    yOffset += 20 * graphZoom;
                }
            }

        }

        if (isDraggingConnection
    && e.type == EventType.MouseUp)
        {
            foreach (DialogueNode targetNode
                     in nodes)
            {
                if (targetNode.id ==
                    connectionStartID)
                {
                    continue;
                }

                if (!graphNodeRects.ContainsKey(
                    targetNode.id))
                {
                    continue;
                }

                Rect targetRect =
                    graphNodeRects[
                        targetNode.id];

                if (targetRect.Contains(
                    e.mousePosition))
                {
                    DialogueNode startNode =
                        nodes.Find(
                            n => n.id ==
                            connectionStartID);

                    if (startNode != null)
                    {
                        if (connectionChoiceIndex >= 0)
                        {
                            startNode
                                .choices[connectionChoiceIndex]
                                .nextNodeID =
                                    targetNode.id;
                        }
                        else
                        {
                            startNode.nextNodeID =
                                targetNode.id;
                        }
                    }

                    break;
                }
            }

            isDraggingConnection = false;

            connectionStartID = "";

            connectionChoiceIndex = -1;

            Repaint();
        }

        if (e.type == EventType.MouseUp)
        {
            isDraggingNode = false;

            draggedNodeID = "";

            isDraggingComment = false;

            draggedComment = -1;
        }

        // =========================
        // BOX SELECTION END
        // =========================

        if (isBoxSelecting
            && e.type == EventType.MouseUp)
        {
            Rect selectionRect =
                Rect.MinMaxRect(
                    Mathf.Min(selectionStart.x, selectionEnd.x),
                    Mathf.Min(selectionStart.y, selectionEnd.y),
                    Mathf.Max(selectionStart.x, selectionEnd.x),
                    Mathf.Max(selectionStart.y, selectionEnd.y));

            selectedNodes.Clear();

            for (int i = 0; i < nodes.Count; i++)
            {
                if (graphNodeRects.ContainsKey(nodes[i].id)
                    && selectionRect.Overlaps(
                        graphNodeRects[nodes[i].id]))
                {
                    selectedNodes.Add(i);
                }
            }

            isBoxSelecting = false;

            Repaint();
        }

        // =========================
        // HOTKEYS
        // =========================

        if (e.type == EventType.KeyDown)
        {

            // UNDO

            if (e.control
                && e.keyCode == KeyCode.Z)
            {
                PerformUndo();

                Repaint();

                e.Use();
            }

            // REDO

            if (e.control
                && e.keyCode == KeyCode.Y)
            {
                PerformRedo();

                Repaint();

                e.Use();
            }

            // DELETE NODE

            if (e.keyCode == KeyCode.Delete)
            {
                if (selectedNode >= 0
                    && selectedNode < nodes.Count)
                {
                    string deletedID =
                        nodes[selectedNode].id;

                    SaveUndoState();

                    nodes.RemoveAt(selectedNode);

                    // remove graph data



                    if (graphNodeRects.ContainsKey(deletedID))
                    {
                        graphNodeRects.Remove(deletedID);
                    }

                    // safety

                    if (nodes.Count == 0)
                    {
                        AddEmptyNode();
                    }

                    selectedNode =
                        Mathf.Clamp(
                            selectedNode - 1,
                            0,
                            nodes.Count - 1);

                    Repaint();

                    e.Use();
                }
            }

            // DUPLICATE NODE

            if (e.control
                && e.keyCode == KeyCode.D)
            {
                if (selectedNode >= 0
                    && selectedNode < nodes.Count)
                {
                    DialogueNode original =
                        nodes[selectedNode];

                    DialogueNode duplicate =
                        JsonUtility.FromJson<DialogueNode>(
                            JsonUtility.ToJson(original));

                    duplicate.id =
                        GenerateUniqueNodeID();

                    // position

                    duplicate.editorPosition =
    original.editorPosition
    + new Vector2(40, 40);

                    SaveUndoState();

                    nodes.Add(duplicate);

                    selectedNode =
                        nodes.Count - 1;

                    Repaint();

                    e.Use();
                }
            }
        }

        // =========================
        // DRAW SELECTION RECT
        // =========================

        if (isBoxSelecting)
        {
            Rect selectionRect =
                Rect.MinMaxRect(
                    Mathf.Min(selectionStart.x, selectionEnd.x),
                    Mathf.Min(selectionStart.y, selectionEnd.y),
                    Mathf.Max(selectionStart.x, selectionEnd.x),
                    Mathf.Max(selectionStart.y, selectionEnd.y));

            EditorGUI.DrawRect(
                selectionRect,
                new Color(
                    0.3f,
                    0.6f,
                    1f,
                    0.15f));

            Handles.color =
                new Color(
                    0.3f,
                    0.6f,
                    1f,
                    0.9f);

            Handles.DrawSolidRectangleWithOutline(
                selectionRect,
                Color.clear,
                Handles.color);
        }

        DrawConnections();

        GUI.EndGroup();

        DrawMinimap(graphArea);

        GUILayout.EndVertical();
    }

    // =========================
    // GENERATE NODE ID
    // =========================

    string GenerateNodeID()
    {
        return "node_" +
               nodes.Count.ToString("000");
    }

    string GenerateUniqueNodeID()
    {
        return
            "node_" +
            System.Guid.NewGuid()
            .ToString("N")
            .Substring(0, 6);
    }

    void DrawConnections()
    {
        Handles.BeginGUI();



        foreach (DialogueNode node in nodes)
        {
            // NEXT NODE
            if (!string.IsNullOrEmpty(node.nextNodeID)
                && graphNodeRects.ContainsKey(node.id)
                && graphNodeRects.ContainsKey(node.nextNodeID))
            {
                DrawConnection(
                    graphNodeRects[node.id],
                    graphNodeRects[node.nextNodeID],
                    Color.green);
            }

            // CHOICES
            if (node.choices != null)
            {
                foreach (DialogueChoice choice in node.choices)
                {
                    if (!string.IsNullOrEmpty(choice.nextNodeID)
                        && graphNodeRects.ContainsKey(node.id)
                        && graphNodeRects.ContainsKey(choice.nextNodeID))
                    {
                        DrawConnection(
                            graphNodeRects[node.id],
                            graphNodeRects[choice.nextNodeID],
                            Color.yellow);
                    }
                }
            }
        }
        // TEMP CONNECTION

        if (isDraggingConnection
            && graphNodeRects.ContainsKey(
                connectionStartID))
        {
            Rect fromRect =
                graphNodeRects[
                    connectionStartID];

            Vector3 startPos =
                new Vector3(
                    fromRect.xMax,
                    fromRect.center.y,
                    0);

            Vector3 endPos =
                currentConnectionMouse;

            Vector3 startTangent =
                startPos + Vector3.right * 50;

            Vector3 endTangent =
                endPos + Vector3.left * 50;

            Handles.DrawBezier(
                startPos,
                endPos,
                startTangent,
                endTangent,
                Color.white,
                null,
                3f);
        }
        Handles.EndGUI();
    }

    void DrawConnection(
    Rect from,
    Rect to,
    Color color)
    {
        Vector3 startPos =
            new Vector3(
                from.xMax,
                from.y + 20,
                0);

        Vector3 endPos =
            new Vector3(
                to.xMin,
                to.y + 20,
                0);

        Vector3 startTangent =
            startPos + Vector3.right * 50;

        Vector3 endTangent =
            endPos + Vector3.left * 50;

        Handles.DrawBezier(
            startPos,
            endPos,
            startTangent,
            endTangent,
            color,
            null,
            3f);
    }
    void DrawGrid(
    Rect graphArea,
    float gridSpacing,
    float gridOpacity,
    Color gridColor)
    {
        int widthDivs =
            Mathf.CeilToInt(
                graphArea.width / gridSpacing);

        int heightDivs =
            Mathf.CeilToInt(
                graphArea.height / gridSpacing);

        Handles.BeginGUI();

        Color oldColor =
            Handles.color;

        Handles.color =
            new Color(
                gridColor.r,
                gridColor.g,
                gridColor.b,
                gridOpacity);

        Vector3 offset =
            new Vector3(
                graphOffset.x % gridSpacing,
                graphOffset.y % gridSpacing,
                0);

        // vertical

        for (int i = 0; i < widthDivs; i++)
        {
            Handles.DrawLine(
                new Vector3(
                    gridSpacing * i,
                    -gridSpacing,
                    0) + offset,

                new Vector3(
                    gridSpacing * i,
                    graphArea.height,
                    0f) + offset);
        }

        // horizontal

        for (int j = 0; j < heightDivs; j++)
        {
            Handles.DrawLine(
                new Vector3(
                    -gridSpacing,
                    gridSpacing * j,
                    0) + offset,

                new Vector3(
                    graphArea.width,
                    gridSpacing * j,
                    0f) + offset);
        }

        Handles.color =
            oldColor;

        Handles.EndGUI();
    }
    void DrawMinimap(Rect graphArea)
    {
        Rect minimapRect =
            new Rect(
                graphArea.xMax - 210,
                graphArea.y + 10,
                200,
                140);

        float minX = float.MaxValue;
        float minY = float.MaxValue;
        float maxX = float.MinValue;
        float maxY = float.MinValue;

        foreach (DialogueNode node in nodes)
        {
            Vector2 pos =
    node.editorPosition;



            minX = Mathf.Min(minX, pos.x);
            minY = Mathf.Min(minY, pos.y);

            maxX = Mathf.Max(maxX, pos.x + 220);
            maxY = Mathf.Max(maxY, pos.y + 80);
        }

        float graphWidth =
    maxX - minX;

        float graphHeight =
            maxY - minY;

        graphWidth =
    Mathf.Max(graphWidth, 1);

        graphHeight =
            Mathf.Max(graphHeight, 1);

        float scaleX =
            minimapRect.width / graphWidth;

        float scaleY =
            minimapRect.height / graphHeight;

        float minimapScale =
            Mathf.Min(scaleX, scaleY) * 0.9f;
        // background

        EditorGUI.DrawRect(
            minimapRect,
            new Color(
                0f,
                0f,
                0f,
                0.75f));

        // border

        Handles.color =
            new Color(
                1f,
                1f,
                1f,
                0.2f);

        Handles.DrawSolidRectangleWithOutline(
            minimapRect,
            Color.clear,
            new Color(
                1f,
                1f,
                1f,
                0.2f));

        // =========================
        // NODE DOTS
        // =========================

        foreach (DialogueNode node in nodes)
        {
            Vector2 worldPos =
    node.editorPosition;

            float miniX =
                minimapRect.x +
                (worldPos.x - minX)
* minimapScale;

            float miniY =
                minimapRect.y +
                (worldPos.y - minY)
* minimapScale;

            Rect dot =
                new Rect(
                    miniX,
                    miniY,
                    6,
                    6);

            Color dotColor =
                new Color(
                    0.7f,
                    0.7f,
                    0.7f);

            // selected node

            if (nodes[selectedNode].id ==
                node.id)
            {
                dotColor = Color.white;
            }

            EditorGUI.DrawRect(
                dot,
                dotColor);
        }

    }

}
