using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Web;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using Newtonsoft.Json;
using Microsoft.AspNet.Identity;
using FishEDexWebAPI.Models;

namespace FishEDexWebAPI.Controllers
{
    [RoutePrefix("api/Fish")]
    public class FishController : BaseController
    {
        //api/Fish/Index?after=1&take=10
        [HttpGet]
        [Route("Index")]
        public IHttpActionResult FishIndex(int after=0, int take=25)
        {
            IQueryable<Fish> fishList = FishDb.Fishes.OrderByDescending(x => x.Id);
            if (after > 0)
            {
                fishList = fishList.Where(x => x.Id < after);
            }
            var model = new
            {
                Title = "Fish Index",
                Description = "All the fish",
                CanCreate = AuthorizeCurrentUser(),
                FishList = fishList.Take(take).ToList()
            };
            return Ok(model);
        }

        //api/Fish/5/Details
        [HttpGet]
        [Route("{fishId}/Details")]
        public IHttpActionResult FishDetails(int fishId)
        {
            var fish = FishDb.Fishes.Where(x => x.Id == fishId).FirstOrDefault();
            if (fish == null)
            {
                return NotFound();
            }
            var model = new
            {
                Title = fish.Title,
                Description = fish.Description,
                CanEdit = AuthorizeCurrentUser(fish),
                CanDelete = AuthorizeCurrentUser(fish),
                UserName= FishDb.AspNetUsers.Where(x => x.Id == fish.CreatedUserId).Select(x => x.UserName).FirstOrDefault()
            };
            return Ok(model);
        }

        // PUT: api/Fish/Create    
        [HttpPost]
        [Authorize]
        //[ValidateAntiForgeryToken]
        [Route("Create")]
        public IHttpActionResult FishCreate(Fish model, HttpPostedFileBase imageFile)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            //modify Title and Description
            var fish = new Fish
            {
                Title = (model.Title != null && model.Title.ToLower() != "null" ? model.Title : ""),
                Description = (model.Description != null && model.Description.ToLower() != "null" ? model.Description : ""),
                CreatedUserId = User.Identity.GetUserId(),
                CreatedDate = DateTime.Now
            };

            //resize new image
            UpdateFishImage(imageFile, fish);

            FishDb.Fishes.Add(model);
            FishDb.SaveChanges();

            return CreatedAtRoute("DefaultApi", new { id = model.Id }, model);
        }

        // POST: api/Fish/{fishId}/Edit
        [HttpPost]
        [Authorize]
        //[ValidateAntiForgeryToken]
        [Route("{fishId}/Edit")]
        public IHttpActionResult FishEdit(Fish model, HttpPostedFileBase imageFile)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var fish = FishDb.Fishes.Where(x => x.Id == model.Id).FirstOrDefault();
            if (fish == null)
            {
                return NotFound();
            }
            if (!AuthorizeCurrentUser(fish))
            {
                return BadRequest();
            }

            //modify Title and Description
            fish.Title = (model.Title != null && model.Title.ToLower() != "null" ? model.Title : "");
            fish.Description = (model.Description != null && model.Description.ToLower() != "null" ? model.Description : "");

            //resize image
            UpdateFishImage(imageFile, fish);

            FishDb.Entry(fish).State = EntityState.Modified;
            FishDb.SaveChanges();

            return StatusCode(HttpStatusCode.NoContent);
        }

        // DELETE: api/Fish/5/delete
        [HttpPost]
        [Authorize]
        //[ValidateAntiForgeryToken]
        [Route("{fishId}/Delete")]
        public IHttpActionResult DeleteFish(int id)
        {
            Fish fish = FishDb.Fishes.Find(id);
            if (fish == null)
            {
                return NotFound();
            }
            if (!AuthorizeCurrentUser(fish))
            {
                return BadRequest();
            }

            ControllerHelpers.DeleteBlobs(fish, imagesBlobContainer);
            FishDb.Fishes.Remove(fish);
            FishDb.SaveChanges();

            return Ok(fish);
        }
        private void UpdateFishImage(HttpPostedFileBase imageFile, Fish fish)
        {
            if (ControllerHelpers.IsValidImage(imageFile))
            {
                ControllerHelpers.DeleteBlobs(fish, imagesBlobContainer);

                var g = Guid.NewGuid().ToString();
                var imageBlob = ControllerHelpers.UploadBlobFile(imageFile.InputStream, imageFile.ContentType, g, imagesBlobContainer);
                var thumbBlob = ControllerHelpers.UploadBlobThumb(imageFile.InputStream, imageFile.ContentType, g, imagesBlobContainer, true);
                var tileBlob = ControllerHelpers.UploadBlobTile(imageFile.InputStream, imageFile.ContentType, g, imagesBlobContainer, true);

                fish.ImageURL = imageBlob.Uri.ToString();
                fish.ThumbnailURL = thumbBlob.Uri.ToString();
                fish.TileURL = tileBlob.Uri.ToString();
            }
            //Add lastedit info
            fish.LastEditUserId = User.Identity.GetUserId();
            fish.LastEditDate = DateTime.Now;
        }
        private bool AuthorizeCurrentUser()
        {
            bool retval = false;
            //User.Identity.GetUserId more consistent than IsAuthenticated
            if (!string.IsNullOrWhiteSpace(User.Identity.GetUserId()))
            {
                retval = true;
            }
            return retval;
        }
        //edit or delete existing
        private bool AuthorizeCurrentUser(Fish f)
        {
            bool retval = false;
            //note role is case sensitive
            if (User.IsInRole("Admin") || f == null)
            {
                retval = true;
            }
            //only owner can edit or delete
            else if (f != null && !string.IsNullOrWhiteSpace(User.Identity.GetUserId()))
            {
                retval = User.Identity.GetUserId() == f.CreatedUserId;
            }
            return retval;
        }
    }
}