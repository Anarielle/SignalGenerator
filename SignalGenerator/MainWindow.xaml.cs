﻿using MathNet.Numerics;
using MathNet.Numerics.IntegralTransforms;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
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
using System.Windows.Threading;

namespace SignalGenerator
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
        private readonly Signal signal = new Signal(1, 0, 0.1, SignalType.Harmonic);

        public MainWindow()
        {
            this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            InitializeComponent();
            
            rbHarmonic.IsChecked = true;
        }

        public void Initialize()
        {
            pOriginalSignal.Plot.Title("Исходный сигнал");
            pRecievedSignal.Plot.Title("Полученный сигнал");
            pHarmonics.Plot.Title("Гармоники");
            pSpectrum.Plot.Title("Спектр исходного сигнала");

            pOriginalSignal.Plot.SetAxisLimits(0, 50, -1, 1);
            pOriginalSignal.Plot.SetOuterViewLimits(0, 10000);
            pHarmonics.Plot.SetAxisLimits(0, 50, -1, 1);
            pHarmonics.Plot.SetOuterViewLimits(0, 10000);
            pSpectrum.Plot.SetAxisLimits(0, 20, -1, 1);
            pSpectrum.Plot.SetOuterViewLimits(0, 10000);
            pRecievedSignal.Plot.SetAxisLimits(0, 0.1, -1, 1);
            pRecievedSignal.Plot.SetOuterViewLimits(0, 10000);
        }

        public void AddDataPoint()
        {
            pHarmonics.Reset();
            pOriginalSignal.Reset();
            pSpectrum.Reset();
            pRecievedSignal.Reset();

            Initialize();

            var func1 = signal.BuildSignal();

            var firstHarmonica = new Func<double, double?>((x) => signal.Amplitude * Math.Sin(2 * Math.PI * signal.Frequency / 10 * x + signal.Phase));
            var secondHarmonica = new Func<double, double?>((x) => signal.Amplitude / 3 * Math.Sin(2 * Math.PI * signal.Frequency / 10 * 3 * x + signal.Phase));
            var thirdHarmonica = new Func<double, double?>((x) => signal.Amplitude / 5 * Math.Sin(2 * Math.PI * signal.Frequency / 10 * 5 * x + signal.Phase));

            pOriginalSignal.Plot.AddFunction(func1);
            pHarmonics.Plot.AddFunction(firstHarmonica);
            pHarmonics.Plot.AddFunction(secondHarmonica);
            pHarmonics.Plot.AddFunction(thirdHarmonica);

            double[] dataY = new double[10000];
            Complex[] samples = new Complex[10000];
            double[] time = new double[10000];

            for (int i = 0; i < 10000; i++)
            {
                samples[i] = new Complex((double)(func1.Invoke(i) + firstHarmonica.Invoke(i) + secondHarmonica.Invoke(i)), 0);
                dataY[i] = (double)(func1.Invoke(i) + firstHarmonica.Invoke(i) + secondHarmonica.Invoke(i));
            }

            for (int i = 0; i < 10000; i++)
            {
                time[i] = ((i + 1.0) / 1000) / 2;
            }
            pRecievedSignal.Plot.AddScatter(time, dataY);

            Fourier.Forward(samples, FourierOptions.NoScaling);


            double[] mag = new double[1000];
            double[] hzPerSeconds = new double[1000];

            for (int i = 0; i < 1000; i++)
            {
                mag[i] = (2.0 / 1000) * (Math.Abs(Math.Sqrt(Math.Pow(samples[i].Real, 2) + Math.Pow(samples[i].Imaginary, 2))));
                hzPerSeconds[i] = 441000 / 10000 * i;
            }

            pSpectrum.Plot.AddScatter(hzPerSeconds, mag);

            pOriginalSignal.Refresh();
            pHarmonics.Refresh();
            pSpectrum.Refresh();
            pRecievedSignal.Refresh();
        }

        private void mRealTime_Click(object sender, RoutedEventArgs e)
        {
            new RealTime().Show();
        }

        private void rbHarmonic_Checked(object sender, RoutedEventArgs e)
        {
            signal.SignalType = SignalType.Harmonic;
            AddDataPoint();
        }

        private void rbSquare_Checked(object sender, RoutedEventArgs e)
        {
            signal.SignalType = SignalType.Square;
            AddDataPoint();
        }

        private void rbTriangle_Checked(object sender, RoutedEventArgs e)
        {
            signal.SignalType = SignalType.Triangle;
            AddDataPoint();
        }

        private void sAmplitude_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            signal.Amplitude = sAmplitude.Value;
            AddDataPoint();
        }

        private void sFrequency_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            signal.Frequency = sFrequency.Value;
            AddDataPoint();
        }

        private void sPhase_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            signal.Phase = sPhase.Value * Math.PI / 180;
            AddDataPoint();
        }
    }
}
