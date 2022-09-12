using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Dalamud.Logging;
using Dalamud.Data;
using Lumina.Excel;
using Lumina.Excel.GeneratedSheets;

namespace PostMeteion
{
    public class Status
    {
        public string DoQuery(string queryStr)
        {
            if (queryStr == "")
            {
                var errorMsg = "StatusQueryError:EmptyQuery";
                PluginLog.Debug(errorMsg);
                return errorMsg;
            }
            switch (queryStr.ToLower())
            {
                case "logged":
                case "isloggedin":
                    return $"{Svc.ClientState.IsLoggedIn}";
                case "cid":
                case "contentid":
                case "localcontentid":
                    return $"{Svc.ClientState.LocalContentId}";

                case "player":
                case "localplayer":
                    //todo
                    return $"{JsonConvert.SerializeObject(Svc.ClientState.LocalPlayer)}";
                case "targetobject":
                    //todo
                    return $"{JsonConvert.SerializeObject(Svc.ClientState.LocalPlayer?.TargetObject)}";
                case "currentworld":
                    return $"{Svc.ClientState.LocalPlayer?.CurrentWorld.GameData?.Name}";
                case "homeworld":
                    return $"{Svc.ClientState.LocalPlayer?.HomeWorld.GameData?.Name}";
                case "oid":
                case "objectid":
                    return $"{Svc.ClientState.LocalPlayer?.ObjectId}";
                case "tid":
                case "TargetObjectId":
                    return $"{Svc.ClientState.LocalPlayer?.TargetObjectId}";
                case "nid":
                case "nameid":
                    return $"{Svc.ClientState.LocalPlayer?.NameId}";
                case "name":
                    return $"{Svc.ClientState.LocalPlayer?.Name}";
                case "job":
                case "classjob":
                    return $"{Svc.ClientState.LocalPlayer?.ClassJob?.GameData?.Name}";
                case "hp":
                    return $"{Svc.ClientState.LocalPlayer?.CurrentHp}/{Svc.ClientState.LocalPlayer?.MaxHp}";
                case "mp":
                    return $"{Svc.ClientState.LocalPlayer?.CurrentMp}/{Svc.ClientState.LocalPlayer?.MaxMp}";
                case "gp":
                    return $"{Svc.ClientState.LocalPlayer?.CurrentGp}/{Svc.ClientState.LocalPlayer?.MaxGp}";
                case "cp":
                    return $"{Svc.ClientState.LocalPlayer?.CurrentCp}/{Svc.ClientState.LocalPlayer?.MaxCp}";
                case "buff":
                    return $"{String.Join("|",(from Status in (Svc.ClientState.LocalPlayer?.StatusList) select Status.GameData.Name.RawString).ToArray())}";
                case "pos":
                case "position":
                    return $"{Svc.ClientState.LocalPlayer?.Position}";
                case "rot":
                case "rotation":
                    return $"{Svc.ClientState.LocalPlayer?.Rotation}";
                case "terr":
                case "where":
                case "territorytype":
                    return $"{GetMapName(Svc.ClientState.TerritoryType)}";
                case "coord":
                    var Map = GetMap(Svc.ClientState.TerritoryType).Value;
                    var coordX = ConvertToMapCoordinate(Svc.ClientState.LocalPlayer.Position.X, Map.SizeFactor);
                    var coordY = ConvertToMapCoordinate(Svc.ClientState.LocalPlayer.Position.Z, Map.SizeFactor);
                    return $"X:{coordX:N1} Y:{coordY:N1}";
                default:
                    return "QueryNotFound";
            }
        }

        public static string GetMapName( uint territoryID){
            return Svc.Data.GetExcelSheet<TerritoryType>()?.GetRow(territoryID)?.PlaceName?.Value?.Name.ToString() ?? "location not found";
        }
        public static LazyRow<Map>? GetMap(uint territoryID)
        {
            return Svc.Data.GetExcelSheet<TerritoryType>()?.GetRow(territoryID)!.Map;
        }
        public static float ConvertToMapCoordinate(float pos, ushort sizeFactor)
        {
            // pos/tilescale + ((2048/(scale/100)/tilescale)/50)/2 +1
            // no accurate but enouth
            return (float)Math.Round( pos * 41 / 2048 + 2048 / sizeFactor + 1.2, 1);
        }
    }
}
