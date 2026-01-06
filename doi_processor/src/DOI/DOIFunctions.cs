namespace DataProcessor
{
    class DOIFunctions
    {
                public static string GetPrefix(string doi)
        {
            var parts = doi.Split("/");
            if (parts.Length >= 1)
            {
                return parts[0];
            }
            else
            {
                throw new Exception("Invalid DOI: " + doi);
            }

        }

    }
}
