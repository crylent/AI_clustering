using System;
using System.IO;
using NAudio.Wave;

namespace AI_clusterization;

public static class Utility
{
    private const string ExtensionWav = ".wav";
    private const string ExtensionMp3 = ".mp3";
    
    public static WaveStream OpenWaveStream(string file)
    {
        var extension = Path.GetExtension(file);
        WaveStream stream = extension switch
        {
            ExtensionWav => new WaveFileReader(file),
            ExtensionMp3 => new Mp3FileReader(file),
            _ => throw new ArgumentException("Unacceptable extension")
        };
        return stream;
    }
}