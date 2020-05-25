using System;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Ujeby.Common.Tools.Types;

namespace Ujeby.UgUi.Common
{
	public class Convert
	{
		public static object ChangeType(object input, Type outputType)
		{
			if (input != null)
			{
				var inputType = input.GetType();
				if (inputType != outputType)
				{
					if (inputType == typeof(v4[]))
					{
						var v4Value = input as v4[];
						if (outputType == typeof(string))
							return v4Value.First().X.ToString("F4").Replace(',', '.');

						if (outputType == typeof(double))
							return v4Value.First().X;

						if (outputType == typeof(Size))
							return new Size(v4Value.First().X, v4Value.First().Y);

						if (outputType == typeof(int))
							return (int)v4Value.First().X;

						if (outputType == typeof(double[]))
							return v4Value.Select(i => i.X).ToArray();
					}
					else if (inputType == typeof(double[]))
						input = (input as double[]).FirstOrDefault();

					else if (inputType == typeof(Size))
					{
						var sizeValue = input as Size?;
						if (outputType == typeof(string))
							return sizeValue.Value.Width.ToString("F4").Replace(',', '.');

						if (outputType == typeof(double))
							return sizeValue.Value.Width;

						if (outputType == typeof(int))
							return (int)sizeValue.Value.Width;

						if (outputType == typeof(v4[]))
							return new v4[] { new v4 { X = sizeValue.Value.Width, Y = sizeValue.Value.Height } };

						if (outputType == typeof(double[]))
							return new double[] { sizeValue.Value.Width, sizeValue.Value.Height };
					}

					else if (inputType == typeof(double))
					{
						if (outputType == typeof(v2))
							return new v2((double)input);

						if (outputType == typeof(v3))
							return new v3((double)input);

						if (outputType == typeof(v4))
							return new v4((double)input);
					}

					else if (inputType == typeof(int))
					{
						if (outputType == typeof(v2))
							return new v2((double)input);

						if (outputType == typeof(v3))
							return new v3((double)input);

						if (outputType == typeof(v4))
							return new v4((double)(int)input);
					}

					else if (inputType == typeof(v2))
					{
						if (outputType == typeof(double))
							return (input as v2).X;

						if (outputType == typeof(v3))
							return new v3(input as v2);

						if (outputType == typeof(v4))
							return new v4(input as v2);
					}

					else if (inputType == typeof(v3))
					{
						if (outputType == typeof(double))
							return (input as v3).X;

						if (outputType == typeof(v2))
							return new v2(input as v2);

						if (outputType == typeof(v4))
							return new v4(input as v3);

						if (outputType == typeof(SolidColorBrush))
							return new SolidColorBrush(Color.FromArgb(
								0xff,
								ColorChannel((input as v3).X),
								ColorChannel((input as v3).Y),
								ColorChannel((input as v3).Z)));
					}

					else if (inputType == typeof(v4))
					{
						if (outputType == typeof(int))
							return (int)(input as v4).X;

						if (outputType == typeof(double))
							return (input as v4).X;

						if (outputType == typeof(v2))
							return new v2(input as v2);

						if (outputType == typeof(v3))
							return new v3(input as v3);

						if (outputType == typeof(SolidColorBrush))
							return new SolidColorBrush(Color.FromArgb(
								ColorChannel((input as v4).X),
								ColorChannel((input as v4).Y),
								ColorChannel((input as v4).Z),
								ColorChannel((input as v4).W)));
					}

					else if (inputType == typeof(SolidColorBrush))
					{
						if (outputType == typeof(string))
							return (input as SolidColorBrush).Color.ToString();

						if (outputType == typeof(v3))
							return new v3(
								ColorChannel((input as SolidColorBrush).Color.R),
								ColorChannel((input as SolidColorBrush).Color.G),
								ColorChannel((input as SolidColorBrush).Color.B));

						if (outputType == typeof(v4))
							return new v4(
								ColorChannel((input as SolidColorBrush).Color.A),
								ColorChannel((input as SolidColorBrush).Color.R),
								ColorChannel((input as SolidColorBrush).Color.G),
								ColorChannel((input as SolidColorBrush).Color.B));

						if (outputType == typeof(byte[]))
							return new byte[]
							{
								(input as SolidColorBrush).Color.A,
								(input as SolidColorBrush).Color.R,
								(input as SolidColorBrush).Color.G,
								(input as SolidColorBrush).Color.B,
							};
					}

					else if (inputType == typeof(string))
					{
						if (outputType == typeof(SolidColorBrush))
						{
							var inputValues = (input as string).Replace("#", string.Empty);
							if (inputValues.Length == 8)
							{
								return new SolidColorBrush(Color.FromArgb(
									byte.Parse(inputValues.Substring(0, 2), System.Globalization.NumberStyles.HexNumber),
									byte.Parse(inputValues.Substring(2, 2), System.Globalization.NumberStyles.HexNumber),
									byte.Parse(inputValues.Substring(4, 2), System.Globalization.NumberStyles.HexNumber),
									byte.Parse(inputValues.Substring(6, 2), System.Globalization.NumberStyles.HexNumber)));
							}
							else
								throw new FriendlyException($"Invalid color format: '{ input }' (correct format is #AARRGGBB in hexa)");
						}
					}

					else if (inputType == typeof(string[]))
					{
						if (outputType == typeof(object[]))
							return (input as string[]).ToArray<object>();
					}
				}
			}

			try
			{
				return System.Convert.ChangeType(input, outputType);
			}
			catch (InvalidCastException)
			{
				if (input != null)
					throw new FriendlyException($"MISSING CAST! { input.GetType().FullName } -> { outputType.FullName }");
				else
					throw;
			}
		}

		private static double ColorChannel(byte b)
		{
			return b == 0 ? 0.0 : (double)b / 255.0;
		}

		private static byte ColorChannel(double d)
		{
			return (byte)(255.0 * d);
		}
	}
}
