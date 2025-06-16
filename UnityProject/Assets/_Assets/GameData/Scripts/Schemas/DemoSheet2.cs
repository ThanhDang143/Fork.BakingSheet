using System;
using System.Collections.Generic;
using Cathei.BakingSheet;
using Cathei.BakingSheet.Unity;
using UnityEngine.AddressableAssets;

namespace ThanhDV.Cathei.BakingSheet.Examples
{
    public class DemoSheet2 : Sheet<DemoSheet2.Row>
    {
        // Row kế thừa từ SheetRowArray để có thể định nghĩa dữ liệu theo từng cấp độ (theo chiều dọc)
        public class Row : SheetRowArray<Elem>
        {
            public string Name { get; private set; }
            public CharacterRace Race { get; private set; }
            public BaseStats Stats { get; private set; } // Dữ liệu struct lồng nhau
            public string Description { get; private set; }
            public DateTime? ReleaseDate { get; private set; } // Kiểu nullable
            public AssetReference Icon { get; private set; } // Tham chiếu đến asset trong Resources
        }

        public class Elem : SheetRowElem
        {
            public DemoSheet1.Reference RefToSheet1 { get; private set; } // Tham chiếu đến sheet khác
            public long RequiredExp { get; private set; }
            public Dictionary<StatType, float> BonusStats { get; private set; } // Dữ liệu Dictionary
            public List<string> AcquiredSkills { get; private set; } // Dữ liệu List
        }
    }

    // Enum cho chủng tộc của nhân vật
    public enum CharacterRace
    {
        Human,
        Elf,
        Orc,
        Undead
    }

    // Enum cho các loại chỉ số
    public enum StatType
    {
        Attack,
        Defense,
        Speed
    }

    // Struct để chứa các chỉ số cơ bản
    public struct BaseStats
    {
        public int HP { get; private set; }
        public float AttackSpeed { get; private set; }
    }
}
