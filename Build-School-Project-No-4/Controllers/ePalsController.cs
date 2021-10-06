﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Build_School_Project_No_4.Services;
using Build_School_Project_No_4.ViewModels;
using Build_School_Project_No_4.Repositories;
using Build_School_Project_No_4.DataModels;


namespace Build_School_Project_No_4.Controllers
{
    public class ePalsController : Controller
    {
        private readonly ProductService _productService;
        private readonly EPalContext _ctx;
        private readonly DetailServices _detailService;
        private readonly AddToCartService _cartService;

        public ePalsController()
        {
            _productService = new ProductService();
            _detailService = new DetailServices();
            _ctx = new EPalContext();
            _cartService = new AddToCartService();
        }

        public ActionResult ePal(int? id)
        {
            if (!id.HasValue)
            {
                return RedirectToAction("ePal", "ePals", new { id = 1 });
            }
            var ProductCards = _productService.GetProductCardsData(id.Value);
            var GamesDeatils = _productService.GetGamesAllAndDeatils(id.Value);

            GroupViewModel result = new GroupViewModel
            {
                GamesDetails = GamesDeatils,
                ProductCards = ProductCards
            };
            ViewBag.ProductCard = _productService.GetProductCardsJson(id.Value);
            return View("ePal",result);
        }

        public ActionResult GamesJson(int id)
        {
            ViewBag.ProductCard = _productService.GetProductCardsJson(id);

            return View();
        }




        public ActionResult NotFound()
        {
            return View();
        }
        [HttpGet]
        public ActionResult DetailPage(int? id)
        {

            if (id == null)
            {
                return RedirectToAction("Index");
            }
            var playerListing = _detailService.FindPlayerListing(id);
            if (playerListing == null)
            {
                return RedirectToAction("NotFound");
            }
            GroupViewModel groupVM = new GroupViewModel
            {
                Deets = playerListing
            };
            return View(groupVM);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult DetailPage(GroupViewModel AddCartVM, string startTime, int id)
        {
            var unpaid = _cartService.CreateUnpaidOrder(AddCartVM, startTime, id);
            using (var tran = _ctx.Database.BeginTransaction())
            {
                try
                {
                    _ctx.Orders.Add(unpaid);
                    _ctx.SaveChanges();
                    tran.Commit();
                    return RedirectToAction("Checkout", unpaid);
                }
                catch (Exception ex)
                {
                    tran.Rollback();
                    return Content("add failed" + ex.ToString());
                }
            }

        }
    }
}