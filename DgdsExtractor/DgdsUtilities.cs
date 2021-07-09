﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DgdsExtractor
{
	public static class DgdsUtilities
	{
		private static DgdsLzw lzw = new DgdsLzw();
		private const int FILENAME_SIZE = 13, ID_LENGTH = 4;

		// Reads a filename from the specified file
		// DGDS always allocates 13 characters for filenames
		// This method reads 13 characters but returns the variable-length string (indicated by null-termination)
		public static string ReadFilename(BinaryReader file)
		{
			byte[] chars = file.ReadBytes(FILENAME_SIZE);
			return Encoding.ASCII.GetString(chars, 0, Array.IndexOf(chars, (byte)0));
		}

		public static string ReadIdentifier(BinaryReader data)
		{
			byte[] id = data.ReadBytes(ID_LENGTH);

			if (id[^1] != ':')
			{
				throw new Exception("Invalid header!");
			}

			int end = id[2] == (byte)0 ? 2 : 1;

			return Encoding.ASCII.GetString(id[0..^end]);
		}

		// Decompress the given data according to the type of compression specified
		public static byte[] Decompress(byte compressionType, byte[] data)
		{
			byte[] output = data;
			switch (compressionType)
			{
				case 0x00:
					// do nothing
					break;
				case 0x01:
					output = RleDecompress(data);
					break;
				case 0x02:
					output = lzw.Decompress(data);
					break;
				default:
					Console.WriteLine("Unknown chunk compression: 0x{0:x}", compressionType);
					break;
			}
			return output;
		}

		// Perform Run Length Encoding decompression on the input data
		// Adapted from https://github.com/vcosta/scummvm/blob/master/engines/dgds/decompress.cpp
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
