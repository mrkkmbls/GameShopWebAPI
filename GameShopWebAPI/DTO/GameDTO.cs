using System.ComponentModel.DataAnnotations;

namespace GameShopWebAPI.DTO
{
    public class GameDTO
    {
        //[Required]
        //public string Title { get; set; }
        //public string? Description { get; set; }
        //public bool Status { get; set; }
        //public DateTime? DueDate { get; set; }




        //public GameDTO()
        //{
        //}
        //public GameDTO( string title, string description, bool status, DateTime dueDate)
        //{
        //    Title = title;
        //    Title = title;
        //    Description = description;
        //    Status = status;
        //    DueDate = dueDate;
        //}

        public int Id { get; set; }

        public string gameName { get; set; }

        public string gameDescription { get; set; }

        public int gamePrice { get; set; }

        public GameDTO()
        {

        }

        public GameDTO(int id, string gameName, string gameDescription, int gamePrice)
        {
            Id = id;
            this.gameName = gameName;
            this.gameDescription = gameDescription;
            this.gamePrice = gamePrice;
        }

    }
}
