using System;
using System.Collections.Generic;
using NAudio.Wave;

namespace AI_clusterization.DataProcessing;

public class Spectrum
{
    private const int BufferSize = 65536;
    private const int PowerBufferSize = BufferSize / 4;
    
    public const double MinPower = -90;

    private readonly double[] _frequencies;
    private readonly double[] _power;

    public IReadOnlyList<double> Frequencies => _frequencies;
    public IReadOnlyList<double> Power => _power;
    public int Length => _power.Length;

    private Spectrum(double[] frequencies, double[] power)
    {
        _frequencies = frequencies;
        _power = power;
    }

    public static Spectrum FromFile(string file)
    {
        var stream = Utility.OpenWaveStream(file);
        var sampleProvider = stream.ToSampleProvider();
        var length = (int) stream.Length / (stream.WaveFormat.BitsPerSample / 8);
        var fullLength = length / BufferSize * BufferSize + BufferSize;
        var samples = new float[fullLength];
        sampleProvider.Read(samples, 0, length);

        var frames = fullLength / BufferSize;
        var power = new double[PowerBufferSize];
        for (var i = 0; i < samples.Length; i += BufferSize)
        {
            var buffer = new double[BufferSize];
            Array.Copy(samples, i, buffer, 0, BufferSize);
            var frameSpectrum = FftSharp.FFT.ForwardReal(buffer);
            var framePower = FftSharp.FFT.Power(frameSpectrum);
            for (var j = 0; j < PowerBufferSize; j++)
            {
                if (framePower[j] > MinPower) power[j] += framePower[j];
                else power[j] += MinPower;
            }
        }

        for (var j = 0; j < PowerBufferSize; j++)
        {
            power[j] /= frames;
        }
        
        var frequencies = FftSharp.FFT.FrequencyScale(PowerBufferSize, stream.WaveFormat.SampleRate);
        return new Spectrum(frequencies, power);
    }

    public double DistanceTo(IReadOnlyList<double> other)
    {
        if (Length != other.Count) throw new ArgumentException("Spectra must be the same length");
        
        var dist = 0.0;
        for (var i = 0; i < Length; i++)
        {
            dist += double.Pow(Power[i] - other[i], 2);
        }
        return dist / Length;
    }
}