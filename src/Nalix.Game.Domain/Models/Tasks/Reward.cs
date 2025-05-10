using Nalix.Game.Domain.Models.Items;
using System.Collections.Generic;

namespace Nalix.Game.Domain.Models.Tasks;

public class Reward
{
    public int Experience { get; set; }      // EXP nhận được
    public int Gold { get; set; }            // Vàng nhận được
    public int Diamonds { get; set; }        // Kim cương nhận được

    public List<Item> Items { get; set; } = []; // Danh sách vật phẩm nhận được
}