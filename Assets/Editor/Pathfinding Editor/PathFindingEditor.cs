using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;
public class PathFindingEditor : EditorWindow
{
    private WaypointsContainer _wpContainer;
    private GameObject _waypoint;
    private List<Node> _nodes;

    //GUI Styles
    private GUIStyle _headerStyle;

    //Settings para creacion de waypoints
    private Vector3 _originPoint;
    private float _pathfindingAreaWidth = 0;
    private float _pathfindingAreaLength = 0;
    private int _waypointRows = 1;
    private int _waypointColumns = 1;
    private bool _enableWPIndicators = true;
    private float _gizmoRadius = .75f;
    private Color _gizmoColor = new Color(0.34f, 0.84f, 0.86f, 0.6f);
    private List<WaypointData> _waypointData;

    //Waypoint obstacles
    private bool _detectObstacles;
    private float _radiusObstacleDetection;
    private LayerMask _obstaclesMask;

    //Waypoints connection
    private bool _setConnections = false;
    private float _radiusDistanceConnection = 0f;
    private string _saveFolderPath = "Assets/pf_data/WaypointsInfo";
    private string _saveFilename = "wpinfo.asset";

    //Indicador area pathfinding
    private Vector3 _textAreaPosition;
    private Vector2 scrollPos;

    //Ventana Loader
    private WPLoad _loadWindow;

    //Flags
    private bool _waypointGenerationMode;
    private bool _calculatingPositions;
    private bool _disableConnectionsButton;
    private bool _pendingConnections;
    private bool _pendingStoreVariables;
    [MenuItem("Tools/Pathfinding Generator")]
    public static void OpenWindow()
    {
        var window = GetWindow<PathFindingEditor>();
        window.Show();
    }
    private void OnEnable()
    {
        _waypoint = Resources.Load("waypoint/waypoint", typeof(GameObject)) as GameObject;
        _wpContainer = FindObjectOfType<WaypointsContainer>();
        if (_wpContainer != null)
        {
            _waypointData = RestoreReferences(_wpContainer.GetComponentsInChildren<Node>());
            _radiusDistanceConnection = _wpContainer.radiusDistanceConnection;
            _setConnections = _wpContainer.displayConnectionLines;
        }
        SceneView.duringSceneGui += OnSceneGUI;
        SetStyles();
    }
    private List<WaypointData> RestoreReferences(Node[] nodes)
    {
        var wpData = new List<WaypointData>();
        for (var i = 0; i < nodes.Length; i++)
        {
            var neighboursIDs = new List<int>();
            for (var j = 0; j < nodes[i].Neighbours.Count; j++) neighboursIDs.Add(nodes[i].Neighbours[j].Id);
            wpData.Add(new WaypointData
            {
                id = nodes[i].Id,
                position = nodes[i].transform.position,
                connectedNodesID = neighboursIDs
            });
        }
        return wpData;
    }
    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        if (_loadWindow != null) _loadWindow.Close();
    }
    private void SetStyles()
    {
        _headerStyle = new GUIStyle();
        _headerStyle.fontStyle = FontStyle.Bold;
    }
    private void OnGUI()
    {
        //Interfaz que se muestra si no hay ningún waypoint o contenedor de waypoints cargado
        if (!_waypointGenerationMode && (_wpContainer == null || _wpContainer.transform.childCount == 0))
        {
            EditorGUILayout.HelpBox("No se han encontrado waypoints o contenedores de waypoints", MessageType.Warning);
            EditorGUILayout.BeginHorizontal();
            if (GUILayout.Button("Crear waypoints")) _waypointGenerationMode = true;
            EditorGUI.BeginDisabledGroup(!AssetDatabase.IsValidFolder(_saveFolderPath));
            if (GUILayout.Button("Cargar waypoints")) OpenLoaderWindow();
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
        }
        //Interfaz de creacion de waypoints
        else if (_waypointGenerationMode)
        {
            EditorGUILayout.LabelField("Waypoints generation");
            var rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, Color.gray);
            EditorGUILayout.Space();
            LayoutWaypointsSettings();
            rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, Color.gray);
            if (GUILayout.Button("Generate Waypoints"))
            {
                GenerateWaypoints(out _nodes);
                _waypointGenerationMode = false;
            }
            _textAreaPosition = _originPoint + new Vector3(-1, 0, -1);
            rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, Color.gray);
            EditorGUILayout.Space();
            LayoutWaypointIndicators();
        }
        //Interfaz cuando ya tengo waypoints seteados
        else if (_wpContainer != null && _wpContainer.transform.childCount > 0)
        {
            //Conexiones
            LayoutConexiones();
            var rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, Color.gray);

            //Scroll waypoints
            LayoutWaypointsScroll();
            rect = EditorGUILayout.GetControlRect(false, 1);
            EditorGUI.DrawRect(rect, Color.gray);
            if (GUILayout.Button("Save waypoints")) SaveWaypoints(_nodes);
        }
    }
    #region Window layouts
    private void LayoutWaypointsSettings()
    {
        EditorGUILayout.LabelField("Pathfinding Area", _headerStyle);
        EditorGUI.BeginChangeCheck();

        //Si hago un cambio en el area o los waypoints, recalculo las posiciones
        GUILayout.BeginHorizontal();
        _pathfindingAreaWidth = EditorGUILayout.FloatField("Width", _pathfindingAreaWidth);
        _pathfindingAreaLength = EditorGUILayout.FloatField("Length", _pathfindingAreaLength);
        GUILayout.EndHorizontal();
        EditorGUILayout.LabelField("Waypoints amount", _headerStyle);
        GUILayout.BeginHorizontal();
        _waypointRows = EditorGUILayout.IntField("Rows", _waypointRows);
        _waypointColumns = EditorGUILayout.IntField("Columns", _waypointColumns);
        GUILayout.EndHorizontal();
        _originPoint = EditorGUILayout.Vector3Field("Origin Point", _originPoint);
        var rect = EditorGUILayout.GetControlRect(false, 1);
        EditorGUI.DrawRect(rect, Color.gray);
        EditorGUILayout.LabelField("Obstacles", _headerStyle);
        _detectObstacles = EditorGUILayout.Toggle("Don't instantiate near obstacles", _detectObstacles);
        _radiusObstacleDetection = EditorGUILayout.FloatField("Radius Detection", _radiusObstacleDetection);
        LayerMask tempMask = EditorGUILayout.MaskField("Obstacles mask",
            InternalEditorUtility.LayerMaskToConcatenatedLayersMask(_obstaclesMask), InternalEditorUtility.layers);
        _obstaclesMask = InternalEditorUtility.ConcatenatedLayersMaskToLayerMask(tempMask);
        if (EditorGUI.EndChangeCheck())
        {
            _pathfindingAreaWidth = _pathfindingAreaWidth < 0 ? 0 : _pathfindingAreaWidth;
            _pathfindingAreaLength = _pathfindingAreaLength < 0 ? 0 : _pathfindingAreaLength;
            _waypointRows = _waypointRows < 1 ? 1 : _waypointRows;
            _waypointColumns = _waypointColumns < 1 ? 1 : _waypointColumns;
            _radiusObstacleDetection = _radiusObstacleDetection < 0 ? 0 : _radiusObstacleDetection;
            CalculatePositions();
        }
    }
    private void LayoutConexiones()
    {
        if (_pendingStoreVariables)
        {
            _wpContainer.displayConnectionLines = _setConnections;
            _wpContainer.radiusDistanceConnection = _radiusDistanceConnection;
            _pendingStoreVariables = false;
        }
        if (_pendingConnections)
        {
            GenerateConnections(_nodes);
            //_wpContainer.displayConnectionLines = _setConnections;
            //_wpContainer.radiusDistanceConnection = _radiusDistanceConnection;
            _pendingConnections = false;
            _pendingStoreVariables = true;
            return;
        }
        EditorGUILayout.LabelField("Connections", _headerStyle);
        EditorGUI.BeginChangeCheck();
        _setConnections = EditorGUILayout.Toggle("Display connection lines", _setConnections);
        //EditorGUI.BeginDisabledGroup(!_setConnections);
        _radiusDistanceConnection = EditorGUILayout.FloatField("Distance Radius", _radiusDistanceConnection);
        //EditorGUI.EndDisabledGroup();
        if (EditorGUI.EndChangeCheck()) _disableConnectionsButton = true;
        EditorGUI.BeginDisabledGroup(!_disableConnectionsButton);
        if (GUILayout.Button("Bake connections"))
        {
            GenerateConnections(_nodes);
            _wpContainer.radiusDistanceConnection = _radiusDistanceConnection;
            _wpContainer.displayConnectionLines = _setConnections;
        }
        EditorGUI.EndDisabledGroup();
    }
    private void LayoutWaypointIndicators()
    {
        //Algunos ajustes de visualizacion de indicadores
        EditorGUILayout.LabelField("Scene Indicators", _headerStyle);
        _enableWPIndicators = EditorGUILayout.ToggleLeft("Show indicators", _enableWPIndicators);
        _gizmoRadius = EditorGUILayout.FloatField("Waypoint indicator radius", _gizmoRadius);
        _gizmoColor = EditorGUILayout.ColorField("Waypoint indicator color", _gizmoColor);
    }
    private void LayoutWaypointsScroll()
    {
        EditorGUILayout.LabelField("Waypoints", _headerStyle);
        scrollPos = EditorGUILayout.BeginScrollView(scrollPos, GUILayout.Width(position.width),
            GUILayout.Height(position.height - 130));
        var wps = _wpContainer.transform.GetComponentsInChildren<Node>();
        _nodes = new List<Node>(wps);
        //for (int i = 0; i < wps.Length; i++)
        //{
        //    EditorGUILayout.ObjectField(wps[i].gameObject, typeof(GameObject), true);
        //}
        for (var i = 0; i < _nodes.Count; i++)
            EditorGUILayout.ObjectField(_nodes[i].gameObject, typeof(GameObject), true);
        EditorGUILayout.EndScrollView();
    }
    #endregion
    #region Generators
    private List<GameObject> GenerateWaypoints()
    {
        return GenerateWaypoints(out var nodes);
    }
    private List<GameObject> GenerateWaypoints(out List<Node> nodes)
    {
        var objs = new List<GameObject>();
        nodes = new List<Node>();
        var undoID = Undo.GetCurrentGroup();
        if (_wpContainer == null)
        {
            _wpContainer = new GameObject("waypoint_container").AddComponent<WaypointsContainer>();
            _wpContainer.transform.position = Vector3.zero;
            Undo.RegisterCreatedObjectUndo(_wpContainer, "Object created");
        }
        var setID = 0;
        for (var i = 0; i < _waypointData.Count; i++)
        {
            var obj = (GameObject)PrefabUtility.InstantiatePrefab(_waypoint);
            obj.transform.position = _waypointData[i].position;
            obj.transform.parent = _wpContainer.transform;
            obj.name = obj.name + i;
            if (i != 0 && _waypointData[i].id == 0)
                setID = i;
            else
                setID = _waypointData[i].id;
            objs.Add(obj);
            nodes.Add(obj.GetComponent<Node>());
            nodes[i].Id = setID;
            Undo.RegisterCreatedObjectUndo(obj, "Object created");
        }
        Undo.CollapseUndoOperations(undoID);
        return objs;
    }
    private void GenerateConnections(List<Node> nodes)
    {
        WaypointData wpData;
        //var undoID = Undo.GetCurrentGroup();

        //ELijo un nodo de la lista
        for (var i = 0; i < nodes.Count; i++)
        {
            wpData = _waypointData[i];
            //Por cada nodo tengo...
            //1. Obtener el ID propio y guardarlo en _wayPointData
            wpData.id = nodes[i].Id;

            //2. Generar las conexiones y obtener el ID de los mismos. Guardarlos en _wayPointData
            nodes[i].RadiusDistance = _radiusDistanceConnection;
            List<Node> connectedNodes;
            connectedNodes = nodes[i].SetNewNeighbours();
            PrefabUtility.RecordPrefabInstancePropertyModifications(nodes[i]);
            wpData.connectedNodesID = new List<int>();

            //Elijo una de las conexiones de la lista
            for (var j = 0; j < connectedNodes.Count; j++) wpData.connectedNodesID.Add(connectedNodes[j].Id);
            _waypointData[i] = wpData;
        }

        //EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
        //Undo.CollapseUndoOperations(undoID);
    }
    #endregion
    #region Save and Load
    private void SaveWaypoints(Node[] wps)
    {
        var wpsList = new List<Node>(wps);
        SaveWaypoints(wpsList);
    }
    private void SaveWaypoints(List<Node> wps)
    {
        var scriptable = CreateInstance<WaypointsInfo>();
        scriptable.waypointsData = new List<WaypointData>();
        WaypointData wpData = default;
        for (var i = 0; i < wps.Count; i++)
        {
            wpData.id = wps[i].Id;
            wpData.position = wps[i].transform.position;
            wpData.connectedNodesID = new List<int>();
            for (var j = 0; j < wps[i].Neighbours.Count; j++) wpData.connectedNodesID.Add(wps[i].Neighbours[j].Id);
            scriptable.waypointsData.Add(wpData);
        }
        scriptable.displayConnectionLines = _setConnections;
        scriptable.radiusDistanceConnection = _radiusDistanceConnection;

        //Guardo el scriptable
        if (!AssetDatabase.IsValidFolder(_saveFolderPath))
        {
            var separatedPath = _saveFolderPath.Split('/');
            AssetDatabase.CreateFolder(separatedPath[0], separatedPath[1]);
        }
        var path = _saveFolderPath + '/' + _saveFilename;
        path = AssetDatabase.GenerateUniqueAssetPath(path);
        AssetDatabase.CreateAsset(scriptable, path);
    }
    private void LoadWaypoints(string fileName)
    {
        var path = _saveFolderPath + '/' + fileName;
        var scriptable = AssetDatabase.LoadAssetAtPath<WaypointsInfo>(path);
        var wpsData = scriptable.waypointsData;
        _waypointData = new List<WaypointData>();
        for (var i = 0; i < wpsData.Count; i++) _waypointData.Add(wpsData[i]);
        _ = GenerateWaypoints(out _nodes);
        _setConnections = scriptable.displayConnectionLines;
        _radiusDistanceConnection = scriptable.radiusDistanceConnection;
        _pendingConnections = true;
    }
    #endregion
    private void OpenLoaderWindow()
    {
        _loadWindow = GetWindow<WPLoad>();
        _loadWindow.SaveFolderPath = _saveFolderPath + '/';
        _loadWindow.wpLoader += LoadWaypoints;
        _loadWindow.Show();
    }
    private void CalculatePositions()
    {
        _calculatingPositions = true;
        float xPos, zPos;
        var rowDivision = _pathfindingAreaLength / (_waypointRows + 1);
        var columnDivision = _pathfindingAreaWidth / (_waypointColumns + 1);
        Vector3 positionToAdd;
        _waypointData = new List<WaypointData>();
        for (var r = 0; r < _waypointRows; r++)
        {
            zPos = (r + 1) * rowDivision + _originPoint.z;
            for (var c = 0; c < _waypointColumns; c++)
            {
                xPos = (c + 1) * columnDivision + _originPoint.x;
                positionToAdd = new Vector3(xPos, _originPoint.y, zPos);
                if (!_detectObstacles || !CheckIfObstaclesNear(positionToAdd))
                    _waypointData.Add(new WaypointData { position = positionToAdd });
            }
        }
        _calculatingPositions = false;
    }
    private bool CheckIfObstaclesNear(Vector3 positionToAdd)
    {
        var colliders = Physics.OverlapSphere(positionToAdd, _radiusObstacleDetection, _obstaclesMask);
        return colliders.Length > 0;
    }
    private void OnSceneGUI(SceneView sceneView)
    {
        //Modo edicion
        if (_waypointGenerationMode)
        {
            //Dibujo el area donde se instanciaran los waypoints
            var MyPosForward = _originPoint + Vector3.forward * _pathfindingAreaLength;
            var MyPosRight = _originPoint + Vector3.right * _pathfindingAreaWidth;
            Handles.DrawDottedLine(_originPoint, MyPosForward, 2);
            Handles.DrawDottedLine(_originPoint, MyPosRight, 2);
            Handles.DrawDottedLine(MyPosForward, MyPosForward + Vector3.right * _pathfindingAreaWidth, 2);
            Handles.DrawDottedLine(MyPosRight, MyPosRight + Vector3.forward * _pathfindingAreaLength, 2);

            //Dibujo los indicadores donde se setearían los waypoints.
            if (_enableWPIndicators && !_calculatingPositions && _waypointRows > 0 && _waypointColumns > 0)
            {
                var id = 0;
                Handles.color = _gizmoColor;
                if (_waypointData != null)
                    for (var i = 0; i < _waypointData.Count; i++)
                    {
                        Handles.SphereHandleCap(id,
                            new Vector3(_waypointData[i].position.x, _originPoint.y, _waypointData[i].position.z),
                            Quaternion.identity, _gizmoRadius, EventType.Repaint);
                        id++;
                    }
            }
            Handles.color = Color.white;
            Handles.DrawDottedLine(_textAreaPosition, _originPoint, 2);
            Handles.BeginGUI();
            var cmraPoint = Camera.current.WorldToScreenPoint(_textAreaPosition);
            var cmraRectHeight = Camera.current.pixelHeight;
            var cmraRectWidth = Camera.current.pixelWidth;
            var rect = new Rect(cmraPoint.x - 75, cmraRectHeight - cmraPoint.y, 200, 50);
            var text = "Pathfinding Area: " +
                       string.Format("{0}x{1}\n", _pathfindingAreaLength, _pathfindingAreaWidth) +
                       "Total waypoints: " + _waypointRows * _waypointColumns;
            GUI.Box(rect, text);
            Handles.EndGUI();
        }
        if (!_waypointGenerationMode && _wpContainer != null && _wpContainer.transform.childCount > 0)
            //Indicadores de conexiones
            if (_setConnections || (_waypointData != null && !_waypointGenerationMode &&
                                    (_wpContainer == null || _wpContainer.transform.childCount == 0)))
            {
                Handles.color = Color.yellow;
                var id = 0;
                for (var i = 0; i < _waypointData.Count; i++)
                {
                    id = i;
                    if (Event.current.type == EventType.Repaint)
                        Handles.SphereHandleCap(id, _waypointData[i].position, Quaternion.identity, .5f,
                            EventType.Repaint);
                    else if (Event.current.type == EventType.ContextClick)
                        Handles.SphereHandleCap(id, _waypointData[i].position, Quaternion.identity, .5f,
                            EventType.ContextClick);
                    var nodesIDs = _waypointData[i].connectedNodesID;
                    if (nodesIDs == null || nodesIDs.Count == 0) break;
                    for (var j = 0; j < nodesIDs.Count; j++)
                        Handles.DrawLine(_waypointData[i].position, _waypointData[nodesIDs[j]].position);
                }
            }
    }
}