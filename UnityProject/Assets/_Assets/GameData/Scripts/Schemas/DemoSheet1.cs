using Cathei.BakingSheet;

namespace ThanhDV.Cathei.BakingSheet.Examples
{
    public class DemoSheet1 : Sheet<DemoSheet1.Row>
    {
        public class Row : SheetRow
        {
            public string Name { get; private set; }
        }
    }
}
