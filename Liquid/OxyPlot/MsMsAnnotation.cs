using LiquidBackend.Domain;
using OxyPlot.Annotations;

namespace Liquid.OxyPlot
{
    public class MsMsAnnotation : TextAnnotation
    {
        public FragmentationType FragmentationType { get; set; }

        public MsMsAnnotation(FragmentationType fragmentationType)
        {
            FragmentationType = fragmentationType;
        }
    }
}
