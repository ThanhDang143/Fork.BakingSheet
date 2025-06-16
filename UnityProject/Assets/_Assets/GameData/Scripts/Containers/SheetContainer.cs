using Cathei.BakingSheet;
using Microsoft.Extensions.Logging;

namespace ThanhDV.Cathei.BakingSheet.Examples
{
    public partial class SheetContainer : SheetContainerBase
    {
        public SheetContainer(ILogger logger) : base(logger) { }

        public DemoSheet1 Demos1 { get; private set; }
        public DemoSheet2 Demos2 { get; private set; }
    }
}