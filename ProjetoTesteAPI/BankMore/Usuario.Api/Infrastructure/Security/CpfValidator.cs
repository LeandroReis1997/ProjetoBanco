namespace Usuario.Api.Infrastructure.Security
{
    public static class CpfValidator
    {
        public static bool Validar(string cpf)
        {
            if (string.IsNullOrWhiteSpace(cpf)) return false;
            var digits = new string(cpf.Where(char.IsDigit).ToArray());
            if (digits.Length != 11) return false;
            if (digits.Distinct().Count() == 1) return false;
            var nums = digits.Select(c => c - '0').ToArray();
            var soma1 = 0;
            for (var i = 0; i < 9; i++) soma1 += nums[i] * (10 - i);
            var resto1 = soma1 % 11;
            var dv1 = resto1 < 2 ? 0 : 11 - resto1;
            if (nums[9] != dv1) return false;
            var soma2 = 0;
            for (var i = 0; i < 10; i++) soma2 += nums[i] * (11 - i);
            var resto2 = soma2 % 11;
            var dv2 = resto2 < 2 ? 0 : 11 - resto2;
            return nums[10] == dv2;
        }
    }
}
