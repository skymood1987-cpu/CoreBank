using System.Text;

namespace MinCoreBank.Models.Utils
{
    public static class BinderNumberGenerator
    {
        private const string Prefix = "IQ-AG";

        public static string GenerateBinderNumber(int branchCode, long transactionId)
        {
            // Format: IQ-AG-001-000000001-5 (23 characters)
            string branchPart = branchCode.ToString("D3"); // 3 digits
            string transactionPart = transactionId.ToString("D9"); // 9 digits

            string baseNumber = $"{branchPart}{transactionPart}";
            string checkDigit = ComputeLuhnCheckDigit(baseNumber);

            return $"{Prefix}-{branchPart}-{transactionPart}-{checkDigit}";
        }

        public static string GenerateTempReference()
        {
            // Temporary reference for immediate feedback
            return $"TMP-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
        }

        private static string ComputeLuhnCheckDigit(string number)
        {
            int sum = 0;
            bool alternate = false;

            // Process from right to left
            for (int i = number.Length - 1; i >= 0; i--)
            {
                int n = int.Parse(number[i].ToString());

                if (alternate)
                {
                    n *= 2;
                    if (n > 9) n = (n % 10) + 1;
                }

                sum += n;
                alternate = !alternate;
            }

            int checkDigit = (10 - (sum % 10)) % 10;
            return checkDigit.ToString();
        }

        public static bool ValidateBinderNumber(string binderNumber)
        {
            if (string.IsNullOrEmpty(binderNumber) || !binderNumber.StartsWith(Prefix))
                return false;

            try
            {
                var parts = binderNumber.Split('-');
                if (parts.Length != 5) return false;

                string baseNumber = $"{parts[2]}{parts[3]}"; // branch + transaction
                string providedCheck = parts[4];
                string calculatedCheck = ComputeLuhnCheckDigit(baseNumber);

                return providedCheck == calculatedCheck;
            }
            catch
            {
                return false;
            }
        }
    }
}