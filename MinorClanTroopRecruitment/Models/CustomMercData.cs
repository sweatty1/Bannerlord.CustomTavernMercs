using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaleWorlds.CampaignSystem;

namespace MinorClanTroopRecruitment
{
    public class CustomMercData
    {
        public string TroopCharacterId { get; set; }
        public bool CustomCost { get; set; }
        public int Cost { get; set; }
        public bool Global { get; set; }
        public List<string> Towns { get; set; } = new List<string>();
        public List<string> Cultures { get; set; } = new List<string>();


    }
}
