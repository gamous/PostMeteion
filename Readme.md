# ZodicPost

FFXIV [Dalamud](https://github.com/goatcorp/Dalamud) Plugin 

Listen local port to receive command from http and execute it  in game.

Can work with ACT Triggernometry.

Still unstable version because multithread problem (TODO: lock), don't post command in 100ms twice :XD 

Repo:

- https://raw.githubusercontent.com/gamous/ZodiacPost/main/PluginMaster.json

Usage: 

- `/xpost` open control panel
- show settings
- set a lucky port number  and start
- Just post your command to `http://localhost:{port_you_set}/command` (Triggernometry Action: Post General JSON )

