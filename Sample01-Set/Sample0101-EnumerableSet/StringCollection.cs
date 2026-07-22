using System.Collections;

namespace Sample0101
{
    /// <summary>
    /// 遍历器实现示例
    /// </summary>
    public class StringCollection:IEnumerable
    {
        public StringCollection(string s)
        {
            if (s != null && s.Length != 0)
                this.Chars = s.ToCharArray();
            else
                this.Chars = [];
           
        }
        private char[] Chars { get; set; }
        private const char EmptyChar = '\0';
        public char this[int index]
        {
            get
            {
                if (index < 0 || index > Chars.Length - 1)
                    return EmptyChar;
                return Chars[index];
            }
            set
            {
                if (index < 0 || index > Chars.Length - 1)
                    return;
                Chars[index] = value;
            }
        }

        public string this[int startIndex, int length]
        {
            get
            {
                if (startIndex < 0 || startIndex > Chars.Length - 1)
                    return string.Empty;
                if (length < 0 || length > Chars.Length - startIndex)
                    return string.Empty;
                return new string(Chars, startIndex, length);
            }
            set
            {
                if (startIndex < 0 || startIndex > Chars.Length - 1)
                    return;
                if (length < 0 || length > Chars.Length - startIndex)
                    return;
                if (value == null || value.Length != length)
                    return;
                int i = startIndex;
                foreach (var c in value)
                {
                    Chars[i] = c;
                    i++;
                }
            }
        }

        public IEnumerator GetEnumerator()
        {
            return new CharCollectionEnumerator(this);
        }

        public class CharCollectionEnumerator : IEnumerator
        {
            public CharCollectionEnumerator(StringCollection collection)
            {
                this.collection = collection;
                currentIndex = -1;
            }
            private StringCollection collection;

            private int currentIndex;
            public object Current
            {
                get
                {
                    return collection.Chars[currentIndex];
                }
            }

            public bool MoveNext()
            {
                if (++currentIndex < collection.Chars.Length)
                    return true;
                else
                    return false;
            }

            public void Reset()
            {
                currentIndex = -1;
            }
        }
    }
}
