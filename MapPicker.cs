using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;

namespace MapPicker
{
  public struct MapInfo
  {
    public string MapName;
    public string MapId;
  }

  public static class Vote
  {

    public static MapVote CurrentHud { get; private set; }
    private static Dictionary<string, string> ClientVotes = new Dictionary<string, string>();
    private static Dictionary<string, int> MapVotes = new Dictionary<string, int>();
    private static List<MapInfo> MapInfos = new List<MapInfo>(); // Changed to a list

    public static void Init(List<MapInfo> maps)
    {
      Log.Info("Init Maps");
      MapInfos.Clear(); // Clear existing data before adding new maps
      foreach (var map in maps)
      {
        MapInfos.Add(map); // Use Add() method to add maps to the list
        MapVotes[map.MapId] = 0;
      }
    }

    public static void BeginVote()
    {
      Log.Info("Created Panel");

      // Your existing code...
      CurrentHud = new MapVote(
        MapInfos,
        MapVotes
      );
      Game.RootPanel = CurrentHud;
    }

    public static void EndVote()
    {
      Log.Info("Removed Panel");
      Game.RootPanel.DeleteChildren();
    }

    [ConCmd.Server]
    public static void VoteForMap(string mapId)
    {
      Log.Info("Clicked Vote!");
      var clientId = ConsoleSystem.Caller.ToString();

      if (clientId == null)
      {
        Log.Info("Command was called by non-client");
        return;
      }

      if (!MapInfos.Any(mapInfo => mapInfo.MapId == mapId))
      {
        Log.Info($"Invalid mapId: {mapId}");
        return;
      }

      if (ClientVotes.ContainsKey(clientId))
      {
        var previousVote = ClientVotes[clientId];

        // decrement vote count for previously voted map
        if (MapVotes.ContainsKey(previousVote))
        {
          MapVotes[previousVote]--;
        }
      }

      ClientVotes[clientId] = mapId; // Save vote against the clientId
                                     // increment vote count for the new voted map
      MapVotes[mapId]++;


      // MapVote.UpdateMapVote(
      //   MapVotes
      // );

      string serializedMapVotes = JsonSerializer.Serialize(MapVotes);
      MapVote.UpdateMapVote(serializedMapVotes);

    }

    [GameEvent.Server.ClientDisconnect]
    private static void ClientDisconnect(ClientDisconnectEvent e)
    {
      var clientId = e.Client.ToString();

      if (ClientVotes.ContainsKey(clientId))
      {
        var mapId = ClientVotes[clientId];
        MapVotes[mapId]--;
        ClientVotes.Remove(clientId);
      }

      Log.Info(e.Client);
    }

    [GameEvent.Server.ClientJoined]
    private static void ClientJoined(ClientJoinedEvent e)
    {
      Log.Info(e.Client);
    }

    public static List<MapInfo> GetMaps() // Changed the return type to List<MapInfo>
    {
      return MapInfos; // Return the list directly
    }

    public static int GetVoteCount(string mapId)
    {
      if (MapVotes.ContainsKey(mapId))
      {
        return MapVotes[mapId];
      }

      return 0;
    }
  }
}