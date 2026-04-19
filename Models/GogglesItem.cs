namespace Prokat.API.Models
{
    public class GogglesItem
    {
        public int ID_Очки { get; set; }
        public string Название { get; set; } = "";
        public string Размер { get; set; } = "";
        public string? ТипЛинзы { get; set; }
    }
}
