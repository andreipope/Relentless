// Copyright (C) 2016-2017 David Pol. All rights reserved.
// This code can only be used under the standard Unity Asset Store End User License Agreement,
// a copy of which is available at http://unity3d.com/company/legal/as_terms.

#if ENABLE_MASTER_SERVER_KIT

using UnityEditor;

/// <summary>
/// Editor utility class to help manage the different builds of the project.
/// </summary>
public class Builder
{
    private static readonly BuildTarget buildTarget = BuildTarget.StandaloneWindows64;
    private static readonly BuildOptions buildOptions = BuildOptions.None;

    [MenuItem("Window/Master Server Kit/Build master server", false, 100)]
    public static void BuildMasterServer()
    {
        var levels = new string[] { "Assets/CCGKit/Demo/Scenes/MSK_MasterServer.unity" };
        BuildPipeline.BuildPlayer(levels, "Builds/MasterServer.exe", buildTarget, buildOptions);
    }

    [MenuItem("Window/Master Server Kit/Build zone server", false, 100)]
    public static void BuildZoneServer()
    {
        var levels = new string[] { "Assets/CCGKit/Demo/Scenes/MSK_ZoneServer.unity" };
        BuildPipeline.BuildPlayer(levels, "Builds/ZoneServer.exe", buildTarget, buildOptions);
    }

    [MenuItem("Window/Master Server Kit/Build game server", false, 100)]
    public static void BuildGameServer()
    {
        var levels = new string[] {
            "Assets/CCGKit/Demo/Scenes/MSK_GameServer.unity",
            "Assets/CCGKit/Demo/Scenes/Game.unity"
        };
        BuildPipeline.BuildPlayer(levels, "Builds/GameServer.exe", buildTarget, buildOptions);
    }

    [MenuItem("Window/Master Server Kit/Build game client", false, 100)]
    public static void BuildGameClient()
    {
        var levels = new string[] {
            "Assets/CCGKit/Demo/Scenes/Home.unity",
            "Assets/CCGKit/Demo/Scenes/MSK_Home.unity",
            "Assets/CCGKit/Demo/Scenes/Lobby.unity",
            "Assets/CCGKit/Demo/Scenes/DeckBuilder.unity",
            "Assets/CCGKit/Demo/Scenes/Game.unity"
        };
        BuildPipeline.BuildPlayer(levels, "Builds/GameClient.exe", buildTarget, BuildOptions.None);
    }

    [MenuItem("Window/Master Server Kit/Build all", false, 50)]
    public static void BuildAll()
    {
        BuildMasterServer();
        BuildZoneServer();
        BuildGameServer();
        BuildGameClient();
    }
}

#endif
