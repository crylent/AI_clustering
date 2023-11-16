using System;
using System.Collections.Generic;
using NAudio.Wave;

namespace AI_clusterization.DataProcessing;

public class Spectrum
{
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

    public static Spectrum FromFile(string file, int bufferSize)
    {
        var stream = Utility.OpenWaveStream(file);
        var sampleProvider = stream.ToSampleProvider();
        var length = (int) stream.Length / (stream.WaveFormat.BitsPerSample / 8);
        var fullLength = length / bufferSize * bufferSize + bufferSize;
        var samples = new float[fullLength];
        sampleProvider.Read(samples, 0, length);

        var frames = fullLength / bufferSize;
        var powerBufferSize = bufferSize / 4;
        var power = new double[powerBufferSize];
        for (var i = 0; i < samples.Length; i += bufferSize)
        {
            var buffer = new double[bufferSize];
            Array.Copy(samples, i, buffer, 0, bufferSize);
            var frameSpectrum = FftSharp.FFT.ForwardReal(buffer);
            var framePower = FftSharp.FFT.Power(frameSpectrum);
            for (var j = 0; j < powerBufferSize; j++)
            {
                if (framePower[j] > MinPower) power[j] += framePower[j];
                else power[j] += MinPower;
            }
        }

        for (var j = 0; j < powerBufferSize; j++)
        {
            power[j] /= frames;
        }
        
        var frequencies = FftSharp.FFT.FrequencyScale(powerBufferSize, stream.WaveFormat.SampleRate);
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