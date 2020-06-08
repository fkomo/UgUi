using System;
using Ujeby.Common.Tools;
using Ujeby.Common.Tools.Types;
using Ujeby.UgUi.Nodes;

namespace Ujeby.UgUi.Nodes.Generators
{
	// TODO NODE PerlinNoiseGenerator

	//[FunctionInfo]
	//[IgnoredProperty(Name = nameof(Generator.Value))]
	//public class PerlinNoiseGenerator : RandomGenerator
	//{
	//	private double frequency = 0.0625;
	//	private double amplitude = 1.0;
	//	private double persistance = 0.5;
	//	private int octaves = 16;

	//	[Input(InputAnchor = true)]
	//	public double Frequency
	//	{
	//		get { return frequency; }
	//		set { SetField(ref frequency, value, nameof(Frequency)); }
	//	}

	//	[Input(InputAnchor = true)]
	//	public double Amplitude
	//	{
	//		get { return amplitude; }
	//		set { SetField(ref amplitude, value, nameof(Amplitude)); }
	//	}

	//	[Input(InputAnchor = true)]
	//	public double Persistance
	//	{
	//		get { return persistance; }
	//		set { SetField(ref persistance, value, nameof(Persistance)); }
	//	}

	//	[Input(InputAnchor = true)]
	//	public int Octaves
	//	{
	//		get { return octaves; }
	//		set { SetField(ref octaves, value, nameof(Octaves)); }
	//	}

	//	public override void Execute()
	//	{
	//		using (new TimedBlock($"{ typeof(PerlinNoiseGenerator).Name }.{ Utils.GetCurrentMethodName() }"))
	//		{
	//			var rnd = Rnd;

	//			var noise = new double[(int)OutputDimensions.Width, (int)OutputDimensions.Height];
	//			for (var x = 0; x < OutputDimensions.Width; ++x)
	//				for (var y = 0; y < OutputDimensions.Height; ++y)
	//					noise[x, y] = (rnd.NextDouble() - 0.5) * 2.0;

	//			Output = new v4[(int)OutputDimensions.Height * (int)OutputDimensions.Width];
	//			for (var y = 0; y < (int)OutputDimensions.Height; ++y)
	//				for (var x = 0; x < (int)OutputDimensions.Width; ++x)
	//					Output[y * (int)OutputDimensions.Width + x] = new v4(GetValue(x, y, noise) * 0.5 + 0.5);
	//		}
	//	}

	//	private double GetValue(int x, int y, double[,] noise)
	//	{
	//		var frequency = Frequency;
	//		var amplitude = Amplitude;

	//		var finalValue = 0.0;
	//		for (var i = 0; i < Octaves; ++i)
	//		{
	//			finalValue += GetSmoothNoise(x * frequency, y * frequency, noise) * amplitude;
	//			frequency *= 2.0f;
	//			amplitude *= Persistance;
	//		}

	//		if (finalValue < -1.0f)
	//			finalValue = -1.0f;
	//		else if (finalValue > 1.0f)
	//			finalValue = 1.0f;

	//		return finalValue;
	//	}

	//	private double GetSmoothNoise(double noise1, double noise2, double[,] noise)
	//	{
	//		var fractionX = noise1 - (int)noise1;
	//		var fractionY = noise2 - (int)noise2;

	//		var x1 = ((int)noise1 + (int)OutputDimensions.Width) % (int)OutputDimensions.Width;
	//		var y1 = ((int)noise2 + (int)OutputDimensions.Height) % (int)OutputDimensions.Height;
	//		var x2 = ((int)noise1 + (int)OutputDimensions.Width - 1) % (int)OutputDimensions.Width;
	//		var y2 = ((int)noise2 + (int)OutputDimensions.Height - 1) % (int)OutputDimensions.Height;

	//		x1 = System.Math.Max(0, x1);
	//		x2 = System.Math.Max(0, x2);

	//		y1 = System.Math.Max(0, y1);
	//		y2 = System.Math.Max(0, y2);

	//		var finalValue = 0.0;
	//		finalValue += fractionX * fractionY * noise[x1, y1];
	//		finalValue += fractionX * (1 - fractionY) * noise[x1, y2];
	//		finalValue += (1 - fractionX) * fractionY * noise[x2, y1];
	//		finalValue += (1 - fractionX) * (1 - fractionY) * noise[x2, y2];

	//		return finalValue;
	//	}
	//}
}
