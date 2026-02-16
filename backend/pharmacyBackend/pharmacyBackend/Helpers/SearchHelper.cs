namespace pharmacyBackend.Helpers
{
    public static class SearchHelper
    {
        private const string eng = "qwertyuiop[]asdfghjkl;'zxcvbnm,.";
        private const string rus = "йцукенгшщзхъфывапролджэячсмитьбю";

        public static string ConvertLayout(string input)
        {
            if (string.IsNullOrEmpty(input))
            {
                return input;
            }
            var result = input.ToLower().Select(c =>
            {
                int index = eng.IndexOf(c);
                return index != -1 ? rus[index] : c;
            });
            return new string(result.ToArray());
        }

        //алгоритм левенштейна, вычисляем превращение стоимости превращения одного слова в другео
        public static int LevensheteinAlgorithm(string source, string target)
        {
            if(string.IsNullOrEmpty(source))
            {
                return target?.Length ?? 0;
            }

            if(string.IsNullOrEmpty(target))
            {
                return source.Length;
            }

            source = source.ToLower();
            target = target.ToLower();

            int[,] result = new int[source.Length + 1, target.Length + 1];


            for (int i = 0; i <= source.Length; result[i, 0] = i++)
            {
            }

            for (int j = 0; j <= target.Length; result[0, j] = j++)
            {
            }

            for (int i = 1; i <= source.Length; i++)
            {
                for (int j = 1; j <= target.Length; j++)
                {
                    //если буквы совпадают, то правка не нужна и ставим 0
                    //если буквы разные то ставим 1
                    int cost = (target[j - 1] == source[i - 1]) ? 0 : 1;
                    //выбираем наименьшее удаление символа, вставка символа или обычная замена
                    result[i, j] = Math.Min(
                    Math.Min(result[i - 1, j] + 1, result[i, j - 1] + 1),
                    result[i - 1, j - 1] + cost);
                }
            }

            //сумма в последней ячейке
            return result[source.Length, target.Length];
        }
    }
}
