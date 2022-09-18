# PostMetion

FFXIV [Dalamud](https://github.com/goatcorp/Dalamud) Plugin which receive instructions and report information like Meteion.

Listen local port to receive command from http and execute it in game just like what [PostNamazu](https://github.com/Natsukage/PostNamazu) did.

Can work with ACT [Triggernometry](https://github.com/paissaheavyindustries/Triggernometry).

In addition, PostMetion also maintain a set of delegate to post realtime ingame information to webhook.

A python trigger framework [**Noetophoreon**](https://github.com/gamous/Noetophoreon) is provided about how to handle the post between PostMetion.

### Install

Add repo: `https://raw.githubusercontent.com/gamous/PostMeteion/main/PluginMaster.json`  to import single plugin to your dalamud.

Or use another [repo](https://github.com/gamous/DalamudPluginsCN-Dev) if gaming with Chinese client: `https://raw.githubusercontent.com/gamous/DalamudPluginsCN-Dev/main/PluginMaster.json` 

### Usage

- `/xpost` open control panel
- show settings
- set a lucky port number and start
- Just post your command to `http://localhost:{port_you_set}/command` (Triggernometry Action: Post General JSON )

## Wiki

[API Doc](https://github.com/gamous/PostMeteion/wiki)

### Thanks

- [XivCommon](https://git.sr.ht/~jkcclemens/XivCommon)
- [PostNamazu](https://github.com/Natsukage/PostNamazu)
