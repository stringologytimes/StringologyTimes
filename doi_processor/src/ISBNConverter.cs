using System;
using System.Linq;

public static class ISBNConverter
{
    public static string Isbn10ToIsbn13(string isbn10)
    {
        if (isbn10 == null)
            throw new ArgumentNullException(nameof(isbn10));

        // ハイフンや空白を除去
        string s = new string(isbn10
            .Where(c => c != '-' && !char.IsWhiteSpace(c))
            .ToArray())
            .ToUpperInvariant();

        if (s.Length != 10)
        {
            Console.WriteLine("ISBN-10 must have 10 characters. " + isbn10);
            throw new ArgumentException("ISBN-10 must have 10 characters.", nameof(isbn10));
        }

        // ISBN-10 の形式チェック
            for (int i = 0; i < 9; i++)
            {
                if (!char.IsDigit(s[i]))
                    throw new ArgumentException("The first 9 characters of ISBN-10 must be digits.", nameof(isbn10));
            }

        if (!char.IsDigit(s[9]) && s[9] != 'X')
            throw new ArgumentException("The last character of ISBN-10 must be a digit or X.", nameof(isbn10));

        // ISBN-10 のチェックディジット検証
        if (!IsValidIsbn10(s))
            throw new ArgumentException("Invalid ISBN-10 check digit.", nameof(isbn10));

        // ISBN-10 の先頭9桁に 978 を付ける
        string first12Digits = "978" + s.Substring(0, 9);

        int sum = 0;
        for (int i = 0; i < 12; i++)
        {
            int digit = first12Digits[i] - '0';
            sum += (i % 2 == 0) ? digit : digit * 3;
        }

        int checkDigit = (10 - (sum % 10)) % 10;

        return first12Digits + checkDigit.ToString();
    }

    public static bool IsValidIsbn10(string isbn10)
    {
        if(isbn10.Length != 10)
        {
            return false;
        }
        
        int sum = 0;

        for (int i = 0; i < 10; i++)
        {
            int value;

            if (isbn10[i] == 'X')
            {
                if (i != 9)
                    return false;

                value = 10;
            }
            else
            {
                value = isbn10[i] - '0';
            }

            sum += value * (10 - i);
        }

        return sum % 11 == 0;
    }

    public static bool isISBNOwner(string type)
    {
        return type == "book" || type == "ConferenceProceeding" || type == "monograph" || type == "reference-book" || type == "proceedings" || type == "journal" || type == "edited-book";        
    }
}