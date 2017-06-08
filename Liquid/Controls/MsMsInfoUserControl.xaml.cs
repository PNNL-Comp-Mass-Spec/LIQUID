using System;
using System.Windows;
using Liquid.ViewModel;

namespace Liquid.Controls
{
    /// <summary>
    /// Interaction logic for MsMsInfoUserControl.xaml
    /// </summary>
    public partial class MsMsInfoUserControl
    {
        public MsMsInfoViewModel MsMsInfoViewModel { get; }

        public MsMsInfoUserControl()
        {
            InitializeComponent();

            MsMsInfoViewModel = new MsMsInfoViewModel();
            DataContext = MsMsInfoViewModel;
        }

        private void CopyCIDSpectra(object sender, RoutedEventArgs e)
        {
            var peaks = MsMsInfoViewModel.CurrentSpectrumSearchResult.CidSpectrum.Peaks;
            var spectrumString = "Mz\tIntensity\n";
            foreach (var peak in peaks)
            {
                spectrumString += String.Format("{0}\t{1}\n", peak.Mz, peak.Intensity);
            }
            Clipboard.SetText(spectrumString);
        }

        private void CopyHCDSpectra(object sender, RoutedEventArgs e)
        {
            var peaks = MsMsInfoViewModel.CurrentSpectrumSearchResult.HcdSpectrum.Peaks;
            var spectrumString = "Mz\tIntensity\n";
            foreach (var peak in peaks)
            {
                spectrumString += String.Format("{0}\t{1}\n", peak.Mz, peak.Intensity);
            }
            Clipboard.SetText(spectrumString);
        }
    }
}
