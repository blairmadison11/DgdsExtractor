using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace DgdsExtractor
{
	class DgdsLzw
	{
		/**
		 * 
		 * Adapted from https://github.com/vcosta/scummvm/tree/master/engines/dgds
		 * Used in accordance with GNU Public License version 2
		 *
		 */

		class TableEntry
		{
			public byte[] str = new byte[256];
			public byte len;
		}

		TableEntry[] codeTable;

		byte[] codeCur = new byte[256];

		uint bitsData, bitsSize;

		uint codeSize, codeLen, cacheBits;

		uint tableSize, tableMax;
		bool tableFull;

		private void Reset()
		{
			codeTable = new TableEntry[0x4000];

			for (uint code = 0; code < 0x4000; code++)
			{
				codeTable[code] = new TableEntry();
				if (code < 256)
				{
					codeTable[code].len = 1;
					codeTable[code].str[0] = Convert.ToByte(code);
				}
				else
				{
					codeTable[code].len = 0;
				}
			}

			tableSize = 0x101;
			tableMax = 0x200;
			tableFull = false;

			codeSize = 9;
			codeLen = 0;

			cacheBits = 0;
		}

		public byte[] Decompress(byte[] data)
		{
			BinaryReader input = new BinaryReader(new MemoryStream(data));
			BinaryWriter output = new BinaryWriter(new MemoryStream());

			bitsData = 0;
			bitsSize = 0;

			Reset();

			cacheBits = 0;

			do
			{
				uint code;

				code = GetCode(input, codeSize);
				if (code == 0xFFFFFFFF) break;

				cacheBits += codeSize;
				if (cacheBits >= codeSize * 8)
				{
					cacheBits -= codeSize * 8;
				}

				if (code == 0x100)
				{
					if (cacheBits > 0)
					{
						GetCode(input, codeSize * 8 - cacheBits);
					}
					Reset();
				}
				else
				{
					if (code >= tableSize && !tableFull)
					{
						codeCur[codeLen++] = codeCur[0];

						for (uint i = 0; i < codeLen; i++)
						{
							output.Write(codeCur[i]);
						}
					}
					else
					{
						for (uint i = 0; i < codeTable[code].len; i++)
						{
							output.Write(codeTable[code].str[i]);
						}
						codeCur[codeLen++] = codeTable[code].str[0];
					}

					if (codeLen >= 2)
					{
						if (!tableFull)
						{
							uint i;

							if (tableSize == tableMax && codeSize == 12)
							{
								tableFull = true;
								i = tableSize;
							}
							else
							{
								i = tableSize++;
								cacheBits = 0;
							}

							if (tableSize == tableMax && codeSize < 12)
							{
								codeSize++;
								tableMax <<= 1;
							}

							for (uint j = 0; j < codeLen; j++)
							{
								codeTable[i].str[j] = codeCur[j];
								codeTable[i].len++;
							}
						}

						for (uint i = 0; i < codeTable[code].len; i++)
							codeCur[i] = codeTable[code].str[i];

						codeLen = codeTable[code].len;
					}
				}
			} while (input.BaseStream.Position <= input.BaseStream.Length);

			return ((MemoryStream)output.BaseStream).ToArray();
		}

		private uint GetCode(BinaryReader input, uint totalBits)
		{
			uint result, numBits;
			byte[] bitMasks = new byte[] { 0x00, 0x01, 0x03, 0x07, 0x0F, 0x1F, 0x3F, 0x7F, 0xFF };

			numBits = totalBits;
			result = 0;
			while (numBits > 0)
			{
				uint useBits;

				if (input.BaseStream.Position >= input.BaseStream.Length) return 0xFFFFFFFF;

				if (bitsSize == 0)
				{
					bitsSize = 8;
					bitsData = input.ReadByte();
				}

				useBits = numBits;
				if (useBits > 8) useBits = 8;
				if (useBits > bitsSize) useBits = bitsSize;

				result |= (bitsData & bitMasks[useBits]) << Convert.ToInt32(totalBits - numBits);

				numBits -= useBits;
				bitsSize -= useBits;
				bitsData >>= Convert.ToInt32(useBits);
			}
			return result;
		}
	}
}
