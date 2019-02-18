using System.Windows.Controls;
using Liquid.ViewModel;

namespace Liquid.Controls
{
    /// <summary>
    /// Interaction logic for MsOneInfoUserControl.xaml
    /// </summary>
    public partial class MsOneInfoUserControl
    {
        public MsOneInfoViewModel MsOneInfoViewModel { get; }

        public MsOneInfoUserControl()
        {
            InitializeComponent();

            MsOneInfoViewModel = new MsOneInfoViewModel();
            DataContext = MsOneInfoViewModel;
        }
    }
}
