namespace Lab1
{
    public static class VigenereCipher
    {
        public static string Encrypt(string input, string key)
        {
            char[] chars = new char[input.Length];

            for (int i = 0; i < chars.Length; i++)
            {
                chars[i] = EncryptChar(input[i], GetStringBias(key, i));
            }

            return new string(chars);
        }

        public static string Decrypt(string input, string key)
        {
            char[] chars = new char[input.Length];

            for (int i = 0; i < chars.Length; i++)
            {
                chars[i] = DecryptChar(input[i], GetStringBias(key, i));
            }

            return new string(chars);
        }

        private static char EncryptChar(char letter, int key)
        {
            return CaesarCipher.Encrypt(letter.ToString(), key)[0];
        }

        private static char DecryptChar(char letter, int key)
        {
            return CaesarCipher.Decrypt(letter.ToString(), key)[0];
        }

        private static int GetStringBias(string str, int index)
        {
            var c = str[index % str.Length];

            return GetAlphabetBias(c);
        }

        private static int GetAlphabetBias(char letter)
        {
            if (CaesarCipher.IsEnglishUpper(letter))
                return letter - 'A';
            else if (CaesarCipher.IsEnglishLower(letter))
                return letter - 'a';
            else if (CaesarCipher.IsRussianUpper(letter))
                return letter - 'А';
            else if (CaesarCipher.IsRussianLower(letter))
                return letter - 'а';
            else
                return -1;
        }
    }
}
