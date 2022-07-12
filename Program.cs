using System;
using System.Collections;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Encryptor
{
	class Program
	{
		static void Main(string[] args)
		{
			var stopwatch = new Stopwatch();
			if (args.Length != 0)
			{
				stopwatch.Start();
				if (args.Contains("/decrypt"))
				{
					Decrypt(args[0]);
				}
				else
				{
					Encrypt(args[0]);
				}
				stopwatch.Stop();
				if (!args.Contains("/silent"))
				{
					Console.WriteLine("Завершено. Затраченное время (секунды) - " + stopwatch.Elapsed.TotalSeconds);
					Console.ReadKey();
				}
				Process.GetCurrentProcess().Kill();
			}
			else
			{
				Console.WriteLine("Использование: encryptor.exe <путь к файлу> [/decrypt] [/silent].\n(Нажмите любую клавишу)");
				Console.ReadKey();
				Process.GetCurrentProcess().Kill();
			}
		}

		public static void Encrypt(string filePath)
		{
			char c;
			BitArray bitsSub = new BitArray(6);
			byte[] curDigit = new byte[1];

			FileStream f = File.OpenRead(filePath);
			byte[] bytes = new byte[(int)f.Length];
			f.Read(bytes, 0, (int)f.Length);
			BitArray bits = new BitArray(bytes);
			bytes = new byte[0];

			using (StreamWriter f1 = new StreamWriter(File.Create(filePath + "_encrypted.txt")))
			{
				int i = 0;
				int i1 = 0;
				foreach (bool bit in bits)
				{
					bitsSub.Set(i1, bit);
					//Console.Write(bit ? '1' : '0');
					//if ((i != 0) & ((i + 1) % 6 == 0))
					if (i1 == 5)
					{
						bitsSub.CopyTo(curDigit, 0);
						bitsSub.SetAll(false);
						i1 = -1;
						c = (char)(1040 + curDigit[0]);
						//Console.WriteLine(curDigit[0]);
						//Console.WriteLine();
						f1.Write(c);

						/*if (i + 1 + 6 > bits.Length)
						{
							s_end = s.Substring(i + 1, s.Length - (i + 1));
							c = (char)(1040 + Convert.ToInt32(s_end.PadRight(6, '0'), 2));
							f1.Write(c);
							f1.Write(s_end.Length.ToString());
						}*/
					}
					i++;
					i1++;
				}
				if (i1 < 5)
				{
					bitsSub.CopyTo(curDigit, 0);
					bitsSub.SetAll(false);
					c = (char)(1040 + curDigit[0]);
					//Console.WriteLine(curDigit[0]);
					f1.Write(c);
					f1.Write(i1.ToString());
				}
			}
			//Console.ReadKey();
		}

		public static void Decrypt(string filePath)
		{
			byte[] b = new byte[1];
			BitArray bits, bitsSub;
			bool[] bitsBool = new bool[8];
			int i = 0;
			bool isEven;
			int bLen;
			byte[] result;

			string fileText = File.ReadAllText(filePath);

			if (!char.IsDigit(fileText.Last()))
			{
				bits = new BitArray(fileText.Length * 6);
				isEven = true;
			}
			else
			{
				bits = new BitArray((fileText.Length - 2) * 6 + (int)char.GetNumericValue(fileText.Last()));
				isEven = false;
			}

			using (FileStream f = File.Create(filePath + "_decrypted"))
			{
				foreach (char c in fileText)
				{
					if (char.IsDigit(c))
					{
						break;
					}
					bLen = (!isEven & (i == fileText.Length - 2)) ? (int)char.GetNumericValue(fileText.Last()) : 6;
					b[0] = (byte)(c - 1040);
					bitsSub = new BitArray(b);
					bitsSub.CopyTo(bitsBool, 0);
					bitsSub = new BitArray(bitsBool.Take(bLen).ToArray());
					for (int i1 = i * 6; i1 < i * 6 + bLen; i1++)
					{
						bits.Set(i1, bitsSub.Get(i1 - i * 6));
					}
					
					/*foreach (bool bit in bits)
					{
						Console.Write(bit ? '1' : '0');
					}*/
					//f.Write((char)b[0]);
					//Console.WriteLine((int)b[0]);
					//Console.WriteLine();
					i++;
				}
				result = new byte[bits.Length / 8];
				bits.CopyTo(result, 0);
				f.Write(result, 0, result.Length);
			}
			
		}
	}
}