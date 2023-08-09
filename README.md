Easily add a map vote at the end of your game (or whenever you wish) with the map picker library. Supports both user defined map lists, and Asset.Party (experimental).

### Usage Example

```c#

public static async Task EndGame()
{
  // Define maps - ImageURL is optional.
  List<MapPicker.MapInfo> maps = new List<MapPicker.MapInfo>()
      {
          new MapPicker.MapInfo(){ Name = "Dust II", Id = "dust2" ImageURL="https://source.unsplash.com/random/200x200?sig=1" },
          new MapPicker.MapInfo(){ Name = "Inferno", Id = "inferno" },
          new MapPicker.MapInfo(){ Name = "Nuke", Id = "nuke" }
      };

  // Assign user maps.
  Vote.AddMapsFromList( maps );

  // Add maps from asset party
  await Vote.AddAssetPartyMapsUsingSearchString( "surf" );
  await Vote.AddAssetPartyMapsFromCollectionName( "xenthio.coolmaps" );

  // Begin vote when you're ready, providing the time in seconds you wish the vote to run for.
  // This will display map vote UI on all connected clients.
  Vote.BeginVote( 10 );
}

// Subscribe to completion event to retrieve the map ID that was top voted.
[Event( "MapPicker.VoteFinished" )]
public void OnVoteFinished( string Id )
{
  Log.Info( $"MapPicker.VoteFinished: {Id}" );
}
```

### Roadmap

- Add image preview [07.08.2023]
- Add Asset.Party support [07.08.2023]
- Add scrollbar (scroll works, but no scrollbar is visible)
- Add optional "Replay" or "Random" buttons.
- Add maps on the fly as admin via CMD.
- Add "disabled" flag for a map (perhaps they have been recently played, but you still want them shown in list).
- Ability to scale vote depending on client ID (useful for VIP, or scaling based on player level).

### Contributing
Feel free to open a PR to improve anything, whether that's a roadmap piece, or a better way to do things.
