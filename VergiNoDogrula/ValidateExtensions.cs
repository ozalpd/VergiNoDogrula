using VergiNoDogrula.Models;

namespace VergiNoDogrula
{
    public static class ValidateExtensions
    {
        /// <summary>
        /// Verilen vergi numarası ile başka bir <see cref="ITaxPayer"/> nesnesinin vergi numarasının aynı olup olmadığını kontrol eder.
        /// </summary>
        /// <param name="taxNumber">Karşılaştırılacak vergi numarası (VKN veya TCKN). Boşluklar ve baştaki sıfırlar için <see cref="IsValidTaxNumber(string)"/> içinde kontroller yapılır.</param>
        /// <param name="other">Karşılaştırılacak <see cref="ITaxPayer"/> nesnesi. <c>null</c> ise dönüş değeri <c>false</c> olur.</param>
        /// <returns>
        /// <c>true</c> — her iki numara geçerli ve eşitse; otherwise <c>false</c>.
        /// </returns>
        /// <remarks>
        /// Bu metot önce karşı tarafın <paramref name="other"/> nesnesinin <c>null</c> olup olmadığını kontrol eder.
        /// Ardından gelen vergi numarasının geçerli bir VKN veya TCKN olup olmadığını <see cref="IsValidTaxNumber(string)"/> ile doğrular.
        /// Geçerlilik sağlanırsa, vergi numaraları doğrudan <see cref="string.Equals(string)"/> ile karşılaştırılır.
        /// </remarks>
        public static bool IsSameTaxNumbers(this string taxNumber, ITaxPayer? other)
        {
            if (other is null)
                return false;

            if (!taxNumber.IsValidTaxNumber())
                return false; //Gelen vergi numarasının boşlukları olsa buradan geçmez, trim'e gerek yok

            //other.TaxNumber'ın geçerliliğinde bir sorun olsa taxNumber ile aynı çıkmazlar, other.TaxNumber'ı kontrol etmeye gerek yok
            return taxNumber.Equals(other.TaxNumber);
        }

        /// <summary>
        /// Vergi için geçerli bir VKN veya TCKN kontrolü
        /// </summary>
        /// <param name="taxPayer">Vergi mükellefi</param>
        /// <returns></returns>
        public static bool IsValidTaxNumber(this ITaxPayer? taxPayer)
        {
            return taxPayer != null && taxPayer.TaxNumber.IsValidTaxNumber();
        }

        /// <summary>
        /// Vergi için geçerli bir VKN veya TCKN kontrolü
        /// </summary>
        /// <param name="taxNumber">VKN veya TCKN</param>
        /// <returns></returns>
        public static bool IsValidTaxNumber(this string taxNumber)
        {
            if (string.IsNullOrEmpty(taxNumber))
                return false;

            //string trimmedTaxNr = taxNumber.Trim();
            //Kapattık ↑ çünkü boşlukların temizlenmiş olarak gelmesi lazım
            //eğer boşluk veya sayısal olmayan başka karakter içeriyorsa geçersiz sayılmalı
            int length = taxNumber.Length;
            if (length == 10)
            {
                return taxNumber.IsValidVKN();
            }
            else if (length == 11)
            {
                return taxNumber.IsValidTCKN();
            }

            return false;
        }


        /// <summary>
        /// Geçerli TC Kimlik No kontrolü
        /// </summary>
        /// <param name="tcNo"></param>
        /// <returns></returns>
        public static bool IsValidTCKN(this string tcNo)
        {
            var digits = tcNo.ParseDigits();
            if (digits.Length != 11 || digits[0] == 0)
                return false;

            var digit10 = ((digits[0] + digits[2] + digits[4] + digits[6] + digits[8]) * 7 - (digits[1] + digits[3] + digits[5] + digits[7])) % 10;
            var digit11 = (digits[0] + digits[1] + digits[2] + digits[3] + digits[4] + digits[5] + digits[6] + digits[7] + digits[8] + digit10) % 10;

            return (digits[9] == digit10 && digits[10] == digit11);
        }

        /// <summary>
        /// Geçerli Vergi Kimlik Numarası kontrolü
        /// </summary>
        /// <param name="vkn">Vergi Kimlik No</param>
        /// <returns></returns>
        public static bool IsValidVKN(this string vkn)
        {
            var vergiNo = vkn.Trim();
            var digits = vergiNo.ParseDigits();
            if (digits.Length != 10)
                return false;

            var sum = 0;
            var tmp = 0;
            for (int i = 0; i < digits.Length - 1; i++)
            {
                tmp = (digits[i] + 10 - (i + 1)) % 10;
                sum = ((int)(tmp == 9 ? sum + tmp : sum + ((tmp * (Math.Pow(2, 10 - (i + 1)))) % 9)));
            }

            return (digits[digits.Length - 1] == (10 - (sum % 10)) % 10);
        }

        private static int[] ParseDigits(this string? s)
        {
            if (string.IsNullOrEmpty(s))
                return new int[0];

            var result = new List<int>();
            for (int i = 0; i < s.Length; i++)
            {
                if (Char.IsDigit(s[i]))
                {
                    result.Add((int)Char.GetNumericValue(s[i]));
                }
            }

            return result.ToArray();
        }
    }
}
