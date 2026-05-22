namespace UM_Project.Models
{
    public static class RoleNames
    {
        public const string SuperAdmin = "SuperAdmin";
        public const string Admin = "Admin";
        public const string Professor = "Professor";
        public const string Student = "Student";
        public const string Parent = "Parent";

        public const string AdminPanel = "SuperAdmin,Admin";
        public const string AdminAndProfessor = "SuperAdmin,Admin,Professor";

        public static readonly string[] All =
        {
            SuperAdmin, Admin, Professor, Student, Parent
        };
    }
}
