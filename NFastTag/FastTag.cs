using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;

namespace NFastTag
{
	/// <summary>
	/// .NET port of the mark-watson FastTag_v2
	/// </summary>
	public class FastTag
	{
		/// <summary>
		/// Internal word lexicon where the word/pos tags are stored
		/// </summary>
		private readonly Dictionary<string, string[]> _lexicon = new Dictionary<string, string[]>();

		private readonly Regex specCharsAtStartAndEnd = new Regex(
				"^[^A-Za-z0-9]+|[^A-Za-z0-9]+$",
				RegexOptions.Compiled | RegexOptions.CultureInvariant
			);

		/// <param name="learningData"></param>
		public FastTag(string learningData)
		{
			using (var sr = new StringReader(learningData))
			{
				string line;
				while ((line = sr.ReadLine()) != null)
				{
					ParseLine(line);
				}
			}
		}

		/// <summary>
		/// Checks if the provided word exist in the imported lexicon
		/// </summary>
		/// <param name="word"></param>
		public bool WordInLexicon(string word)
		{
			return _lexicon.ContainsKey(word) || _lexicon.ContainsKey(word.ToLower());
		}

		/// <summary>
		/// Assigns parts of speech to each word
		/// </summary>
		public IList<FastTagResult> Tag(IList<string> words)
		{
			if (words == null || words.Count == 0) return new List<FastTagResult>();
			
			var result = new List<FastTagResult>();
			var pTags = GetPosTagsFor(words);

			// Apply transformational rules
			for (var i = 0; i < words.Count; i++)
			{
				var word = words[i];
				var pTag = pTags[i];

				//  rule 1: DT, {VBD | VBP} --> DT, NN
				if (i > 0 && pTags[i - 1] == "DT")
				{
					if (pTag == "VBD" || pTag == "VBP" || pTag == "VB")
					{
						pTag = "NN";
					}
				}

				// rule 2: convert a noun to a number (CD)
				if (pTag.StartsWith("N"))
				{
					if (float.TryParse(word, out var s))
					{
						pTag = "CD";
					}
				}

				// rule 3: convert a noun to a past participle if words.get(i) ends with "ed"
				if (pTag.StartsWith("N") && word.EndsWith("ed"))
				{
					pTag = "VBN";
				}

				// rule 4: convert any type to adverb if it ends in "ly";
				if (word.EndsWith("ly"))
				{
					pTag = "RB";
				}

				// rule 5: convert a common noun (NN or NNS) to a adjective if it ends with "al"
				if (pTag.StartsWith("NN") && word.EndsWith("al"))
				{
					pTag = "JJ";
				}

				// rule 6: convert a noun to a verb if the preceding word is "would"
				if (i > 0 && pTag.StartsWith("NN") && words[i - 1] == "would")
				{
					pTag = "VB";
				}

				// rule 7: if a word has been categorized as a common noun and it ends with "s",
				//         then set its type to plural common noun (NNS)
				if (pTag == "NN" && word.EndsWith("s"))
				{
					pTag = "NNS";
				}

				// rule 8: convert a common noun to a present participle verb (i.e., a gerund)
				if (pTag.StartsWith("NN") && word.EndsWith("ing"))
				{
					pTag = "VBG";
				}

				result.Add(new FastTagResult(word, pTag));
			}

			return result;
		}

		/// <summary>
		/// Assigns parts of speech to a sentence
		/// </summary>
		public IList<FastTagResult> Tag(string sentence)
		{
			if (string.IsNullOrEmpty(sentence)) return new List<FastTagResult>();
			var sentenceWords = sentence.Split(' ');
			return Tag(sentenceWords);
		}

		/// <summary>
		/// Retrieve the pos tags from the lexicon for the provided word list
		/// </summary>
		private IList<string> GetPosTagsFor(IList<string> words)
		{
			IList<string> ret = new List<string>(words.Count);

			for (int i = 0, size = words.Count; i < size; i++)
			{
				var word = RemoveSpecialCharacters(words[i]);

				if (string.IsNullOrEmpty(word))
				{
					ret.Add("");

					continue;
				}

				_lexicon.TryGetValue(word, out var ss);

				// 1/22/2002 mod (from Lisp code): if not in hash, try lower case:
				if (ss == null)
				{
					_lexicon.TryGetValue(word.ToLower(), out ss);
				}

				if (ss == null && word.Length == 1)
				{
					ret.Add(word + "^");
				}
				else if (ss == null)
				{
					ret.Add("NN");
				}
				else
				{
					ret.Add(ss[0]);
				}
			}

			return ret;
		}

		/// <summary>
		/// Clears special chars from start and end of the word
		/// </summary>
		private string RemoveSpecialCharacters(string str)
		{
			if (str.Length == 1) return str;

			var rpl = specCharsAtStartAndEnd.Replace(str, "");

			return rpl;
		}

		/// <summary>
		/// Parse a line into word and part of speech tags
		/// </summary>
		private void ParseLine(string line)
		{
			var ss = line.Split(' ');

			var word = ss[0];

			_lexicon[word] = ss.Skip(1).ToArray();
		}
	}
}