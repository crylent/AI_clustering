using System.IO;
using System.Windows.Input;
using System.Windows.Media;
using NAudio.Wave;

namespace AI_clusterization.UI;

public partial class DatasetItem
{
    private readonly WaveOut _waveOut = new();
    
    private readonly string _file;
    private bool _isPlaying;
    
    public DatasetItem(string file)
    {
        _file = file;
        InitializeComponent();
        ItemName.Text = Path.GetFileName(file);
    }

    private void PlayOrStop(object sender, MouseButtonEventArgs e)
    {
        _isPlaying = !_isPlaying;
        if (_isPlaying)
        {
            _waveOut.Init(Utility.OpenWaveStream(_file));
            _waveOut.Play();
            _waveOut.PlaybackStopped += OnPlaybackStopped;
            Button.Source = Resources["StopSource"] as ImageSource;
        }
        else
        {
            _waveOut.Stop();
            OnPlaybackStopped();
        }
    }

    private void OnPlaybackStopped(object? sender, StoppedEventArgs e)
    {
        OnPlaybackStopped();
    }

    private void OnPlaybackStopped()
    {
        Button.Source = Resources["PlaySource"] as ImageSource;
    }
}