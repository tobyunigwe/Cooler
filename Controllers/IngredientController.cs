using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using CoolerApplication.Models;

namespace CoolerApplication.Controllers
{
    public class IngredientController : Controller
    {
        readonly ApplicationDbContext db = new ApplicationDbContext();

        
        /// <summary>
        /// method for the index page
        /// </summary>
        /// <param name="Search">get the search information and pass it trough the method</param>
        /// <returns>returns view page</returns>
        public ActionResult Index(string Search)
        {
            List<Ingredient> ingredients;
            if (!String.IsNullOrEmpty(Search))
            {
                ingredients = db.Ingredients.Where(i => i.Name.Contains(Search)).ToList();
            }
            else
            {
                ingredients = db.Ingredients.ToList();
            }
            return View(ingredients);
        }

        /// <summary>
        /// method for the details page
        /// </summary>
        /// <param name="id">get the id of the selected ingredient and pass it trough the method</param>
        /// <returns>view page</returns>
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ingredient ingredient = db.Ingredients.Find(id);
            if (ingredient == null)
            {
                return HttpNotFound();
            }
            return View(ingredient);
        }

      /// <summary>
      /// method for the create view page
      /// </summary>
      /// <returns>view page</returns>
        public ActionResult Create()
        {
            return View();
        }

        /// <summary>
        /// method to store the ingredient
        /// </summary>
        /// <param name="ingredient">pass the model trough the method</param>
        /// <returns>returns to action index</returns>
        [HttpPost]
        public ActionResult Create(Ingredient ingredient)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    db.Ingredients.Add(ingredient);
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
            }
            catch (RetryLimitExceededException /* dex */)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.)
                ModelState.AddModelError("", "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
            }

            return RedirectToAction("Index");
        }

     /// <summary>
     /// Edit method that returns the edit view
     /// </summary>
     /// <param name="id">id of the ingredient</param>
     /// <returns>returns view with the ingredient</returns>
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Ingredient ingredient = db.Ingredients.Find(id);
            if (ingredient == null)
            {
                return HttpNotFound();
            }
            return View(ingredient);
        }

       /// <summary>
       /// method that proccesses the edited data
       /// </summary>
       /// <param name="id">id of the ingredient</param>
       /// <returns>ingredient update page</returns>
        [HttpPost]
        public ActionResult Edit(int id)
        {

            var ingredientToUpdate = db.Ingredients.Find(id);
            if (TryUpdateModel(ingredientToUpdate, "",
                new string[] { "Name", "Amount" }))
                try
                {
                    db.SaveChanges();
                    return RedirectToAction("Index");
                }
                catch (RetryLimitExceededException /* dex */)
                {
                    //Log the error (uncomment dex variable name and add a line here to write a log.
                    ModelState.AddModelError("",
                        "Unable to save changes. Try again, and if the problem persists, see your system administrator.");
                }

            return View(ingredientToUpdate);
        }

      /// <summary>
      /// method that redirect to delete page
      /// </summary>
      /// <param name="id">id of the ingredient</param>
      /// <param name="saveChangesError">check if you really want to delete</param>
      /// <returns>ingredient delete page</returns>
        public ActionResult Delete(int? id, bool? saveChangesError = false)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            if (saveChangesError.GetValueOrDefault())
            {
                ViewBag.ErrorMessage = "Delete failed. Try again, and if the problem persists see your system administrator.";
            }
            Ingredient ingredient = db.Ingredients.Find(id);
            if (ingredient == null)
            {
                return HttpNotFound();
            }

            return View(ingredient);
        }

        /// <summary>
        /// Delete function to delete 
        /// </summary>
        /// <param name="id">id of the card</param>
        /// <param name="collection"></param>
        /// <returns>index page of ingredients</returns>
        
        [HttpPost]
        public ActionResult Delete(int? id)
        {
            if(id ==null){
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            try
            {
                Ingredient ingredient = db.Ingredients.Find(id);
                db.Ingredients.Remove(ingredient);
                db.SaveChanges();
            }
            catch (DataException /* dex */)
            {
                //Log the error (uncomment dex variable name and add a line here to write a log.
                return RedirectToAction("Delete", new {id, saveChangesError = true});
            }
            return RedirectToAction("Index");
        }
    }
}


