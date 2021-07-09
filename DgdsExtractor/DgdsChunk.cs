using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace DgdsExtractor
{
	class DgdsChunk
	{
		private AssetType type;
		private AssetSection section;
		private byte[] data;
		private bool isContainer;
		private List<string> textLines;

		public bool IsContainer { get => isContainer; }
		public AssetType ChunkType { get => type; }

		public DgdsChunk(AssetType type)
		{
			this.type = type;
			section = AssetSection.NONE;
		}

		// Parses the chunk data from the specified asset data
		public void ReadChunk(BinaryReader assetData)
		{
			string idStr = DgdsUtilities.ReadIdentifier(assetData);
			uint sizeData = assetData.ReadUInt32();
			isContainer = Convert.ToBoolean(sizeData >> 31);
			int size = Convert.ToInt32(sizeData & 0x7FFFFFFF);
			
			if (!isContainer)
			{
				section = DgdsMetadata.GetAssetSection(idStr);
				if (DgdsMetadata.IsCompressed(ChunkType, section))
				{
					byte compressionType = assetData.ReadByte();
					assetData.ReadUInt32(); // skip unpack size
					byte[] compressedData = assetData.ReadBytes(size - 5);
					data = DgdsUtilities.Decompress(compressionType, compressedData);
				}
				else
				{
					data = assetData.ReadBytes(size);
				}

				if (section == AssetSection.SDS)
				{
					ExtractText();
				}
			}
			else
			{
				type = DgdsMetadata.GetAssetType(idStr);
			}
		}

		private void ExtractText()
		{
			textLines = new List<string>();
			int index = 13;
			while (index + 6 < data.Length)
			{
				if (data[index++] == 0x04)
				{
					ushort op0 = BitConverter.ToUInt16(data, index - 1);
					ushort op1 = BitConverter.ToUInt16(data, index + 1);
					ushort op2 = BitConverter.ToUInt16(data, index + 3);

					if (op0 == 0x04 && (op1 == 0x02 || op1 == 0x03) && (op2 == 0x00 || op2 == 0x80))
					{
						index += 11;
						ushort count = BitConverter.ToUInt16(data, index);
						index += 2;

						string line = Encoding.ASCII.GetString(data, index, Array.IndexOf(data, (byte)0, index, count) - index).Replace('\r', '\n');
						
						if (line.EndsWith("\n?"))
						{
							line = line[0..(line.Length - 2)].Trim();
						}

						if (line.Length > 1)
						{
							textLines.Add(line);
						}
						
						index += count;
					}
				}
			}
		}

		// Write the chunk data to disk
		public void WriteData(BinaryWriter outFile)
		{
			if (data != null)
			{
				outFile.Write(data);
			}
		}

		// Write all text lines (dialogue, descriptions, etc.) contained in this chunk to disk
		public void WriteText(StreamWriter outFile)
		{
			if (textLines != null)
			{
				foreach (string line in textLines)
				{
					outFile.Write("{0}\n\n", line);
				}
			}
		}

		// Write log information to output stream
		public void WriteLog(StreamWriter output)
		{
			string str = string.Format("\tChunk: {0}", section == AssetSection.NONE ? type.ToString() : section.ToString());
			if (data != null)
			{
				str += string.Format(" ({0} bytes)", data.Length);
			}
			output.WriteLine(str);
		}
	}
}
