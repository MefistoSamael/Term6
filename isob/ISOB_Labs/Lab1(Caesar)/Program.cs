namespace Lab1;


public class Program
{
    private static void Main(string[] args)
    {
        string path = @"datafile.txt";

        string fileData = ReadFile(path);

        TestCesar(fileData);

        Console.WriteLine();

        TestVigenère(fileData);
    }

    private static void TestCesar(string str)
    {
        Console.WriteLine("Cesar\n");

        Console.WriteLine($"Original: {str}");
        int key = 10000;
        Console.WriteLine($"Key: {key}");

        var encrypted = CaesarCipher.Encrypt(str, key);

        Console.WriteLine($"Encrypted: {encrypted}");

        var decrypted = CaesarCipher.Decrypt(encrypted, key);

        Console.WriteLine($"Decrypted: {decrypted}");
    }

    private static void TestVigenère(string str)
    {
        Console.WriteLine("Vigenère\n");

        Console.WriteLine($"Original: {str}");

        string key = "emon";
        Console.WriteLine($"Key: {key}");


        var encrypted = VigenereCipher.Encrypt(str, key);

        Console.WriteLine($"Encrypted: {encrypted}");

        var decrypted = VigenereCipher.Decrypt(encrypted, key);

        Console.WriteLine($"Decrypted: {decrypted}");
    }


    private static string ReadFile(string path)
    {
        return File.ReadAllText(path);
    }
}
