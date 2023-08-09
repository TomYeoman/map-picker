using Sandbox;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System;
// using System.Net.Http;
// using System.Xml;
// using System.Threading.Tasks;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MapPicker
{
  public struct MapInfo
  {
    public string Name;
    public string Id;
    public string ImageURL;
  }

  public static class Vote
  {
    public static TimeSince timeSinceVoteStarted;
    public static int VoteTime { get; set; }

    public static MapVote CurrentHud { get; private set; }

    private static Dictionary<string, string> ClientVotes = new Dictionary<string, string>();
    private static Dictionary<string, int> MapVotes = new Dictionary<string, int>();
    private static List<MapInfo> MapInfos = new List<MapInfo>(); // Changed to a list

    private static bool voteInProgress = false;

    public static void AddMapsFromList(List<MapInfo> maps)
    {
      Log.Info("Initializing maps in MapPicker");

      // Displaying the results
      foreach (var mapInfo in maps)
      {
        Log.Info($"Name: {mapInfo.Name}, Id: {mapInfo.Id}, ImageURL: {mapInfo.ImageURL}");
        MapInfos.Add(mapInfo); // Use Add() method to add maps to the list
        MapVotes[mapInfo.Id] = 0;
      }
    }

    public static async Task AddAssetPartyMapsUsingSearchString(string filter)
    {
      var query = $"type:map {filter}";
      var result = await Package.FindAsync(query, 100);
      Log.Info($"Found {result} packages");

      foreach (var package in result.Packages)
      {
        MapInfos.Add(new MapInfo { Name = package.Title, Id = package.FullIdent, ImageURL = package.Thumb });
        MapVotes[package.FullIdent] = 0;
      }
    }

    public static async Task AddAssetPartyMapsFromCollectionName(string filter)
    {
      var query = $"type:collection";
      var result = await Package.FindAsync(query, 100);
      Log.Info($"Found {result} packages");

      foreach (var package in result.Packages)
      {
        if (package.FullIdent == filter)
        {

          foreach (var map in package.PackageReferences)
          {
            var fullMapDetail = await Package.Fetch(map, false);
            MapInfos.Add(new MapInfo { Name = fullMapDetail.Title, Id = fullMapDetail.FullIdent, ImageURL = fullMapDetail.Thumb });
            MapVotes[fullMapDetail.FullIdent] = 0;
          }
        }
      }
    }

    public static void BeginVote(int voteTime)
    {

      if (voteInProgress)
      {
        Log.Info("Vote already in progress");
        return;
      }

      if (!MapInfos.Any())
      {
        Log.Info("No maps found");
        return;
      }

      Log.Info("Beginning Vote");
      Vote.VoteTime = voteTime;
      Vote.voteInProgress = true;

      if (Game.IsClient)
      {
        // Your existing code...
        CurrentHud = new MapVote(
          MapInfos,
          MapVotes
        );
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
        // Log.Info($"Time since vote started: {timeSinceVoteStarted} has exceeded vote time: {Vote.VoteTime}");
        EndVote();
      }
      else
      {
        var remainingTime = Vote.VoteTime - timeSinceVoteStarted;
        MapVote.UpdateVoteTimeRemaining(remainingTime);
      }
    }

    public static void EndVote()
    {
      Vote.voteInProgress = false;
      var mapWithMostVotes = GetMapWithMostVotes();
      Event.Run("MapPicker.VoteFinished", mapWithMostVotes.Id);
      MapInfos.Clear(); // Clear existing data before adding new maps
      // UI cleanuip
      MapVote.EndVote();
    }

    [ConCmd.Server]
    public static void VoteForMap(string Id)
    {
      var clientId = ConsoleSystem.Caller.ToString();

      if (clientId == null)
      {
        Log.Info("Command was called by non-client");
        return;
      }

      if (!MapInfos.Any(mapInfo => mapInfo.Id == Id))
      {
        Log.Info($"Invalid Id: {Id}");
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

      ClientVotes[clientId] = Id; // Save vote against the clientId
                                  // increment vote count for the n
                                  // increment vote count for the new voted map
      MapVotes[Id]++;
      string serializedMapVotes = JsonSerializer.Serialize(MapVotes);
      MapVote.UpdateMapVote(serializedMapVotes);

    }

    [GameEvent.Server.ClientDisconnect]
    private static void ClientDisconnect(ClientDisconnectEvent e)
    {
      var clientId = e.Client.ToString();
      if (ClientVotes.ContainsKey(clientId))
      {
        var Id = ClientVotes[clientId];
        MapVotes[Id]--;
        ClientVotes.Remove(clientId);
      }
    }

    [GameEvent.Server.ClientJoined]
    private static void ClientJoined(ClientJoinedEvent e)
    {
      var clientId = e.Client.ToString();
    }

    public static List<MapInfo> GetMaps() // Changed the return type to List<MapInfo>
    {
      return MapInfos; // Return the list directly
    }

    // Implement a method to get map with most votes using MapVotes
    public static MapInfo GetMapWithMostVotes()
    {
      // Check if all maps have zero votes
      if (MapVotes.All(mv => mv.Value == 0))
      {
        // Return a random map if none have votes
        Random rng = new Random();
        int randomIndex = rng.Next(MapInfos.Count);
        return MapInfos[randomIndex];
      }
      else
      {
        // Return the map with the most votes
        var mapWithMostVotes = MapVotes.Aggregate((l, r) => l.Value > r.Value ? l : r).Key;
        return MapInfos.Find(map => map.Id == mapWithMostVotes);
      }
    }

    public static int GetVoteCount(string Id)
    {
      if (MapVotes.ContainsKey(Id))
      {
        return MapVotes[Id];
      }

      return 0;
    }
  }
}