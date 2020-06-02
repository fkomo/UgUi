﻿using System.Security.Cryptography;
using Ujeby.UgUi.Nodes.Abstract;

namespace Ujeby.UgUi.Nodes.Crypto
{
	[NodeInfo]
	public class SHA1 : HashNode
	{
		public override void Execute()
		{
			Output = SHA1Managed.Create().ComputeHash(Input);

			base.Execute();
		}
	}
}
