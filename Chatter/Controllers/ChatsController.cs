using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using Chatter.Models;
using Newtonsoft.Json;
using Microsoft.AspNet.Identity;

namespace Chatter.Controllers
{
    [Authorize]
    [RequireHttps]

    public class ChatsController : Controller
    {
        private ChatterEntities db = new ChatterEntities();

        // GET: Chats
        public ActionResult Index()
        {
            var chats = db.Chats.Include(c => c.AspNetUser);
            return View(chats.ToList());
        }

        public JsonResult TestJson()
        {
            //SQL Statement for inner join on tables Chat and Aspusers:
            //select AspNetUsers.UserName, Chat.Message, Chat.Timestamp
            //from Chat
            //INNER JOIN AspNetUsers on Chat.UserID = AspNetUsers.Id
            //ORDER BY Chat.Timestamp DESC;

            //LINQ translation of above SQL statement- declared as a variable
            var chats = from Chats in db.Chats
                        orderby
                          Chats.Timestamp descending
                        select new
                        {
                            Chats.AspNetUser.UserName,
                            Chats.Message,
                            Chats.Timestamp
                        };

            //Create ToList() method - Serialize data in to JSON which uses Newtonsoft library
            var output = JsonConvert.SerializeObject(chats.ToList());

            //Return the Json to the view
            return Json(output, JsonRequestBehavior.AllowGet);
        }


        //Code used earlier in project to test before LINQ
        //  string jsonTest = "{ \"firstName\": \"Melanie\",\"lastName\": \"McGee\", \"children\": [{\"firstName\": \"Mira\", \"age\": 13 },{\"firstName\": \"Ethan\", \"age\": null }] }";

        //  return Json(jsonTest, JsonRequestBehavior.AllowGet);

            //Create a method to post chats to the SQL database
        public JsonResult PostChats([Bind(Include = "Message")] Chat chat)
        {

            //we need to fill out information for our model so we can easily insert it.

            //We need the following for our model / db. The user, the message, and a timestamp.
            //Since we're using entity framework, all of the Chat table's fields are in a C# model.
            //We work through that (keeps everything in sync) and the model is what is used to update the database
            //So it's a bit different than adding information directly to the database itself.

            //We already have the message. It was passed to us via AJAX-> JSON vals from the View

            //Let's add the Timestamp field's value
            chat.Timestamp = DateTime.Now;
            //Now, since we have a foreign key join on the aspnetuser table, the 2 models in EF (AspNetUser and Chat) reference each other
            // Because of this, we need to get a complete user object, not just the userID 
            string currentUserId = User.Identity.GetUserId();
            chat.AspNetUser = db.AspNetUsers.FirstOrDefault(x => x.Id == currentUserId);


            if (ModelState.IsValid)
            {
                db.Chats.Add(chat);
                db.SaveChanges();
            }
            return new JsonResult() { Data = JsonConvert.SerializeObject(chat.ID), JsonRequestBehavior = JsonRequestBehavior.AllowGet };
        }


        // GET: Chats/Details/5
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Chat chat = db.Chats.Find(id);
            if (chat == null)
            {
                return HttpNotFound();
            }
            return View(chat);
        }

        // GET: Chats/Create
        public ActionResult Create()
        {
            ViewBag.UserID = new SelectList(db.AspNetUsers, "Id", "Email");
            return View();
        }

        // POST: Chats/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "ID,UserID,Message,Timestamp")] Chat chat)
        {
            if (ModelState.IsValid)
            {
                db.Chats.Add(chat);
                db.SaveChanges();
                return RedirectToAction("Index");
            }

            ViewBag.UserID = new SelectList(db.AspNetUsers, "Id", "Email", chat.UserID);
            return View(chat);
        }

        // GET: Chats/Edit/5
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Chat chat = db.Chats.Find(id);
            if (chat == null)
            {
                return HttpNotFound();
            }
            ViewBag.UserID = new SelectList(db.AspNetUsers, "Id", "Email", chat.UserID);
            return View(chat);
        }

        // POST: Chats/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "ID,UserID,Message,Timestamp")] Chat chat)
        {
            if (ModelState.IsValid)
            {
                db.Entry(chat).State = EntityState.Modified;
                db.SaveChanges();
                return RedirectToAction("Index");
            }
            ViewBag.UserID = new SelectList(db.AspNetUsers, "Id", "Email", chat.UserID);
            return View(chat);
        }

        // GET: Chats/Delete/5
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Chat chat = db.Chats.Find(id);
            if (chat == null)
            {
                return HttpNotFound();
            }
            return View(chat);
        }

        // POST: Chats/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Chat chat = db.Chats.Find(id);
            db.Chats.Remove(chat);
            db.SaveChanges();
            return RedirectToAction("Index");
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
