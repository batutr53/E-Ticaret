namespace E_Ticaret.Core.Entities
{
    public class AppUser : IEntity
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Surname { get; set; }
        public string Email { get; set; }
        public string? Phone { get; set; }
        public string Password { get; set; }
        public string? UserName { get; set; }
        public bool IsActive { get; set; }
        public bool IsAdmin { get; set; }
        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;
        public Guid? UserGuid { get; set; } = Guid.NewGuid();

    }
}
