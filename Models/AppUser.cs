namespace Prokat.API.Models
{
    public class AppUser
    {
        public int ID_Учетной_записи { get; set; }
        public string Логин { get; set; } = "";
        public string ПарольХеш { get; set; } = "";
        public string Роль { get; set; } = "User";
        public int? ID_Клиента { get; set; }

        public Client? Client { get; set; }
    }
}
