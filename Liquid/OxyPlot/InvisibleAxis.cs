using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

		public override OxySize Measure(IRenderContext rc)
		{
			return new OxySize(0, 0);
		}
	}
}
