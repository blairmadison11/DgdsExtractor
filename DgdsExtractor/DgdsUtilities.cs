using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace DgdsExtractor
{
	public static class DgdsUtilities
	{
		public static string ReadString(BinaryReader file)
		{
			bool end = false;
			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < 13; ++i)
			{
				byte c = file.ReadByte();

				end = end || (c == 0);

				if (!end)
				{
					sb.Append((char)c);
				}
			}
			return sb.ToString();
		}

		public static byte[] Decompress(byte compressionType, byte[] data)
		{
			byte[] output = data;
			switch (compressionType)
			{
				case 0x00:
					{
						// do nothing
						break;
					}
				case 0x01:
					{
						output = RleDecompress(data);
						break;
					}
				case 0x02:
					{
						LzwDecompressor lzw = new LzwDecompressor();
						output = lzw.Decompress(data);
						break;
					}
				default:
					Console.WriteLine("Unknown chunk compression: 0x{0:x}", compressionType);
					break;
			}
			return output;
		}

		private static byte[] RleDecompress(byte[] input)
		{
			byte marker, symbol;
			uint i, inputPosition, count;
			int inputSize = input.Length;
			List<byte> output = new List<byte>();

			inputPosition = 0;
			marker = input[inputPosition++];

			do
			{
				symbol = input[inputPosition++];

				if (symbol == marker)
				{
					count = input[inputPosition++];

					if (count <= 2)
					{
						for (i = 0; i <= count; ++i)
						{
							output.Add(marker);
						}
					}
					else
					{
						if (Convert.ToBoolean(count & 0x80))
						{
							count = ((count & 0x7f) << 8) + input[inputPosition++];
						}

						symbol = input[inputPosition++];

						for (i = 0; i <= count; ++i)
						{
							output.Add(symbol);
						}
					}
				}
				else
				{
					output.Add(symbol);
				}
			} while (inputPosition < inputSize);

			return output.ToArray();
		}
	}
}
