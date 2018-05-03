using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Description;
using FishEDexWebAPI.Models;

namespace FishEDexWebAPI.Controllers
{
    [RoutePrefix("api/Lists")]
    public class CategoriesController : BaseController
    {
        [HttpGet]
        [Route("FishEDex")]
        public IHttpActionResult FishEDex(int id=0)
        {
            Category category;
            if (id != 0)
            {
                category = FishDb.Categories.Where(x => x.Id == id).FirstOrDefault();
            }
            else
            {
                category = FishDb.Categories.OrderBy(x => x.Id).FirstOrDefault();
            }
            if (category == null)
            {
                return NotFound();
            }
            var groupedList = from s in FishDb.Species.Where(x => x.CategoryId == category.Id)
                              let f = FishDb.Fishes.Where(x => x.Title.ToLower() == s.Title.ToLower()).FirstOrDefault()
                              select new 
                              {
                                  //cant use constructors in linq queries, have to use new nonEF class...
                                  Species = new 
                                  {
                                      Id = s.Id,
                                      Title = s.Title,
                                      Description = s.Description,
                                      ImageURL = s.ImageURL,
                                      ThumbnailURL = s.ThumbnailURL,
                                      TileURL = s.TileURL,
                                      Location = s.Location
                                  },
                                  Fish = f == null ? null : new
                                  {
                                      Id = f.Id,
                                      Title = f.Title,
                                      Description = f.Description,
                                      ImageURL = f.ImageURL,
                                      ThumbnailURL = f.ThumbnailURL,
                                      TileURL = f.TileURL,
                                  }
                              };
            var ComboList = groupedList.OrderBy(x => x.Species.Title).ThenBy(x => x.Species.Id).AsEnumerable().Select((item, index) =>
                       new
                       {
                           Number = index + 1,
                           Species = item.Species,
                           Fish = item.Fish
                       }).ToList();
            var model = new
            {
                Id = category.Id,
                Title = category.Title,
                Description = category.Description,
                TileURL = category.TileURL,
                ComboList = ComboList,
                Locations = category.Species.Select(x => x.Location).Distinct(),
                Count = ComboList.Where(x => x.Fish != null).Count()
            };
            return Ok(model);
        }
    }
}