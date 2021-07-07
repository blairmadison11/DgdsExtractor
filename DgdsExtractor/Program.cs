using System;
using System.IO;

namespace DgdsExtractor
{
	class Program
	{
		const string GAME_PATH = "D:\\dragon\\";
		const string VOLUME_INDEX_FILENAME = "VOLUME.VGA";
		const string EXTRACT_FOLDER = "extracted\\";

		static void Main(string[] args)
		{
			string gamePath = GAME_PATH;
			if (args.Length > 0)
			{
				if (Directory.Exists(args[0]))
				{
					gamePath = args[0];
				}
			}

			Console.WriteLine("DGDS Extractor 1.0\nBy Blair Durkee\n");
			DgdsVolumeIndex index = new DgdsVolumeIndex(gamePath, VOLUME_INDEX_FILENAME);

			Console.Write("Extracting data...");
			index.ReadVolumes();
			Console.WriteLine("DONE!");

			//index.PrintVolumes();

			Console.Write("Writing assets to disk...");
			index.WriteExtractedVolumes(gamePath + EXTRACT_FOLDER);
			Console.WriteLine("DONE!");

			Console.Write("Writing text to disk...");
			index.WriteText(GAME_PATH + EXTRACT_FOLDER);
			Console.WriteLine("DONE!");
		}	
	}
}
