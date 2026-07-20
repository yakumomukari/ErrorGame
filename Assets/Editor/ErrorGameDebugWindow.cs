using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;

public sealed class ErrorGameDebugWindow : EditorWindow
{
    private sealed class ResourceEntry
    {
        public string Path { get; }
        public UnityEngine.Object Asset { get; }

        public ResourceEntry(string path, UnityEngine.Object asset)
        {
            Path = path;
            Asset = asset;
        }
    }

    private readonly List<ResourceEntry> resources = new List<ResourceEntry>();
    private Vector2 scrollPosition;
    private string resourceSearch = string.Empty;
    private int selectedTab;

    [MenuItem("Tools/Error Game/Debug Inspector")]
    public static void Open()
    {
        GetWindow<ErrorGameDebugWindow>("ErrorGame Debug");
    }

    private void OnEnable()
    {
        RefreshResources();
    }

    private void OnInspectorUpdate()
    {
        if (Application.isPlaying) Repaint();
    }

    private void OnGUI()
    {
        selectedTab = GUILayout.Toolbar(selectedTab, new[] { "Runtime", "Input", "Resources" });
        EditorGUILayout.Space(6f);
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
        switch (selectedTab)
        {
            case 0: DrawRuntime(); break;
            case 1: DrawInput(); break;
            default: DrawResources(); break;
        }
        EditorGUILayout.EndScrollView();
    }

    private static void DrawRuntime()
    {
        GameSession session = FindObjectOfType<GameSession>();
        Player player = session != null ? session.Player : FindObjectOfType<Player>();

        DrawObjectRow("Game Session", session);
        DrawObjectRow("Player", player);
        if (!Application.isPlaying)
        {
            EditorGUILayout.HelpBox("Enter Play Mode to inspect live dungeon, player, input, and enemy state.", MessageType.Info);
        }

        if (session != null)
        {
            EditorGUILayout.Space(6f);
            EditorGUILayout.LabelField("Session", EditorStyles.boldLabel);
            DrawValue("Seed", session.ActiveSeed);
            DrawValue("Current Room", session.CurrentRoom != null
                ? $"{session.CurrentRoom.Node.Type} {session.CurrentRoom.Node.Coordinate}"
                : "None");
            DrawValue("Entrance", session.Rooms != null && session.Rooms.CurrentEntranceDirection.HasValue
                ? session.Rooms.CurrentEntranceDirection.Value.ToString()
                : "Center / None");
            DrawValue("Save Ready", session.Persistence != null && session.Persistence.IsReady);
            if (session.SaveRepository is JsonGameSaveRepository jsonRepository)
            {
                DrawValue("Save Path", jsonRepository.SavePath);
                DrawValue("Has Save", jsonRepository.HasSave);
            }
        }

        if (player != null) DrawPlayer(player);
        if (session != null && session.Layout != null) DrawRooms(session.Layout);
        DrawEnemies();
    }

    private static void DrawPlayer(Player player)
    {
        EditorGUILayout.Space(6f);
        EditorGUILayout.LabelField("Player", EditorStyles.boldLabel);
        DrawValue("Health", $"{player.Health.CurrentHealthUnits}/{player.Health.MaxHealthUnits} units");
        DrawValue("Dead", player.Health.IsDead);
        DrawValue("Coins", player.Inventory.Coins);
        DrawValue("Bombs", player.Inventory.Bombs);
        DrawValue("Move Speed", player.Stats.MoveSpeed);
        DrawValue("Fire Rate", player.Stats.FireRate);
        DrawValue("Damage", player.Stats.Damage);
        DrawValue("Range", player.Stats.Range);
        DrawValue("Projectile Speed", player.Stats.ProjectileSpeed);
        DrawValue("Luck", player.Stats.Luck);

        PlayerMovement movement = player.GetComponent<PlayerMovement>();
        PlayerAim aim = player.GetComponent<PlayerAim>();
        PlayerDash dash = player.GetComponent<PlayerDash>();
        DrawValue("Move Input", movement != null ? movement.MoveInput.ToString("F3") : "None");
        DrawValue("Aim", aim != null ? aim.AimDirection.ToString("F3") : "None");
        DrawValue("Dashing", dash != null && dash.IsDashing);
        DrawValue("Dash Cooldown", dash != null ? dash.CooldownRemaining : 0f);
    }

    private static void DrawRooms(DungeonLayout layout)
    {
        EditorGUILayout.Space(6f);
        EditorGUILayout.LabelField($"Dungeon Rooms ({layout.Rooms.Count})", EditorStyles.boldLabel);
        foreach (RoomNode room in layout.Rooms.Values
                     .OrderByDescending(node => node.Coordinate.Y)
                     .ThenBy(node => node.Coordinate.X))
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField($"{room.Type} {room.Coordinate}", EditorStyles.boldLabel);
            DrawValue("Visited / Cleared", $"{room.IsVisited} / {room.IsCleared}");
            DrawValue("Connections", string.Join(", ", room.Connections));
            DrawValue("Item Claimed", room.IsItemClaimed);
            DrawValue("Combat Reward", $"type {room.CombatRewardType}, collected {room.IsCombatRewardCollected}");
            DrawValue("Purchased Slots", string.Join(", ", room.PurchasedShopSlots));
            DrawValue("Secret Rewards", string.Join(", ", room.CollectedSecretRewards));
            EditorGUILayout.EndVertical();
        }
    }

    private static void DrawEnemies()
    {
        EnemyHealth[] enemies = FindObjectsOfType<EnemyHealth>(true);
        EditorGUILayout.Space(6f);
        EditorGUILayout.LabelField($"Living Enemy Objects ({enemies.Length})", EditorStyles.boldLabel);
        foreach (EnemyHealth enemy in enemies)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField(enemy, typeof(EnemyHealth), true);
            GUILayout.Label($"HP {enemy.CurrentHealthUnits}/{enemy.MaxHealthUnits}", GUILayout.Width(110f));
            EditorGUILayout.EndHorizontal();
        }
    }

    private static void DrawInput()
    {
        Player player = FindObjectOfType<Player>();
        GameInputReader reader = player != null ? player.Input : FindObjectOfType<GameInputReader>();
        InputActionAsset asset = reader != null
            ? reader.InputActions
            : AssetDatabase.LoadAssetAtPath<InputActionAsset>("Assets/Scripts/InputSystem.inputactions");

        DrawObjectRow("Input Reader", reader);
        DrawObjectRow("Actions Asset", asset);
        if (reader != null)
        {
            DrawValue("Ready", reader.IsReady);
            DrawValue("Move", reader.Move.ToString("F3"));
            DrawValue("Pointer", reader.PointerScreenPosition.ToString("F1"));
            DrawValue("Fire Held", reader.FireHeld);
            DrawValue("Dash Pressed", reader.DashPressedThisFrame);
            DrawValue("Bomb Pressed", reader.BombPressedThisFrame);
            DrawValue("Interact Pressed", reader.InteractPressedThisFrame);
            DrawValue("Pause Pressed", reader.PausePressedThisFrame);
            DrawValue("Restart Pressed", reader.RestartPressedThisFrame);
        }

        if (asset == null) return;
        EditorGUILayout.Space(6f);
        foreach (InputActionMap map in asset.actionMaps)
        {
            EditorGUILayout.LabelField($"Map: {map.name} ({(map.enabled ? "Enabled" : "Disabled")})", EditorStyles.boldLabel);
            foreach (InputAction action in map.actions)
            {
                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                DrawValue("Action", action.name);
                DrawValue("Phase", action.phase);
                DrawValue("Control", action.activeControl != null ? action.activeControl.path : "None");
                DrawValue("Bindings", string.Join(", ", action.bindings.Select(binding => binding.effectivePath)));
                EditorGUILayout.EndVertical();
            }
        }
    }

    private void DrawResources()
    {
        EditorGUILayout.BeginHorizontal();
        resourceSearch = EditorGUILayout.TextField("Search", resourceSearch);
        if (GUILayout.Button("Refresh", GUILayout.Width(80f))) RefreshResources();
        EditorGUILayout.EndHorizontal();
        EditorGUILayout.LabelField($"Unity assets exposed: {resources.Count}", EditorStyles.miniLabel);

        string search = resourceSearch.Trim();
        foreach (ResourceEntry entry in resources)
        {
            if (search.Length > 0 && entry.Path.IndexOf(search, StringComparison.OrdinalIgnoreCase) < 0) continue;

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.ObjectField(entry.Asset, typeof(UnityEngine.Object), false);
            if (GUILayout.Button("Select", GUILayout.Width(55f))) Selection.activeObject = entry.Asset;
            if (GUILayout.Button("Ping", GUILayout.Width(45f))) EditorGUIUtility.PingObject(entry.Asset);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField(entry.Path, EditorStyles.miniLabel);
        }
    }

    private void RefreshResources()
    {
        resources.Clear();
        foreach (string guid in AssetDatabase.FindAssets(string.Empty, new[] { "Assets" }))
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            if (AssetDatabase.IsValidFolder(path) || path.EndsWith(".cs", StringComparison.OrdinalIgnoreCase)) continue;

            UnityEngine.Object asset = AssetDatabase.LoadMainAssetAtPath(path);
            if (asset == null || asset is MonoScript) continue;
            resources.Add(new ResourceEntry(path, asset));
        }
        resources.Sort((left, right) => string.Compare(left.Path, right.Path, StringComparison.OrdinalIgnoreCase));
        Repaint();
    }

    private static void DrawObjectRow(string label, UnityEngine.Object value)
    {
        EditorGUILayout.BeginHorizontal();
        EditorGUILayout.ObjectField(label, value, typeof(UnityEngine.Object), true);
        if (value != null && GUILayout.Button("Select", GUILayout.Width(55f))) Selection.activeObject = value;
        EditorGUILayout.EndHorizontal();
    }

    private static void DrawValue(string label, object value)
    {
        EditorGUILayout.LabelField(label, value != null ? value.ToString() : "null");
    }
}
