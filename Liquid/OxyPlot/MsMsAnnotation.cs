using LiquidBackend.Domain;
using OxyPlot.Annotations;

namespace Liquid.OxyPlot
{
	public class MsMsAnnotation : TextAnnotation
	{
		public FragmentationType FragmentionType { get; set; }

		public MsMsAnnotation(FragmentationType fragmentationType)
		{
			this.FragmentionType = fragmentationType;
		}
	}
}
