using Nalix.Game.Domain.Models.Maps.Items;
using Nalix.Game.Domain.Models.Maps.NPCs;
using Nalix.Game.Domain.Models.Monsters;
using System.Collections.Generic;

namespace Nalix.Game.Domain.Models.Maps;

public class TileMap
{
    public ushort WidthInTiles { get; set; }   // Chiều rộng bản đồ theo số lượng tile
    public ushort HeightInTiles { get; set; }  // Chiều cao bản đồ theo số lượng tile
    public int WidthInPixels => WidthInTiles * 24; // Chiều rộng bản đồ theo pixel
    public int HeightInPixels => HeightInTiles * 24; // Chiều cao bản đồ theo pixel

    public int MapId { get; set; }             // ID của bản đồ
    public short OffsetX { get; set; }         // Tọa độ X ban đầu của bản đồ
    public short OffsetY { get; set; }         // Tọa độ Y ban đầu của bản đồ
    public string MapName { get; set; }        // Tên của bản đồ
    public byte ZoneCount { get; set; }        // Số khu vực trong bản đồ
    public byte MaxPlayers { get; set; }       // Số lượng người chơi tối đa
    public short TeleportId { get; set; }      // ID của điểm dịch chuyển (Teleport)
    public ushort[] TileIds { get; set; }      // Danh sách ID của các tile trong bản đồ
    public ushort[] TileTypes { get; set; }    // Danh sách loại của các tile
    public ushort BackgroundId { get; set; }   // ID của background
    public byte BackgroundType { get; set; }   // Loại background

    public List<Npc> Npcs { get; set; }       // Danh sách NPC trong bản đồ
    public List<WayPoint> WayPoints { get; set; } // Danh sách các điểm waypoint
    public List<Monster> Monsters { get; set; } // Danh sách bản đồ quái vật
    public List<BackgroundItem> BackgroundItems { get; set; } // Danh sách các item nền
    public List<ActionItem> ActionItems { get; set; }         // Danh sách hành động
}