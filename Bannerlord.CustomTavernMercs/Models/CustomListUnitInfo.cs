using System.Collections.Generic;

namespace Bannerlord.CustomTavernMercs
{
    public class CustomListUnitInfo
    {
        public string TroopCharacterId { get; set; }
        public bool CustomCost { get; set; }
        public int Cost { get; set; }
        public bool Global { get; set; }
        public List<string> Towns { get; set; } = new List<string>();
        public List<string> Cultures { get; set; } = new List<string>();


    }
}
