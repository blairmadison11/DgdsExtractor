using System;
using System.IO;

namespace DgdsExtractor
{
	class DgdsVolumeIndex
	{
		/*
		 * NOTE: This class is the base container for all DGDS data
		 * The data structure hierarchy is as follows:
		 * VolumeIndex -> Volume -> Asset -> Chunk
		 */

		private readonly string directory, filename;
		private DgdsVolume[] volumes;

		public DgdsVolumeIndex(string directory, string filename)
		{
			this.directory = directory;
			this.filename = filename;
			ReadIndex();
		}

		// Read information about volumes from the volume index file
		private void ReadIndex()
		{
			using BinaryReader file = new BinaryReader(File.OpenRead(directory + filename));
			file.ReadBytes(4); // skip hash salt
			ushort numVolumes = file.ReadUInt16();
			volumes = new DgdsVolume[numVolumes];

			for (int i = 0; i < numVolumes; ++i)
			{
				string volName = DgdsUtilities.ReadFilename(file);
				int numFiles = Convert.ToInt32(file.ReadUInt16());
				volumes[i] = new DgdsVolume(directory, volName, numFiles);

				for (int j = 0; j < numFiles; ++j)
				{
					file.ReadInt32(); // skip file hash
					uint offset = file.ReadUInt32();
					volumes[i].InitializeAsset(j, offset);
				}
			}
		}

		// Read in the volume data
		public void ReadVolumes()
		{
			foreach (DgdsVolume volume in volumes)
			{
				volume.ReadAssets();
			}
		}

		// Write all extracted volume data to disk at the specified path
		public void WriteData(string path)
		{
			if (!Directory.Exists(path))
			{
				Directory.CreateDirectory(path);
			}

			foreach (DgdsVolume volume in volumes)
			{
				volume.WriteData(path);
			}
		}

		// Write all text lines (dialogue, descriptions, etc.) to disk
		public void WriteText(string path)
		{
			using StreamWriter writer = new StreamWriter(File.Create(path + "dialogue.txt"));
			foreach (DgdsVolume volume in volumes)
			{
				volume.WriteText(writer);
			}
		}

		// Print some information about volumes to console
		public void Print()
		{
			foreach (DgdsVolume volume in volumes)
			{
				volume.Print();
			}
		}
	}
}
