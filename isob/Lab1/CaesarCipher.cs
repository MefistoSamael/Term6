namespace Lab1
{
    public static class CaesarCipher
    {
        public static string Encrypt(string input, int key)
        {
            char[] chars = new char[input.Length];

            for (int i = 0; i < input.Length; i++)
            {
                chars[i] = EncryptChar(input[i], key);
            }

            return new string(chars);
        }

        public static string Decrypt(string input, int key)
        {
            return Encrypt(input, -key);
        }

        private static char EncryptChar(char letter, int key)
        {
            if (IsEnglishUpper(letter))
            {
                return ShiftLetter(letter, 'A', 26, key);
            }
            else if (IsEnglishLower(letter))
            {
                return ShiftLetter(letter, 'a', 26, key);
            }
            else if (IsRussianUpper(letter))
            {
                return ShiftLetter(letter, 'А', 32, key);
            }
            else if (IsRussianLower(letter))
            {
                return ShiftLetter(letter, 'а', 32, key);
            }
            else
            {
                return letter;
            }
        }

        private static char ShiftLetter(char letter, char baseChar, int alphabetSize, int key)
        {
            return (char)(Mod(letter - baseChar + key, alphabetSize) + baseChar);
        }

        public static bool IsEnglishUpper(char letter) => letter >= 'A' && letter <= 'Z';

        public static bool IsEnglishLower(char letter) => letter >= 'a' && letter <= 'z';

        public static bool IsRussianUpper(char letter) => letter >= 'А' && letter <= 'Я';

        public static bool IsRussianLower(char letter) => letter >= 'а' && letter <= 'я';

        private static int Mod(int x, int m) => ((x %= m) < 0) ? x + m : x;
    }
}
