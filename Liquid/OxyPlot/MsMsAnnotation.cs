using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
