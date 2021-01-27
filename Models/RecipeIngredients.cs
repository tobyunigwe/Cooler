using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CoolerApplication.Models
{
    public class RecipeIngredients
    {
        [Key, Column(Order = 1)]
        public int RecipeId { get; set; }
        public Recipe Recipe { get; set; }

        [Key, Column(Order = 2)]
        public int IngredientID { get; set; }
        public Ingredient Ingredient { get; set; }

        public int Amount { get; set; }

        public bool EnoughIngredients { get; set; }
    }
}
