using System;
using System.Security.Cryptography;

namespace Ujeby.UgUi.Nodes.Crypto
{
	[NodeInfo]
	public class PBKDF2 : NodeBase
	{
		[Input(Order = 0, InputAnchor = true, AnchorOnly = true)]
		public string Login { get; set; }

		[Input(Order = 2, InputAnchor = true, AnchorOnly = true)]
		public string Password { get; set; }

		[Input(Order = 4, InputAnchor = true, AnchorOnly = true)]
		public byte[] Salt { get; set; }

		private int iterations = 32000;
		[Input(Order = 3, InputAnchor = true)]
		public int Iterations 
		{
			get { return iterations; } 
			set { SetField(ref iterations, value, nameof(Iterations)); }
		}

		private int passwordHashLength = 24;
		[Input(Order = 4, InputAnchor = true)]
		public int PasswordHashLength
		{
			get { return passwordHashLength; }
			set { SetField(ref passwordHashLength, value, nameof(PasswordHashLength)); }
		}

		private string hashAlgorithm = "sha1";
		[Input(Order = 5)]
		public string HashAlgorithm
		{
			get { return hashAlgorithm; }
			set { SetField(ref hashAlgorithm, value, nameof(HashAlgorithm)); }
		}

		[Output(AnchorOnly = true)]
		public string Hash { get; private set; }

		private HashAlgorithmName CurrentHashAlgorithmName 
		{ 
			get
			{
				switch (this.HashAlgorithm)
				{
					case "sha1": return HashAlgorithmName.SHA1;
					case "sha256": return HashAlgorithmName.SHA256;
					case "sha384": return HashAlgorithmName.SHA384;
					case "sha512": return HashAlgorithmName.SHA512;
				}

				throw new NotImplementedException(HashAlgorithm);
			}
		}

		/// <summary>
		/// Password-Based Key Derivation Function 2
		/// https://github.com/defuse/password-hashing
		/// </summary>
		public override void Execute()
		{
			var passwordHash = null as byte[];


			using (var pbkdf2 = new Rfc2898DeriveBytes(Password, Salt, Iterations, CurrentHashAlgorithmName))
				passwordHash = pbkdf2.GetBytes(PasswordHashLength);

			Hash = string.Join(DELIMITER.ToString(), new string[]
			{
				HashAlgorithm,
				Iterations.ToString(),
				PasswordHashLength.ToString(),
				Convert.ToBase64String(Salt),
				Convert.ToBase64String(passwordHash)
			});
		}

		public override string[] GetInputs()
		{
			return new string[] 
			{
				$"{ nameof(Login) }:{ Login }",
				$"{ nameof(Password) }:{ Password }",
				$"{ nameof(Iterations) }:{ Iterations.ToString() }",
				$"{ nameof(PasswordHashLength) }:{ PasswordHashLength.ToString() }",
				// Salt,
				$"{ nameof(Salt) }.Length:{ (Salt?.Length ?? 0).ToString() }",
			};
		}

		public override string[] GetOutputs()
		{
			return new string[] { Hash };
		}

		public const int HASH_ALGORITHM_INDEX = 0;
		public const int PBKDF2_ITERATIONS_INDEX = 1;
		public const int PASSWORD_HASH_BYTES_INDEX = 2;
		public const int SALT_INDEX = 3;
		public const int PASSWORD_HASH_INDEX = 4;

		public const char DELIMITER = ':';
		public const int DELIMITER_COUNT = 4;

		//private static bool SlowEquals(byte[] a, byte[] b)
		//{
		//	var diff = (uint)a.Length ^ (uint)b.Length;
		//	for (var i = 0; i < a.Length && i < b.Length; i++)
		//		diff |= (uint)(a[i] ^ b[i]);

		//	return diff == 0;
		//}
	}
}
