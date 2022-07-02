using UnityEngine;
using UnityEditor;
using BlazeAISpace;

[CanEditMultipleObjects]
[CustomEditor(typeof(BlazeAI))]
public class BlazeAIEditor : Editor
{
    string[] tabs = {"General", "States", "Distractions", "Hits", "Death", "Profile"};
    int tabSelected = 0;
    int tabIndex = -1;

    // variables
    SerializedProperty groundLayers,
    pathRecalculationRate,
    pathSmoothing,
    pathSmoothingFactor,
    pathFindingProxy,
    proxyOffset,
    enableGravity,
    gravityStrength,
    useRootMotion,
    centerPosition,
    showCenterPosition,
    avoidFacingObstacles,
    obstacleRayDistance,
    obstacleRayOffset,
    obstacleLayers,
    layersToAvoid,
    waypoints,
    vision,

    normalState,
    alertState,
    attackState,

    distractions,
    hits,
    death,
    blazeProfile,
    profileSync;


    void OnEnable()
    {
        if (EditorPrefs.HasKey("TabSelected")) {
            tabSelected = EditorPrefs.GetInt("TabSelected");
        }else{
            tabSelected = 0;
        }   

        // general
        groundLayers = serializedObject.FindProperty("groundLayers");
        pathRecalculationRate = serializedObject.FindProperty("pathRecalculationRate");
        pathSmoothing = serializedObject.FindProperty("pathSmoothing");
        pathSmoothingFactor = serializedObject.FindProperty("pathSmoothingFactor");
        pathFindingProxy = serializedObject.FindProperty("pathFindingProxy");
        proxyOffset = serializedObject.FindProperty("proxyOffset");
        enableGravity = serializedObject.FindProperty("enableGravity");
        gravityStrength = serializedObject.FindProperty("gravityStrength");
        useRootMotion = serializedObject.FindProperty("useRootMotion");
        centerPosition = serializedObject.FindProperty("centerPosition");
        showCenterPosition = serializedObject.FindProperty("showCenterPosition");

        avoidFacingObstacles = serializedObject.FindProperty("avoidFacingObstacles");
        obstacleRayDistance = serializedObject.FindProperty("obstacleRayDistance");
        obstacleRayOffset = serializedObject.FindProperty("obstacleRayOffset");
        obstacleLayers = serializedObject.FindProperty("obstacleLayers");
        
        layersToAvoid = serializedObject.FindProperty("layersToAvoid");
        waypoints = serializedObject.FindProperty("waypoints");
        vision = serializedObject.FindProperty("vision");

        // states
        normalState = serializedObject.FindProperty("normalState");
        alertState = serializedObject.FindProperty("alertState");
        attackState = serializedObject.FindProperty("attackState");

        // distractions
        distractions = serializedObject.FindProperty("distractions");

        // hits
        hits = serializedObject.FindProperty("hits");

        // death
        death = serializedObject.FindProperty("death");

        // profile
        blazeProfile = serializedObject.FindProperty("blazeProfile");
        profileSync = serializedObject.FindProperty("profileSync");
    }

    public override void OnInspectorGUI () 
    {
        var oldColor = GUI.backgroundColor;
        GUI.backgroundColor = new Color(0.55f, 0.55f, 0.55f, 1f);
        
        // unselected btns style
        var unSelectedBtn = new GUIStyle(GUI.skin.button);
        unSelectedBtn.fixedHeight = 45;
        unSelectedBtn.normal.textColor = Color.red;
        unSelectedBtn.fontSize = 13;

        // selected btn style
        var selectedBtn = new GUIStyle(GUI.skin.button);
        selectedBtn.fixedHeight = 45;
        selectedBtn.normal.textColor = Color.white;
        selectedBtn.fontSize = 15;
        selectedBtn.normal.background = MakeTex(0, 45, Color.gray);

        // render the toolbar
        GUILayout.BeginHorizontal("box");
        foreach (var item in tabs) {
            tabIndex++;
            if (tabIndex == tabSelected) {
                GUILayout.Button(item, selectedBtn);
            }else{
                if (GUILayout.Button(item, unSelectedBtn)) {
                    tabSelected = tabIndex;
                }
            }
        }
        GUILayout.EndHorizontal();
        EditorGUILayout.LabelField("Hover on any property below for insights", EditorStyles.helpBox);

        // reset the tabs
        tabIndex = -1;

        GUI.backgroundColor = oldColor;
        BlazeAI script = (BlazeAI)target;
        
        EditorGUILayout.Space(1);
        if (script.profileSync && script.blazeProfile) EditorGUILayout.HelpBox("Profile sync enabled. Some changes can only be made from the Blaze profile.", MessageType.Warning);

        switch (tabSelected)
        {
            case 0:
                GeneralTab(script);
                break;
            case 1:
                StatesTab();
                break;
            case 2:
                DistractionsTab();
                break;
            case 3:
                HitsTab();
                break;
            case 4:
                DeathTab();
                break;
            case 5:
                ProfileTab();
                break;
        }

        EditorPrefs.SetInt("TabSelected", tabSelected);
        serializedObject.ApplyModifiedProperties();
        
        if (script.profileSync) script.LoadProfile(script.blazeProfile);
        script.lastProfile = script.blazeProfile;
    }

    Texture2D MakeTex( int width, int height, Color col )
    {
        Color[] pix = new Color[width * height];
        for( int i = 0; i < pix.Length; ++i )
        {
            pix[ i ] = col;
        }
        Texture2D result = new Texture2D( width, height );
        result.SetPixels( pix );
        return result;
    }

    // build npc button functionality and style
    void BuildNPC(BlazeAI script)
    {
        var btnStyle = new GUIStyle(GUI.skin.button);
        btnStyle.normal.textColor = Color.white;
        btnStyle.active.textColor = Color.red;
        btnStyle.fontSize = 14;
        btnStyle.fixedHeight = 35;

        if (GUILayout.Button("Build Agent", btnStyle)) {
            if(script.CheckNPCBuild()){
                if(EditorUtility.DisplayDialog("Rebuild structure?","Blaze AI has detected that you have already built the agent. Are you sure you want to rebuild? This might break things!", "Build", "Do Not Build")){
                    script.BuildNPC();
                    EditorUtility.DisplayDialog("Agent Built!","Please read the messages printed in the console for important info.", "OK");
                }
            }else{
                script.BuildNPC();
                EditorUtility.DisplayDialog("Agent Built!","Please read the messages printed in the console for important info.", "OK");
            }
        }
    }

    // render the general tab properties
    void GeneralTab(BlazeAI script)
    {
        EditorGUILayout.PropertyField(groundLayers);
        EditorGUILayout.PropertyField(pathRecalculationRate);
        
        EditorGUILayout.PropertyField(pathSmoothing);
        if (script.pathSmoothing) EditorGUILayout.PropertyField(pathSmoothingFactor);
        
        EditorGUILayout.PropertyField(pathFindingProxy);
        EditorGUILayout.PropertyField(proxyOffset);
        
        EditorGUILayout.PropertyField(enableGravity);
        if (script.enableGravity) EditorGUILayout.PropertyField(gravityStrength);
        
        EditorGUILayout.PropertyField(useRootMotion);
        EditorGUILayout.PropertyField(centerPosition);
        EditorGUILayout.PropertyField(showCenterPosition);
        
        EditorGUILayout.PropertyField(avoidFacingObstacles);
        if (script.avoidFacingObstacles) {
            EditorGUILayout.PropertyField(obstacleRayDistance);
            EditorGUILayout.PropertyField(obstacleRayOffset);
            EditorGUILayout.PropertyField(obstacleLayers);
        }

        EditorGUILayout.PropertyField(layersToAvoid);
        EditorGUILayout.PropertyField(waypoints);
        EditorGUILayout.PropertyField(vision);

        EditorGUILayout.Space(25);
        BuildNPC(script);
    }

    // render the states classes
    void StatesTab()
    {
        EditorGUILayout.PropertyField(normalState);
        EditorGUILayout.PropertyField(alertState);
        EditorGUILayout.PropertyField(attackState);

        EditorGUILayout.Space(15);
    }

    // render the distractions tab class
    void DistractionsTab()
    {
        EditorGUILayout.PropertyField(distractions);
        EditorGUILayout.Space(15);
    }

    // render the hits tab class
    void HitsTab()
    {
        EditorGUILayout.PropertyField(hits);
        EditorGUILayout.Space(15);
    }

    // render the death tab class
    void DeathTab()
    {
        EditorGUILayout.PropertyField(death);
        EditorGUILayout.Space(15);
    }

    // render the profile tab
    void ProfileTab()
    {
        EditorGUILayout.PropertyField(blazeProfile);
        EditorGUILayout.PropertyField(profileSync);

        EditorGUILayout.Space(15);
    }
}
