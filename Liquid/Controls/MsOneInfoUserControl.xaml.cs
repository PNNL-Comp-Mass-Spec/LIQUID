using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Liquid.ViewModel;

namespace Liquid.Controls
{
	/// <summary>
	/// Interaction logic for MsOneInfoUserControl.xaml
	/// </summary>
	public partial class MsOneInfoUserControl : UserControl
	{
		public MsOneInfoViewModel MsOneInfoViewModel { get; private set; }

		public MsOneInfoUserControl()
		{
			InitializeComponent();

			this.MsOneInfoViewModel = new MsOneInfoViewModel();
			this.DataContext = this.MsOneInfoViewModel;
		}
	}
}
