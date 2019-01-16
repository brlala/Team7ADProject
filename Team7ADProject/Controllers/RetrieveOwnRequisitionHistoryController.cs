﻿using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
//using System.Threading.Tasks;
using System.Web;
using System.Web.Mvc;
using Team7ADProject.Entities;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.AspNet.Identity;
using Team7ADProject.ViewModels;
//Authors:Lynn Lynn Oo
namespace Team7ADProject.Controllers
{
    //For employee/department rep can view their own requisition histories
    public class RequisitionHistoryController : Controller
    {
        #region Lynn Lynn Oo
        private LogicDB _context;

        public RequisitionHistoryController()
        {
            _context = new LogicDB();
        }

        // GET: RequisitionHistory
        // To view the Requisition History
        public ActionResult Index(string search)
        {
            OwnRequisitionHistoryViewModel vModel = new OwnRequisitionHistoryViewModel();
            string userid = User.Identity.GetUserId();
            string depId = _context.AspNetUsers.Where(x => x.Id == userid).Select(x => x.DepartmentId).First();
            var stationery = _context.StationeryRequest.Where(x => x.DepartmentId==depId).ToList();
            //forsearch
            string reqid = _context.StationeryRequest.Single(x => x.DepartmentId == depId).RequestId;
            if (search!=null)
            {
                return View(_context.StationeryRequest.Where(x => x.RequestId == reqid).ToList());
            }
            else
                return View(stationery);
            //ViewBag.DepName = _context.StationeryRequest.DepartmentId;

        }

        //RequisitionHistory/Details
        public ActionResult Details(string id)
        {
            OwnRequisitionHistoryViewModel vmodel = new OwnRequisitionHistoryViewModel();
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            vmodel.ReqID = id;
             return View(vmodel);

        }
        //public async Task<ActionResult> Index(string searchString)
        //{
        //    var request = from m in _context.StationeryRequest
        //                 select m;

        //    if (!String.IsNullOrEmpty(searchString))
        //    {
        //        request = request.Where(s => s.DepartmentId.Contains(searchString));
        //    }

        //    return View(await request.ToListAsync());
        //}
        #endregion  
    }
}
