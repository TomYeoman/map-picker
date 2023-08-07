using Sandbox;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
// using System.Timers;

namespace MapPicker
{
  public struct MapInfo
  {
    public string MapName;
    public string MapId;

  }

  public static class Vote
  {
    public static TimeSince timeSinceVoteStarted;
    public static int VoteTime { get; set; }

    public static MapVote CurrentHud { get; private set; }
    // public static Timer VoteTimer { get; private set; }

    private static Dictionary<string, string> ClientVotes = new Dictionary<string, string>();
    private static Dictionary<string, int> MapVotes = new Dictionary<string, int>();
    private static List<MapInfo> MapInfos = new List<MapInfo>(); // Changed to a list

    private static bool voteInProgress = false;

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

    public static void BeginVote(int voteTime)
    {
      Log.Info("Begin Vote");
      Vote.VoteTime = voteTime;
      Vote.voteInProgress = true;

      if (Game.IsClient)
      {
        // Your existing code...
        CurrentHud = new MapVote(
          MapInfos,
          MapVotes
        );
        Game.RootPanel = CurrentHud;

      }
    }

    [GameEvent.Tick.Server]
    public static void OnTick()
    {
      if (!voteInProgress)
      {
        return;
      }

      if (timeSinceVoteStarted > Vote.VoteTime)
      {
        Log.Info($"Time since vote started: {timeSinceVoteStarted} has exceeded vote time: {Vote.VoteTime}");
        EndVote();

      }
      else
      {
        var remainingTime = Vote.VoteTime - timeSinceVoteStarted;
        Log.Info($"Time since vote started: {timeSinceVoteStarted} has not exceeded vote time: {Vote.VoteTime} remaining time: {remainingTime}");
        MapVote.UpdateVoteTimeRemaining(remainingTime);
      }
    }

    public static void EndVote()
    {
      Log.Info("Removed Panel");
      // Todo find correct way to remove panel
      Game.RootPanel = null;

      Vote.voteInProgress = false;
      var mapWithMostVotes = GetMapWithMostVotes();
      Event.Run("MapPicker.VoteFinished", mapWithMostVotes.MapId);
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

      Log.Info("Time called 1 was" + Time.Now);
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
        Log.Info($"Client {clientId} disconnected, removed vote for {mapId}");
      }
      else
      {
        Log.Info($"Client {clientId} disconnected, no vote to remove");
      }
    }

    [GameEvent.Server.ClientJoined]
    private static void ClientJoined(ClientJoinedEvent e)
    {
      var clientId = e.Client.ToString();
      Log.Info("Client Joined: " + clientId);
    }

    public static List<MapInfo> GetMaps() // Changed the return type to List<MapInfo>
    {
      return MapInfos; // Return the list directly
    }

    // Implement a method to get map with most votes using MapVotes
    public static MapInfo GetMapWithMostVotes()
    {
      var mapWithMostVotes = MapVotes.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
      return MapInfos.Find(map => map.MapId == mapWithMostVotes);
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