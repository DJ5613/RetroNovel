using System.Collections.Generic;
using System.IO;
using Unity.VisualScripting;
using UnityEditor;
using UnityEditor.IMGUI.Controls;
using UnityEngine;

public class DialogueEditorWindow : EditorWindow
{

    
    float graphZoom = 1f;

    Vector2 graphOffset = Vector2.zero;
    Vector2 panStart;
    Vector2 dragOffset;

    bool isPanningGraph;


    bool showGraph = true;

    bool isDraggingNode;

    string draggedNodeID;


    Dictionary<string, Rect> graphNodeRects =
    new Dictionary<string, Rect>();

    Dictionary<string, Vector2> nodePositions =
    new Dictionary<string, Vector2>();

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

        DrawTopBar();

        GUILayout.BeginHorizontal();

        DrawLeftPanel();

        if (showGraph)
        {
            DrawMiniGraph();
        }

        DrawRightPanel();

        GUILayout.EndHorizontal();

        Event e = Event.current;

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
        GUILayout.BeginVertical(
            GUILayout.Width(250));

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
            }
        }

        GUILayout.EndScrollView();

        if (GUILayout.Button("+ Add Node"))
        {
            DialogueNode previous =
    nodes[selectedNode];

            DialogueNode newNode =
                new DialogueNode()
                {
                    id = GenerateNodeID(),

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
            nodes[selectedNode];

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

        node.background =
            EditorGUILayout.TextField(
                "Background",
                node.background);

        // =========================
        // MUSIC
        // =========================

        node.music =
            EditorGUILayout.TextField(
                "Music",
                node.music);

        // =========================
        // NEXT NODE
        // =========================

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
        // MULTI CHARACTER PREVIEW
        // =========================

        if (node.setCharacters != null)
        {
            foreach (CharacterState character
                     in node.setCharacters)
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
            GUILayout.Width(300));



        GUILayout.Label(
            "Graph View",
            EditorStyles.boldLabel);

        Rect graphArea =
    GUILayoutUtility.GetRect(
        position.width - 500,
        position.height - 100);



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

        Matrix4x4 oldMatrix =
    GUI.matrix;

        GUIUtility.ScaleAroundPivot(
            Vector2.one * graphZoom,
            Vector2.zero);

        Event e = Event.current;

        // =========================
        // GRAPH ZOOM
        // =========================

        if (e.type == EventType.ScrollWheel &&
            graphArea.Contains(e.mousePosition))
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
    e.type == EventType.MouseDown &&
    graphArea.Contains(e.mousePosition))
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
            if (!nodePositions.ContainsKey(node.id))
            {
                nodePositions[node.id] =
                    new Vector2(
                        20,
                        40 + nodes.IndexOf(node) * 80);
            }

            Vector2 worldPos =
    nodePositions[node.id];

            Vector2 screenPos =
    worldPos + graphOffset;



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
        220,
nodeHeight);

            

            if (isDraggingNode
    && draggedNodeID == node.id
    && e.type == EventType.MouseDrag)
            {
                nodePositions[node.id] =
    ((e.mousePosition / graphZoom)
    - dragOffset)
    - graphOffset;

                Repaint();
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

            if (nodes.IndexOf(node) == selectedNode)
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

            GUI.Box(
                nodeRect,
                node.id);

            if (e.type == EventType.MouseDown
                && e.button == 0
                && nodeRect.Contains(e.mousePosition))
            {
                selectedNode =
                    nodes.IndexOf(node);

                isDraggingNode = true;

                draggedNodeID = node.id;

                dragOffset =
    (e.mousePosition / graphZoom)
    - screenPos;

                e.Use();
            }

            graphNodeRects[node.id] =
    new Rect(
        nodeRect.x * graphZoom,
        nodeRect.y * graphZoom,
        nodeRect.width * graphZoom,
        nodeRect.height * graphZoom
    );




            // NEXT NODE LABEL

            if (!string.IsNullOrEmpty(node.nextNodeID))
            {
                GUI.Label(
                    new Rect(
                        nodeRect.x,
                        nodeRect.y + 45,
                        220,
                        20),
                    "↓ " + node.nextNodeID);
            }

            // CHOICES

            if (node.choices != null)
            {
                float yOffset = 65;

                foreach (DialogueChoice choice
                         in node.choices)
                {
                    GUI.Label(
                        new Rect(
                            nodeRect.x,
                            nodeRect.y + yOffset,
                            220,
                            20),
                        "├─ " +
                        choice.text +
                        " → " +
                        choice.nextNodeID);

                    yOffset += 20;
                }
            }

        }
        if (e.type == EventType.MouseUp)
        {
            isDraggingNode = false;

            draggedNodeID = "";
        }
        GUI.matrix = oldMatrix;
        DrawConnections();

        GUI.EndGroup();

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
}




