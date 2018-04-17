namespace NFastTag
{
	/// <summary>
	/// Represent POS result for a word
	/// </summary>
	public class FastTagResult
	{
		/// <summary>
		/// The word used for tagging
		/// </summary>
		public string Word { get; private set; }

		/// <summary>
		/// The assigned tag
		/// </summary>
		public string PosTag { get; private set; }

		/// <param name="word"></param>
		/// <param name="pTag"></param>
		public FastTagResult(string word, string pTag)
		{
			Word = word;
			PosTag = pTag;
		}
	}
}