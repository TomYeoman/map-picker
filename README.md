Easily add a map vote at the end of your game (or whenever you wish) with the map picker library.

```c#

// Define maps.
List<MapPicker.MapInfo> maps = new List<MapPicker.MapInfo>()
    {
        new MapPicker.MapInfo(){ Name = "Dust II", Id = "dust2" },
        new MapPicker.MapInfo(){ Name = "Inferno", Id = "inferno" },
        new MapPicker.MapInfo(){ Name = "Nuke", Id = "nuke" }
    };

// Assign maps.
Vote.Init( maps );

// Begin vote when you're ready, providing the time in seconds you wish the vote to run for.
// This will display map vote UI on all connected clients.
Vote.BeginVote( 10 );

// Subscribe to completion event to retrieve the map ID that was top voted.
[Event( "MapPicker.VoteFinished" )]
public void OnVoteFinished( string Id )
{
  Log.Info( $"MapPicker.VoteFinished: {Id}" );
}
```

**Roadmap**
- Add image support for maps.
- Add optional "Replay" or "Random" buttons.
- Add maps on the fly as admin via CMD.
- Ability to scale vote depending on client ID (useful for VIP, or scaling based on player level).

**Contribute**
Feel free to open a PR to improve anything, whether that's a roadmap piece, or a better way to do things.
