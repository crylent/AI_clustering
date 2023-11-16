using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Media;
using AI_clusterization.DataProcessing;

namespace AI_clusterization.UI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private const double MaxFrequency = 20000;
        private const double PlotExp = 10;

        private static readonly Brush[] ClusterColors =
        {
            Brushes.LightPink,
            Brushes.LightGreen, 
            Brushes.LightBlue,
            Brushes.LightYellow
        };

        private Dataset _dataset = null!;
        private List<DatasetItem> _items = null!;

        private int _bufferSize = 65536;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void ShowSpectrum(Spectrum spectrum)
        {
            Plot.Plot.Clear();
            
            Plot.Plot.XAxis.SetBoundary(0, Math.Log(MaxFrequency, PlotExp));
            Plot.Plot.YAxis.SetBoundary(Spectrum.MinPower, 0);

            var zeroLine = Enumerable.Repeat(Spectrum.MinPower, spectrum.Length).ToArray();
            var logFreq = spectrum.Frequencies.Select(x => x > 0 ? Math.Log(x, PlotExp) : 0).ToArray();
            Plot.Plot.AddFill(logFreq, zeroLine, spectrum.Power.ToArray());
            Plot.Plot.XAxis.TickLabelFormat(x => Math.Pow(PlotExp, x).ToString("N0"));
            
            Plot.Refresh();
        }

        private void SelectDataset(object sender, RoutedEventArgs e)
        {
            using var dialog = new FolderBrowserDialog();
            var result = dialog.ShowDialog();
            if (result != System.Windows.Forms.DialogResult.OK) return;

            DatasetPath.Text = dialog.SelectedPath;
            DatasetItems.Items.Clear();
            
            var files = Directory.GetFiles(dialog.SelectedPath);

            _dataset = new Dataset();
            foreach (var file in files)
            {
                Debug.Print(file);
                var spectrum = Spectrum.FromFile(file, _bufferSize);
                _dataset.Add(spectrum);
                DatasetItems.Items.Add(new DatasetItem(file));
            }

            // save initial order of items
            _items = DatasetItems.Items.OfType<DatasetItem>().ToList();
        }

        private void SelectItem(object sender, SelectionChangedEventArgs e)
        {
            if (DatasetItems.SelectedItem == null) return;
            var name = (DatasetItems.SelectedItem as DatasetItem)!.ItemName.Text;
            for (var i = 0; i < _items.Count; i++)
            {
                if (_items[i].ItemName.Text == name)
                {
                    ShowSpectrum(_dataset[i]);
                }
            }
        }

        private void RunClusterization(object sender, RoutedEventArgs e)
        {
            var clusters = Clusterization.Run(_dataset, Convert.ToInt32(Clusters.Text));
            
            DatasetItems.Items.Clear();
            for (var i = 0; i < clusters.Count; i++)
            {
                var cluster = clusters[i];
                DatasetItems.Items.Add(new DatasetCluster(i));
                foreach (var index in cluster.Select(spectrum => _dataset.IndexOf(spectrum)))
                {
                    DatasetItems.Items.Add(_items[index]);
                    _items[index].Background = ClusterColors[i % ClusterColors.Length];
                }
            }
        }

        private void EditBufferSize(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _bufferSize = (int) Math.Pow(2, e.NewValue);
            BufferSize.Text = _bufferSize.ToString();
        }
    }
}