using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Web.Mvc;

namespace CoolerApplication.Models
{
    public class Recipe
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)] 
        public int id { get; set; }

        [Required] public string Name { get; set; }

        public string[] SelectedIngredients { get; set; }

        [DisplayName("Ingredients")]
        public IList<RecipeIngredients> RecipeIngredients { get; set; }

        public bool EnoughIngredients { get; set; }
    }
}
