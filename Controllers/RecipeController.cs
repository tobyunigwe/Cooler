using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics.Eventing.Reader;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using CoolerApplication.Models;

namespace CoolerApplication.Controllers
{
    public class RecipeController : Controller
    {
        private readonly ApplicationDbContext db = new ApplicationDbContext();

        /// <summary>
        /// method for the index page
        /// </summary>
        /// <param name="Search">get the search information and pass it trough the method</param>
        /// <returns>returns view page</returns>
        public async Task<ActionResult> Index(string Search)
        {
            List<Recipe> recipes;
            DbSet<Ingredient> ingredients = db.Ingredients;

            if (!String.IsNullOrEmpty(Search))
            {
                recipes = db.Recipes.Include(r => r.RecipeIngredients.Select(i => i.Ingredient)).Where(i => i.Name.Contains(Search)).ToList();
            }
            else
            {
                recipes = db.Recipes.Include(r => r.RecipeIngredients.Select(i => i.Ingredient)).ToList();
            }
            

            //Check if recipe can be made.
            foreach (var recipe in recipes)
            {
                int ingredientAvailable = 0;
                int ingredientUnavailable = 0;

                foreach (var recipeIngredient in recipe.RecipeIngredients)
                {
                    Ingredient ingredient = recipeIngredient.Ingredient;

                    if (ingredient.Amount < recipeIngredient.Amount)
                    {
                        recipeIngredient.EnoughIngredients = false;
                        ingredientUnavailable++;
                    }
                    else
                    {
                        recipeIngredient.EnoughIngredients = true;
                        ingredientAvailable++;
                    }
                }

                recipe.EnoughIngredients = ingredientUnavailable < 1;
            }
            return View(recipes);
        }

        /// <summary>
        /// method for the details page
        /// </summary>
        /// <param name="id">get the id of the selected recipe and pass it trough the method</param>
        /// <returns>view page</returns>
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Recipe recipe = db.Recipes.Include(r => r.RecipeIngredients.Select(i => i.Ingredient)).First(r => r.id == id);
            if (recipe == null)
            {
                return HttpNotFound();
            }

            return View(recipe);
        }

        /// <summary>
        /// method for the create view page
        /// </summary>
        /// <returns>view page</returns>
        public ActionResult Create()
        {
            ViewBag.AllIngredients = db.Ingredients.ToList();

            return View();
        }

        /// <summary>
        /// method to store the ingredient
        /// </summary>
        /// <param name="recipe">pass the model trough the method</param>
        /// <returns>returns to action index</returns>
        [HttpPost]
        public ActionResult Create(Recipe recipe)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    DbSet<Ingredient> allIngredients = db.Ingredients;
                    recipe = db.Recipes.Add(recipe);
                    db.SaveChanges();
                    recipe = db.Recipes.Include(r => r.RecipeIngredients.Select(i => i.Ingredient)).First(r => r.id == recipe.id);

                    foreach (var item in allIngredients)
                    {
                        var amountString = Request.Form["SelectedIngredients[" + item.Id + "]"];
                        if (amountString != null && amountString != "")
                        {
                            int amount = Int32.Parse(amountString);
                            if (amount > 0)
                            {
                                recipe.RecipeIngredients.Add(new RecipeIngredients
                                {
                                    RecipeId = recipe.id,
                                    Recipe = recipe,
                                    IngredientID = item.Id,
                                    Ingredient = item,
                                    Amount = amount
                                });
                            }
                        }
                    }
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch (RetryLimitExceededException /* dex */)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.)
                ModelState.AddModelError("",
                    "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }

            return RedirectToAction("Index");
        }


        /// <summary>
        /// Edit method that returns the edit view
        /// </summary>
        /// <param name="id">id of the recipe</param>
        /// <returns>returns view with the recipe</returns>

        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            Recipe recipe = db.Recipes.Include(r=>r.RecipeIngredients.Select(ri=>ri.Ingredient)).First(r=>r.id == id);
            if (recipe == null)
            {
                return HttpNotFound();
            }

            List<List<KeyValuePair<string,string>>> list = new List<List<KeyValuePair<string, string>>>();
            var AllIngredients = db.Ingredients.ToList();
            List<int> selectedIngredientIDs = new List<int>();

            foreach (RecipeIngredients recipeIngredient in recipe.RecipeIngredients)
            {
                var tmplist = new List<KeyValuePair<string,string>>();
                tmplist.Add(new KeyValuePair<string, string>("Id", recipeIngredient.IngredientID.ToString()));
                tmplist.Add(new KeyValuePair<string, string>("Name", recipeIngredient.Ingredient.Name));
                tmplist.Add(new KeyValuePair<string, string>("Amount", recipeIngredient.Amount.ToString()));
                list.Add(tmplist);
                AllIngredients = AllIngredients.Where(i => i.Id != recipeIngredient.IngredientID).ToList();
            }

            foreach (Ingredient ingredient in AllIngredients)
            {
                var tmplist = new List<KeyValuePair<string,string>>();
                tmplist.Add(new KeyValuePair<string, string>("Id", ingredient.Id.ToString()));
                tmplist.Add(new KeyValuePair<string, string>("Name", ingredient.Name));
                tmplist.Add(new KeyValuePair<string, string>("Amount", 0.ToString()));
                list.Add(tmplist);
            }

            ViewBag.AllIngredients = list;
            ViewBag.selectedIngredientIDs = selectedIngredientIDs;

            return View(recipe);
        }


        /// <summary>
        /// method that proccesses the edited data
        /// </summary>
        /// <param name="recipeData">the recipe data with the ingredients</param>
        /// <returns>recipe update page</returns>
        [HttpPost, ActionName("Edit")]
        public ActionResult EditIngredient(Recipe recipeData)
        {
            if (recipeData.id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
             
            var recipeToUpdate = db.Recipes.Find(recipeData.id);
            if (TryUpdateModel(recipeToUpdate, "",
                new string[] {"Name"}))
            {
                try
                {
                    DbSet<Ingredient> allIngredients = db.Ingredients;
                    recipeToUpdate = db.Recipes.Include(r => r.RecipeIngredients.Select(i => i.Ingredient)).First(r => r.id == recipeToUpdate.id);
                    recipeToUpdate.RecipeIngredients.Clear();
                    foreach (var item in allIngredients)
                    {
                        var amountString = Request.Form["SelectedIngredients[" + item.Id + "]"];
                        if (amountString != null && amountString != "")
                        {
                            int amount = Int32.Parse(amountString);
                            if (amount > 0)
                            {
                                recipeToUpdate.RecipeIngredients.Add(new RecipeIngredients
                                {
                                    RecipeId = recipeToUpdate.id,
                                    Recipe = recipeToUpdate,
                                    IngredientID = item.Id,
                                    Ingredient = item,
                                    Amount = amount
                                });
                            }
                        }
                    }
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    //Log the error (uncomment dex variable name and add a line here to write a log.
                    ModelState.AddModelError("",
                        "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }
            }

            return View(recipeToUpdate);
        }

        /// <summary>
        /// method that redirect to delete page
        /// </summary>
        /// <param name="id">id of the recipe</param>
        /// <param name="saveChangesError">check if you really want to delete</param>
        /// <returns>recipe delete page</returns>
        public ActionResult Delete(int id, bool? saveChangesError = false)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            if (saveChangesError.GetValueOrDefault())
            {
                ViewBag.ErrorMessage =
                    "Delete failed. Try again, and if the problem persists see your system administrator.";
            }

            Recipe recipe = db.Recipes.Find(id);
            if (recipe == null)
            {
                return HttpNotFound();
            }

            return View(recipe);
        }

        /// <summary>
        /// Delete function to delete 
        /// </summary>
        /// <param name="id">id of the card</param>
        /// <param name="collection"></param>
        /// <returns>index page of recipes</returns>
        [HttpPost]
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            try
            {
                Recipe recipe = db.Recipes.Find(id);
                db.Recipes.Remove(recipe);
                db.SaveChanges();
            }
            catch (DataException /* dex */)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.
                return RedirectToAction("Delete", new {id = id, saveChangesError = true});
            }

            return RedirectToAction("Index");
        }
    }
}