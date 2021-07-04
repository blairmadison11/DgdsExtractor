using System;
using System.Collections.Generic;
using System.Text;
using System.IO;

namespace DgdsExtractor
{
	class DgdsVolumeIndex
	{
		private string directory, filename;
		private DgdsVolume[] volumes;

		public DgdsVolumeIndex(string directory, string filename)
		{
			this.directory = directory;
			this.filename = filename;
			ReadIndex();
		}

		private void ReadIndex()
		{
			using (BinaryReader file = new BinaryReader(File.OpenRead(directory + filename)))
			{
				byte[] salt = file.ReadBytes(4);
				int numVolumes = file.ReadUInt16();
				volumes = new DgdsVolume[numVolumes];

				for (int i = 0; i < numVolumes; ++i)
				{
					string volName = DgdsUtilities.ReadString(file);
					uint numFiles = file.ReadUInt16();
					volumes[i] = new DgdsVolume(directory, volName, Convert.ToInt32(numFiles));

					for (int j = 0; j < numFiles; ++j)
					{
						int hash = file.ReadInt32();
						uint offset = file.ReadUInt32();
						volumes[i].InitializeAsset(j, hash, offset);
					}
				}
			}
		}

		public void ReadVolumes()
		{
			for (int i = 0; i < volumes.Length; ++i)
			{
				volumes[i].ReadAssets();
			}
		}

		public void WriteExtractedAssets(string path)
		{
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}

			for (int i = 0; i < volumes.Length; ++i)
			{
				volumes[i].WriteAssets(path);
			}
		}

		public void PrintVolumes()
		{
			for (int i = 0; i < volumes.Length; ++i)
			{
				Console.WriteLine("{0} contains {1} assets", volumes[i].Filename, volumes[i].NumAssets);
				volumes[i].PrintAssets();
			}
		}
	}
}
