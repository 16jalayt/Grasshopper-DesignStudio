using System;
using System.Collections;

namespace Cricut_Design_Studio
{
	public class TagDictionary
	{
		public class TagRef
		{
			public int fontNo;

			public int featureId;

			public int glyphId;

			public TagRef(int fontNo, int featureId, int glyphId)
			{
				this.fontNo = fontNo;
				this.featureId = featureId;
				this.glyphId = glyphId;
			}
		}

		public class WordNode
		{
			public char ch;

			public int id = -1;

			public ArrayList nextList = new ArrayList();

			public static int wordCounter;

			public static int maxLen;

			private int getWordCounter()
			{
				return wordCounter;
			}

			public int addWord(string s, int wordId, int index)
			{
				if (index >= s.Length)
				{
					if (index > maxLen)
					{
						maxLen = index;
					}
					if (-1 == id)
					{
						if (wordId != -1)
						{
							id = wordId;
						}
						else
						{
							id = wordCounter++;
						}
					}
					return id;
				}
				int num = -1;
				foreach (WordNode next in nextList)
				{
					char c = s[index];
					if (next.ch == c)
					{
						num = next.addWord(s, wordId, index + 1);
						break;
					}
				}
				if (-1 == num)
				{
					WordNode wordNode2 = new WordNode();
					wordNode2.ch = s[index];
					nextList.Add(wordNode2);
					num = wordNode2.addWord(s, wordId, index + 1);
				}
				return num;
			}

			public int lookup(string s, int index)
			{
				if (index >= s.Length)
				{
					return id;
				}
				WordNode wordNode = null;
				foreach (WordNode next in nextList)
				{
					char c = s[index];
					if (next.ch == c)
					{
						wordNode = next;
						break;
					}
				}
				return wordNode?.lookup(s, index + 1) ?? (-1);
			}
		}

		public class WordTree
		{
			private ArrayList firstLetters = new ArrayList();

			private int getFirstLetterIndex(char fl)
			{
				int num = -1;
				for (int i = 0; i < firstLetters.Count; i++)
				{
					WordNode wordNode = (WordNode)firstLetters[i];
					char ch = wordNode.ch;
					if (fl == ch)
					{
						num = i;
						break;
					}
					if (fl < ch)
					{
						WordNode wordNode2 = new WordNode();
						wordNode2.ch = fl;
						num = i;
						firstLetters.Insert(i, wordNode2);
						break;
					}
				}
				if (-1 == num)
				{
					num = firstLetters.Count;
					WordNode wordNode3 = new WordNode();
					wordNode3.ch = fl;
					num = firstLetters.Count;
					firstLetters.Add(wordNode3);
				}
				return num;
			}

			public int addWord(string s, int wordId)
			{
				char fl = '\0';
				if (s == null || s.Length == 0)
				{
					return -1;
				}
				int firstLetterIndex = getFirstLetterIndex(fl);
				return ((WordNode)firstLetters[firstLetterIndex]).addWord(s, wordId, 0);
			}

			public int lookupWord(string s)
			{
				char c = '\0';
				if (s == null || s.Length == 0)
				{
					return -1;
				}
				foreach (WordNode firstLetter in firstLetters)
				{
					if (firstLetter.ch == c)
					{
						return firstLetter.lookup(s, 0);
					}
				}
				return -1;
			}
		}

		private const string whitespace = "_-(){}[], \t";

		private const string trimChars = " !\"#$%&'()*+-./:;<=>?@[\\]^_`{|}~";

		private static string[] fontTagDictionary = new string[0];

		private WordNode blankNode = new WordNode();

		private WordTree wordTree = new WordTree();

		private ArrayList arrayOfTagRefArrays = new ArrayList();

		private string[] stringArray;

		private ArrayList stringList = new ArrayList();

		public static void swap(int[] list, int i, int j)
		{
			int num = list[i];
			list[i] = list[j];
			list[j] = num;
		}

		public static void sort(int[] list, int left, int right)
		{
			if (left >= right)
			{
				return;
			}
			swap(list, left, (left + right) / 2);
			int num = left;
			for (int i = left + 1; i <= right; i++)
			{
				if (list[i] < list[left])
				{
					swap(list, ++num, i);
				}
			}
			swap(list, left, num);
			sort(list, left, num - 1);
			sort(list, num + 1, right);
		}

		public static void swap(ArrayList list, int i, int j)
		{
			object value = list[i];
			list[i] = list[j];
			list[j] = value;
		}

		public static void sort(ArrayList list, int left, int right)
		{
			if (left >= right)
			{
				return;
			}
			swap(list, left, (left + right) / 2);
			int num = left;
			for (int i = left + 1; i <= right; i++)
			{
				if (((string)list[i]).CompareTo((string)list[left]) < 0)
				{
					swap(list, ++num, i);
				}
			}
			swap(list, left, num);
			sort(list, left, num - 1);
			sort(list, num + 1, right);
		}

		public static int toTagRef(int fontNo, int featureId, int glyphId)
		{
			return (fontNo << 10) | (featureId << 6) | glyphId;
		}

		public static void fromTagRef(int tr, ref int fontNo, ref int featureId, ref int glyphId)
		{
			fontNo = tr >> 10;
			featureId = (tr >> 6) & 0xF;
			glyphId = tr & 0x3F;
		}

		public void load(ArrayList shapes, FontMetadata fontMetadata)
		{
			int num = 0;
			new ArrayList();
			foreach (Shape shape in shapes)
			{
				string fontName = shape.getFontName();
				SplashScreen.SetStatus("Loading keywords\n\"" + FontLoading.GetFontDisplayName(fontName) + "\"...", setReference: true);
				for (int i = 0; i < 14; i++)
				{
					for (int j = 0; j < 50; j++)
					{
						toTagRef(num, i, j);
						string[] tags = fontMetadata.getTags(fontName, i, j);
						if (tags != null && tags.Length > 0)
						{
							addTagStrings(tags, num, i, j);
						}
					}
				}
				num++;
			}
		}

		public void addTagStrings(string[] tags, int fontNo, int featureNo, int glyphNo)
		{
			if (stringList == null)
			{
				stringList = new ArrayList();
			}
			int num = toTagRef(fontNo, featureNo, glyphNo);
			foreach (string text in tags)
			{
				ArrayList arrayList = null;
				int num2 = wordTree.addWord(text, -1);
				if (num2 > arrayOfTagRefArrays.Count - 1)
				{
					if (num2 != arrayOfTagRefArrays.Count)
					{
						throw new Exception("Bad dictionary add " + num2 + " > " + arrayOfTagRefArrays.Count);
					}
					arrayList = new ArrayList();
					arrayOfTagRefArrays.Add(arrayList);
					stringList.Add(text);
				}
				else
				{
					arrayList = (ArrayList)arrayOfTagRefArrays[num2];
				}
				arrayList?.Add(num);
			}
		}

		public void removeTagString(string tag, int fontNo, int featureNo, int glyphNo)
		{
			int num = toTagRef(fontNo, featureNo, glyphNo);
			int num2 = wordTree.lookupWord(tag);
			if (num2 >= arrayOfTagRefArrays.Count || arrayOfTagRefArrays[num2] == null)
			{
				return;
			}
			ArrayList arrayList = (ArrayList)arrayOfTagRefArrays[num2];
			for (int i = 0; i < arrayList.Count; i++)
			{
				if (num == (int)arrayList[i])
				{
					arrayList.RemoveAt(i);
					break;
				}
			}
		}

		public string[] getKeywordsStringList()
		{
			if (stringArray == null || stringList != null)
			{
				string[] array = fontTagDictionary;
				foreach (string text in array)
				{
					int num = wordTree.lookupWord(text);
					if (-1 == num)
					{
						stringList.Add(text);
					}
				}
				sort(stringList, 0, stringList.Count - 1);
				stringArray = new string[stringList.Count];
				int num2 = 0;
				foreach (string @string in stringList)
				{
					stringArray[num2++] = @string;
				}
				stringList = null;
			}
			return stringArray;
		}

		public ArrayList matchTags(string tag1, string tag2)
		{
			ArrayList arrayList = null;
			ArrayList arrayList2 = null;
			int num = -1;
			int num2 = -1;
			if (tag1 != null && tag1.Length > 0)
			{
				num = wordTree.lookupWord(tag1);
			}
			if (tag2 != null && tag2.Length > 0)
			{
				num2 = wordTree.lookupWord(tag2);
			}
			int num3 = 0;
			int num4 = 0;
			if (num >= arrayOfTagRefArrays.Count)
			{
				num4++;
			}
			else if (num >= 0)
			{
				arrayList = (ArrayList)arrayOfTagRefArrays[num];
				num3 += arrayList.Count;
				num4++;
			}
			if (num2 >= arrayOfTagRefArrays.Count)
			{
				num4++;
			}
			else if (num2 >= 0)
			{
				arrayList2 = (ArrayList)arrayOfTagRefArrays[num2];
				num3 += arrayList2.Count;
				num4++;
			}
			if (num4 == 0)
			{
				return null;
			}
			int[] array = new int[num3];
			num3 = 0;
			if (arrayList != null)
			{
				foreach (int item in arrayList)
				{
					array[num3++] = item;
				}
			}
			if (arrayList2 != null)
			{
				foreach (int item2 in arrayList2)
				{
					array[num3++] = item2;
				}
			}
			sort(array, 0, array.Length - 1);
			ArrayList arrayList3 = new ArrayList();
			if (2 == num4)
			{
				for (int i = 1; i < array.Length; i++)
				{
					if (array[i] == array[i - 1])
					{
						arrayList3.Add(array[i]);
					}
				}
			}
			else
			{
				int[] array2 = array;
				foreach (int num7 in array2)
				{
					arrayList3.Add(num7);
				}
			}
			return arrayList3;
		}

		public void Dispose()
		{
			Dispose(disposing: true);
			GC.SuppressFinalize(this);
		}

		protected virtual void Dispose(bool disposing)
		{
		}

		~TagDictionary()
		{
			Dispose(disposing: false);
		}
	}
}
