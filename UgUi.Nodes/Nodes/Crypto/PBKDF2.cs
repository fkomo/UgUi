using System;
using System.Security.Cryptography;
using System.Web.UI.WebControls;

namespace Ujeby.UgUi.Nodes.Crypto
{
	[NodeInfo]
	public class PBKDF2 : NodeBase
	{
		[Input(Order = 0, InputAnchor = true, AnchorOnly = true)]
		public string Login { get; set; }

		[Input(Order = 1, InputAnchor = true, AnchorOnly = true)]
		public string Password { get; set; }

		[Input(Order = 2, InputAnchor = true, AnchorOnly = true)]
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

		[Input(Order = 6, AnchorOnly = true, InputAnchor = true)]
		public string ToVerify { get; set; }

		[Output(AnchorOnly = true)]
		public string Hash { get; private set; }

		[Output(AnchorOnly = true)]
		public bool Verified { get; private set; }

		private static HashAlgorithmName GetHashAlgorithmName(string hashAlg)
		{
			switch (hashAlg?.ToLower())
			{
				case "sha1": return HashAlgorithmName.SHA1;
				case "sha256": return HashAlgorithmName.SHA256;
				case "sha384": return HashAlgorithmName.SHA384;
				case "sha512": return HashAlgorithmName.SHA512;
			}

			throw new NotImplementedException($"HashAlgorithmName={ hashAlg }");
		}

		/// <summary>
		/// Password-Based Key Derivation Function 2
		/// https://github.com/defuse/password-hashing
		/// </summary>
		public override void Execute()
		{
			Verified = false;

			if (Salt != null && !string.IsNullOrEmpty(HashAlgorithm))
			{
				var passwordHash = GetHash(
						Login,
						Password,
						Salt,
						HashAlgorithm,
						Iterations,
						PasswordHashLength);

				Hash = string.Join(DELIMITER.ToString(), new string[]
				{
					HashAlgorithm,
					Iterations.ToString(),
					PasswordHashLength.ToString(),
					Convert.ToBase64String(Salt),
					Convert.ToBase64String(passwordHash)
				});
			}
			else
				Hash = null;

			if (!string.IsNullOrEmpty(ToVerify) /*&& !string.IsNullOrEmpty(Login)*/ && !string.IsNullOrEmpty(Password))
			{
				var split = ToVerify.Split(DELIMITER);
				var hash = Convert.FromBase64String(split[PASSWORD_HASH_INDEX]);

				var storedHashSize = Int32.Parse(split[PASSWORD_HASH_BYTES_INDEX]);
				if (storedHashSize == hash.Length)
				{
					var computedHash = GetHash(
						Login,
						Password,
						Convert.FromBase64String(split[SALT_INDEX]),
						split[HASH_ALGORITHM_INDEX],
						Int32.Parse(split[PBKDF2_ITERATIONS_INDEX]),
						hash.Length);

					Verified = SlowEquals(hash, computedHash);
				}
			}
			else
				Verified = false;
		}

		private static byte[] GetHash(string login, string password, byte[] salt, string hashAlgorithm, int iterations, int hashLength)
		{
			// NOTE Login is not used

			var hash = null as byte[];
			using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, iterations, GetHashAlgorithmName(hashAlgorithm)))
				hash = pbkdf2.GetBytes(hashLength);

			return hash;
		}

		private static bool SlowEquals(byte[] a, byte[] b)
		{
			var diff = (uint)a.Length ^ (uint)b.Length;
			for (var i = 0; i < a.Length && i < b.Length; i++)
				diff |= (uint)(a[i] ^ b[i]);

			return diff == 0;
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
				$"{ nameof(ToVerify) }:{ ToVerify }",
			};
		}

		public override string[] GetOutputs()
		{
			return new string[] 
			{ 
				Hash,
				$"{ nameof(Verified) }:{ Verified }"
			};
		}

		public const int HASH_ALGORITHM_INDEX = 0;
		public const int PBKDF2_ITERATIONS_INDEX = 1;
		public const int PASSWORD_HASH_BYTES_INDEX = 2;
		public const int SALT_INDEX = 3;
		public const int PASSWORD_HASH_INDEX = 4;

		public const char DELIMITER = ':';
		public const int DELIMITER_COUNT = 4;
	}
}
