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
	/// Interaction logic for MsMsInfoUserControl.xaml
	/// </summary>
	public partial class MsMsInfoUserControl
	{
		public MsMsInfoViewModel MsMsInfoViewModel { get; private set; }

		public MsMsInfoUserControl()
		{
			InitializeComponent();

			this.MsMsInfoViewModel = new MsMsInfoViewModel();
			this.DataContext = this.MsMsInfoViewModel;
		}

	    private void CopyCIDSpectra(object sender, RoutedEventArgs e)
	    {
	        var peaks = this.MsMsInfoViewModel.CurrentSpectrumSearchResult.CidSpectrum.Peaks;
	        string spectrumString = "Mz\tIntensity\n";
	        foreach (var peak in peaks)
	        {
	            spectrumString += String.Format("{0}\t{1}\n", peak.Mz, peak.Intensity);
	        }
            Clipboard.SetText(spectrumString);
	    }

        private void CopyHCDSpectra(object sender, RoutedEventArgs e)
        {
            var peaks = this.MsMsInfoViewModel.CurrentSpectrumSearchResult.HcdSpectrum.Peaks;
            string spectrumString = "Mz\tIntensity\n";
            foreach (var peak in peaks)
            {
                spectrumString += String.Format("{0}\t{1}\n", peak.Mz, peak.Intensity);
            }
            Clipboard.SetText(spectrumString);
        }
	}
}
