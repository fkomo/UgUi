using Newtonsoft.Json;
using System;
using System.Security.Cryptography;
using System.Text;
using Ujeby.UgUi.Core;

namespace Ujeby.UgUi.Operations.Generators
{
	[NodeInfo]
	public class RandomString : NodeOperationBase, ISerializableNode
	{
		protected int length = 0;
		[Input(Order = 0, InputAnchor = true, OutputAnchor = true, Serializable = true)]
		public int Length
		{
			get { return length; }
			set { SetField(ref length, value, nameof(Length)); }
		}

		protected string _value = string.Empty;
		[Output(AnchorOnly = false, ReadOnly = true, DisplayName = "")]
		public string _Value
		{
			get { return _value; }
			set { SetField(ref this._value, value, nameof(_Value)); }
		}

		const string chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

		public override void Execute()
		{
			// https://stackoverflow.com/questions/1344221/how-can-i-generate-random-alphanumeric-strings

			//_Value = new string(Enumerable.Repeat(chars, length).Select(s => s[random.Next(s.Length)]).ToArray());

			var data = new byte[4 * length];
			using (var crypto = new RNGCryptoServiceProvider())
				crypto.GetBytes(data);

			var result = new StringBuilder(length);
			for (int i = 0; i < length; i++)
			{
				var rnd = BitConverter.ToUInt32(data, i * 4);
				var idx = rnd % chars.Length;

				result.Append(chars[(int)idx]);
			}

			_Value = result.ToString();
		}

		public override string[] GetInputs()
		{
			return new string[] { Length.ToString() };
		}

		public override string[] GetOutputs()
		{
			return new string[] { _Value };
		}
	}
}
