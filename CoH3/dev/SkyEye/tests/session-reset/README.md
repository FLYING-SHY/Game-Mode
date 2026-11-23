# Session-reset fixture

From the repository root:

```powershell
& .\CoH3\dev\SkyEye\tests\session-reset\Run.ps1
```

The net48/x64 fixture source-links each active client separately into this test directory. It executes the real production `ClearSessionData`, `AcceptSnapshot`, and `DropReader` methods by reflection. Both form constructors are skipped. Only required dependencies are created, with every overlay set to `track:false`. The fixture calls no game reader constructor, process discovery, client startup, credentials, or native game methods, and opens no window handles or renderer threads.

Coverage: full reset and repeat reset; support identity/seen/active/baseline; both enemy selections; alert baseline and caches; armour initialized/seen and reused-job alerts after same-key reconnect; old-reader disposal using a managed, handle-free reader identity and a new reader with the same key; successful fresh rows retained; same-reader A-to-B change retains reader/pid; same-match polls retain choices/dedupe; persistent preferences retained; production IL ordering of Snapshot -> AcceptSnapshot -> first layer publication. A source input SHA256 manifest and per-client logs are written locally.

This is F-level offline lifecycle evidence. GUI presentation and actual-game validation remain `not_run`. The copied before sources are excluded from compilation via `EnableDefaultCompileItems=false` and explicit production source includes.
