﻿using System.Security.Cryptography;
using Ujeby.UgUi.Core;
using Ujeby.UgUi.Operations.Types;

namespace Ujeby.UgUi.Operations.Crypto
{
	[NodeInfo]
	public class SHA512 : _String
	{
		public override void Execute()
		{
			var hash = SHA512Managed.Create().ComputeHash(System.Text.Encoding.UTF8.GetBytes(input));
			var newValue = System.Convert.ToBase64String(hash);

			SetField(ref this.value, newValue, nameof(Value));

			base.Execute();
		}
	}
}