# ZodicPost

FFXIV [Dalamud]([goatcorp/Dalamud: FFXIV plugin framework and API (github.com)](https://github.com/goatcorp/Dalamud)) Plugin 

Listen local port to receive command from http and execute it  in game.

Can work with ACT Triggernometry.

Still unstable version because multithread problem (TODO: lock), don't post command in 100ms twice :XD 

Usage: 

- `/xpost` open control panel
- show settings
- set a lucky port number  and start
- Just post your command to `http://localhost:{port_you_set}/command` (Triggernometry Action: Post General JSON )

