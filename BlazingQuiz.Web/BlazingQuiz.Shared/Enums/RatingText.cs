namespace BlazingQuiz.Shared.Enums
{
    public static class RatingText
    {
        public const string VeryBad = "very bad";
        public const string Bad = "bad";
        public const string Normal = "normal";
        public const string Good = "good";
        public const string VeryGood = "very good";
        
        public static readonly string[] AllValues = { VeryBad, Bad, Normal, Good, VeryGood };
        
        public static bool IsValid(string value)
        {
            return AllValues.Contains(value?.Trim().ToLower());
        }
    }
}