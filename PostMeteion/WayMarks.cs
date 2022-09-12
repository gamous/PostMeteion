using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Diagnostics.CodeAnalysis;
using Dalamud.Game;
using Dalamud.Logging;
using Dalamud.Memory;

namespace PostMeteion
{
    public class WayMark
    {
        public class Waymark
        {
            public float X { get; set; }
            public float Y { get; set; }
            public float Z { get; set; }
            public WaymarkID ID { get; set; }
            public bool Active { get; set; }
        }
        public enum WaymarkID : byte { A = 0, B, C, D, One, Two, Three, Four }
        public class WayMarks
        {
            public Waymark? A { get; set; }
            public Waymark? B { get; set; }
            public Waymark? C { get; set; }
            public Waymark? D { get; set; }
            public Waymark? One { get; set; }
            public Waymark? Two { get; set; }
            public Waymark? Three { get; set; }
            public Waymark? Four { get; set; }
        }

        private WayMarks? tempMarks; //暂存场地标点
        private IntPtr MarkingController;
        private IntPtr Waymarks;

        private static class Signatures
        {
            internal const string MarkingController = "48 8B 94 24 ?? ?? ?? ?? 48 8D 0D ?? ?? ?? ?? 41 B0 01";//"48 8d 0d ?? ?? ?? ?? e8 ?? ?? ?? ?? 48 3b c3 75 ?? ff c7 3b fe";//
        }
        internal WayMark()
        {

            try
            {

                MarkingController = Svc.SigScanner.GetStaticAddressFromSig(Signatures.MarkingController);
                Waymarks = MarkingController + 0x1b0;
            }
            catch (KeyNotFoundException)
            {
                PluginLog.Warning("Could not find signature for WayMark. This functionality will be disabled.");
            }
        }
        public string DoWaymarks(string waymarksStr)
        {
            if (waymarksStr == "") {
                var errorMsg = "WayMarkError:EmptyCommand";
                PluginLog.Debug(errorMsg);
                return errorMsg; 
            }
            if (MarkingController == IntPtr.Zero)
            {
                var errorMsg = "WayMarkError:SigNotFound";
                PluginLog.Debug(errorMsg);
                return errorMsg;
            }
            switch (waymarksStr.ToLower())
            {
                case "save":
                case "backup":
                    SaveWaymark();
                    return "Saved";
                case "load":
                case "restore":
                    LoadWaymark();
                    return "Restored";
                case "clear":
                    WriteWaymarks(new WayMarks { A = new Waymark(), B = new Waymark(), C = new Waymark(), D = new Waymark(), One = new Waymark(), Two = new Waymark(), Three = new Waymark(), Four = new Waymark() });
                    return "Cleared";
                case "now":
                    return ExportWaymark();
                case "tmp":
                    return ExportTempWaymark();
                default:
                    WayMarks? waymarks = JsonConvert.DeserializeObject<WayMarks>(waymarksStr);
                    PluginLog.Debug(waymarksStr+"\n"+ JsonConvert.SerializeObject(waymarks));
                    if(waymarks is not null)
                    {
                        WriteWaymarks(waymarks);
                        return "Placed";
                    }else
                    {
                        return "PlzCheckPayload";
                    }
            }
        }

        public WayMarks ReadWaymarks()
        {
            WayMarks temp= new WayMarks();

            Waymark ReadWaymark(IntPtr addr, WaymarkID id) => new()
            {
                X = MemoryHelper.Read<float>(addr),
                Y = MemoryHelper.Read<float>(addr + 0x4),
                Z = MemoryHelper.Read<float>(addr + 0x8),
                Active = MemoryHelper.Read<byte>(addr + 0x1C) == 1,
                ID = id
            };
            try
            {
                temp.A = ReadWaymark(Waymarks + 0x00, WaymarkID.A);
                temp.B = ReadWaymark(Waymarks + 0x20, WaymarkID.B);
                temp.C = ReadWaymark(Waymarks + 0x40, WaymarkID.C);
                temp.D = ReadWaymark(Waymarks + 0x60, WaymarkID.D);
                temp.One = ReadWaymark(Waymarks + 0x80, WaymarkID.One);
                temp.Two = ReadWaymark(Waymarks + 0xA0, WaymarkID.Two);
                temp.Three = ReadWaymark(Waymarks + 0xC0, WaymarkID.Three);
                temp.Four = ReadWaymark(Waymarks + 0xE0, WaymarkID.Four);
                PluginLog.Debug("ReadWaymark");
            }
            catch (Exception ex)
            {
                PluginLog.Error("ReadWaymarkWorng:" + ex.Message);
            }
            return temp;
        }
        public string ExportWaymark()
        {
            PluginLog.Debug("ExportWaymark");
            WayMarks now = ReadWaymarks();
            string json = JsonConvert.SerializeObject(now);
            return json;
        }
        public string ExportTempWaymark()
        {
            //PluginLog.Debug("ExportTempWaymark");
            string json = JsonConvert.SerializeObject(tempMarks);
            return json;
        }

        public void SaveWaymark()
        {
            PluginLog.Debug("SaveWaymark");
            tempMarks = ReadWaymarks();
        }

        public void LoadWaymark()
        {
            if (tempMarks is null)
                return;
            WriteWaymarks(tempMarks);
            PluginLog.Debug("RestoreWaymark");
        }
        private void WriteWaymarks(WayMarks waymarks)
        {
            WriteWaymark(waymarks.A, 0);
            WriteWaymark(waymarks.B, 1);
            WriteWaymark(waymarks.C, 2);
            WriteWaymark(waymarks.D, 3);
            WriteWaymark(waymarks.One, 4);
            WriteWaymark(waymarks.Two, 5);
            WriteWaymark(waymarks.Three, 6);
            WriteWaymark(waymarks.Four, 7);
        }
        private void WriteWaymark(Waymark? waymark, int id = -1)
        {
            if (waymark is null)
                return;

            var wId = id == -1 ? (byte)waymark.ID : id;

            var markAddr = wId switch
            {
                (int)WaymarkID.A     => Waymarks + 0x00,
                (int)WaymarkID.B     => Waymarks + 0x20,
                (int)WaymarkID.C     => Waymarks + 0x40,
                (int)WaymarkID.D     => Waymarks + 0x60,
                (int)WaymarkID.One   => Waymarks + 0x80,
                (int)WaymarkID.Two   => Waymarks + 0xA0,
                (int)WaymarkID.Three => Waymarks + 0xC0,
                (int)WaymarkID.Four  => Waymarks + 0xE0,
                _ => IntPtr.Zero
            };
            if (markAddr == IntPtr.Zero)
            {
                return;
            }
            PluginLog.Debug($"write at waymark.{wId} {Waymarks:x}->{markAddr:x} \n{JsonConvert.SerializeObject(waymark)}");
            // Write the X, Y and Z coordinates
            MemoryHelper.Write(markAddr + 0x0, waymark.X);
            MemoryHelper.Write(markAddr + 0x4, waymark.Y);
            MemoryHelper.Write(markAddr + 0x8, waymark.Z);
            //
            MemoryHelper.Write(markAddr + 0x10, (int)(waymark.X * 1000));
            MemoryHelper.Write(markAddr + 0x14, (int)(waymark.Y * 1000));
            MemoryHelper.Write(markAddr + 0x18, (int)(waymark.Z * 1000));
            //
            //// Write the active state
            MemoryHelper.Write(markAddr + 0x1C, (byte)(waymark.Active ? 1 : 0),false);
        }

    }
}
