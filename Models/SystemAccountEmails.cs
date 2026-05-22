namespace UM_Project.Models
{
    public static class SystemAccountEmails
    {
        public const string SuperAdmin = "superadmin@shkollademo.edu";
        public const string Admin = "admin@shkollademo.edu";
        public const string Domain = "shkollademo.edu";

        public static readonly string[] Protected =
        [
            SuperAdmin,
            Admin
        ];

        private static readonly (string Legacy, string Current)[] LegacyMigrations =
        [
            ("superadmin@umproject.com", SuperAdmin),
            ("admin@umproject.com", Admin)
        ];

        public static bool IsProtected(string? email) =>
            !string.IsNullOrWhiteSpace(email)
            && Protected.Contains(email, StringComparer.OrdinalIgnoreCase);

        public static IReadOnlyList<(string Legacy, string Current)> GetLegacyMigrations() => LegacyMigrations;
    }
}
