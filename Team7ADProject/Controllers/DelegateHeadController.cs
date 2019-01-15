﻿using Microsoft.AspNet.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Team7ADProject.Entities;
using Team7ADProject.Models;
using Microsoft.AspNet.Identity.Owin;
using Team7ADProject.ViewModels;

namespace Team7ADProject.Controllers
{
    //For DH to delegate ADH
    public class DelegateHeadController : Controller
    {
        #region Author: Kay Thi Swe Tun
        // GET: DelegateHead
        public ActionResult Index()
        {
            LogicDB context = new LogicDB();
            DelegateHeadViewModel vmodel = new DelegateHeadViewModel();
           
               return View(vmodel);
           
        }


        [HttpPost]
        public string Delegate(DelegateHeadViewModel model)
        {
            string userId = User.Identity.GetUserId();
            // the value is received in the controller.
            var selectedGender = model.SelectedUser;
     
            LogicDB context = new LogicDB();
            DelegationOfAuthority dd = new DelegationOfAuthority();

          //  AspNetUsers c = context.AspNetUsers.Where(x => x.Id == selectedGender).First();//validate lote yan
            dd.DelegatedBy = "992addd3-07ff-48c5-8c48-9561b163cf57";
            dd.DelegatedTo = "b36a58f3-51f9-47eb-8601-bcc757a8cadb";//selected Employee ID;
            dd.StartDate = new DateTime(2017,3,5);
            dd.EndDate = new DateTime(2017, 5, 5);
            dd.DepartmentId = "BUSI";

            //AspNetUserRoles r = new AspNetUserRoles();
            ApplicationUserManager manager = HttpContext.GetOwinContext().GetUserManager<ApplicationUserManager>();

            manager.RemoveFromRole(dd.DelegatedTo, "Employee");
            manager.AddToRole(dd.DelegatedTo, "Acting Department Head");


            if (ModelState.IsValid)
            {
                context.DelegationOfAuthority.Add(dd);
                context.SaveChanges();
               return "success";
            }
            return "success";

        }
    }

    






    #endregion
}