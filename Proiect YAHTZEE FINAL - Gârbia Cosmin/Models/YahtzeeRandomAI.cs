using YahtzeeProject.Models;

public class YahtzeeRandomAI : YahtzeeAI
{
    private Random random = new Random();

    public override string ChooseCategory(int[] dice, Dictionary<string, int?> availableCategories)
    {
        // Alege o categorie aleatorie
        var categories = availableCategories
            .Where(c => c.Value == null)
            .Select(c => c.Key)
            .ToList();

        return categories[random.Next(categories.Count)];
    }

    public override int[] GetDiceToKeep(int[] dice, string targetCategory)
    {
        // 50% șansă să păstreze sau să schimbe fiecare zar
        return dice.Select((_, index) => random.Next(0, 2) == 0 ? -1 : index)
                   .Where(i => i != -1)
                   .ToArray();
    }
}
