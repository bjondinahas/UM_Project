namespace UM_Project.Models
{
    public class ParentIndexViewModel
    {
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? UserId { get; set; }
        public int RepresentativeParentId { get; set; }
        public List<ParentChildLink> Children { get; set; } = new();
    }

    public class ParentChildLink
    {
        public int ParentId { get; set; }
        public int StudentId { get; set; }
        public string StudentName { get; set; } = string.Empty;
        public string StudentNumber { get; set; } = string.Empty;
    }
}
