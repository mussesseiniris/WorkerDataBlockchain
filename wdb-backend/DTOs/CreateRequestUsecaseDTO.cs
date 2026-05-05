using wdb_backend.Models;
namespace wdb_backend.DTOs;



public class CreateRequestUsecaseDTO
{
    public string Email { get; set; }
    public List<string> InfoDesc { get; set; }
    public string Reason { get; set; }
}

