using System;

namespace DgdsExtractor
{
	class Program
	{
		const string GAME_PATH = "D:\\dragon\\";
		const string VOLUME_INDEX_FILENAME = "VOLUME.VGA";
		const string EXTRACT_FOLDER = "extracted\\";

		static void Main(string[] args)
		{
			Console.WriteLine("DGDS Extractor 0.2\nBy Blair Durkee");
			DgdsVolumeIndex index = new DgdsVolumeIndex(GAME_PATH, VOLUME_INDEX_FILENAME);
			index.ReadVolumes();
			index.PrintVolumes();
			index.WriteExtractedVolumes(GAME_PATH + EXTRACT_FOLDER);
		}	
	}
}
