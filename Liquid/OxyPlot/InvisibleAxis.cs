using OxyPlot;
using OxyPlot.Axes;

namespace Liquid.OxyPlot
{
    public class InvisibleAxis : LinearAxis
    {
        public InvisibleAxis(AxisPosition axisPosition, string title)
        {
            Position = axisPosition;
            Title = title;
        }

        public override bool IsXyAxis()
        {
            return true;
        }
    }
}
