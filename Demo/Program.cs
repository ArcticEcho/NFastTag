﻿using System;
using System.IO;
using NFastTag;

class Program
{
	static void Main(string[] args)
	{
		Console.WriteLine("Welcome to FastTag_v2 .NET PORT");
		Console.WriteLine("Enter an English sentence and watch it being tagged!");

		// read the english lexicon data
		var lexicon = File.ReadAllText(@"..\NFastTag\lexicon.txt");

		// run the sample loop
		var ft = new FastTag(lexicon);

		string sentence;
		while ((sentence = Console.ReadLine()) != "[x]")
		{
			var tagResult = ft.Tag(sentence);

			foreach (var ftr in tagResult)
			{
				var message = string.Format("[{0} {1}]", ftr.Word, ftr.PosTag);

				Console.WriteLine(message);
			}
		}

		Console.WriteLine("Bye Bye!");
	}
}
