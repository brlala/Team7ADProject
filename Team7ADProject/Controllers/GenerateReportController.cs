﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Team7ADProject.Entities;
using Team7ADProject.ViewModels.GenerateReport;
using Newtonsoft.Json;
using Microsoft.AspNet.Identity;
using ClosedXML.Excel;
using System.IO;
using System.Data;

namespace Team7ADProject.Controllers
{
    //For SS to generate reports
    //Author: Elaine Chan
    [RoleAuthorize(Roles = "Store Supervisor,Department Head")]
    public class GenerateReportController : Controller
    {
        #region Author: Elaine Chan
        // GET: GenerateReport
        public ActionResult GenerateDashboard()
        {
            LogicDB context = new LogicDB();
            String userId = User.Identity.GetUserId();

            String DID = context.AspNetUsers.Where(x => x.Id == userId).First().DepartmentId;

            #region Build VM
            var grvm = GenerateReportViewModel.InitGRVM(DID);

            #endregion

            #region Disbursement by DeptID
            
            if(DID == "STAT") { 
            var gendeptRpt = context.TransactionDetail.Where(x=> x.Disbursement.AcknowledgedBy != null).
                GroupBy(x => new { x.Disbursement.DepartmentId }).
                Select(y => new { DeptID = y.Key.DepartmentId, TotalAmt = y.Sum(z => (z.Quantity * z.UnitPrice)) });

                foreach (var i in gendeptRpt)
                {
                    grvm.deptDP.datapoint.Add(new StringDoubleDPViewModel(i.DeptID, (double)i.TotalAmt));
                }
            }
            else
            {
                var gendeptRpt = context.TransactionDetail.Where(x => x.Disbursement.AcknowledgedBy != null 
                && x.Disbursement.DepartmentId == DID).
                GroupBy(x => new { x.Disbursement.DepartmentId }).
                Select(y => new { DeptID = y.Key.DepartmentId, TotalAmt = y.Sum(z => (z.Quantity * z.UnitPrice)) });

                foreach (var i in gendeptRpt)
                {
                    grvm.deptDP.datapoint.Add(new StringDoubleDPViewModel(i.DeptID, (double)i.TotalAmt));
                }
            }

            
            #endregion

            #region Disbursement by Stationery Category
            
            if (DID == "STAT") { 
            var genstatRpt = context.TransactionDetail.Where(x => x.Disbursement.AcknowledgedBy != null).
                    GroupBy(y => new { y.Stationery.Category }).
                    Select(z => new { itemCat = z.Key.Category, totalAmt = z.Sum(a => (a.Quantity * a.UnitPrice)) });

            foreach (var i in genstatRpt)
            {
                grvm.statDP.datapoint.Add(new StringDoubleDPViewModel(i.itemCat, (double)i.totalAmt));
            }
            }

            else
            {
                var genstatRpt = context.TransactionDetail.Where(x => x.Disbursement.AcknowledgedBy != null 
                && x.Disbursement.DepartmentId== DID).
        GroupBy(y => new { y.Stationery.Category }).
        Select(z => new { itemCat = z.Key.Category, totalAmt = z.Sum(a => (a.Quantity * a.UnitPrice)) });

                foreach (var i in genstatRpt)
                {
                    grvm.statDP.datapoint.Add(new StringDoubleDPViewModel(i.itemCat, (double)i.totalAmt));
                }
            }
            #endregion

            #region Disbursements by entity over time

            int r = 0;
            int r1 = 0;
            foreach (var i in grvm.selectentcategory)
            {
                if (DID == "STAT")
                {
                    var etimeRpt = context.TransactionDetail.Where(x => x.Disbursement.AcknowledgedBy != null && x.Disbursement.DepartmentId == i).
                        OrderBy(x => x.TransactionDate).
                        GroupBy(x => new { x.Stationery.Category, x.TransactionDate.Year, x.TransactionDate.Month }).ToArray().
                        Select(y => new
                        {
                            catID = y.Key.Category,
                            dateval = string.Format("{0} {1}", Enum.Parse(typeof(EnumMonth), y.Key.Month.ToString()), y.Key.Year),
                            totalAmt = y.Sum(z => (z.Quantity * z.UnitPrice))
                        });
                    grvm.entdata.Add(new ChartViewModel(i, i, new List<StringDoubleDPViewModel>()));

                    foreach (var q in etimeRpt)
                    {
                        grvm.entdata[r].datapoint.Add(new StringDoubleDPViewModel(q.dateval, (double)q.totalAmt));

                    }
                    r++;
                }

                else
                {
                    var timeRpt = context.TransactionDetail.Where(x => x.Disbursement.AcknowledgedBy != null && x.Stationery.Category == i
                    && x.Disbursement.DepartmentId == DID).
                        OrderBy(x => x.TransactionDate).
                        GroupBy(x => new { x.Stationery.Category, x.TransactionDate.Year, x.TransactionDate.Month }).ToArray().
                        Select(y => new
                        {
                            catID = y.Key.Category,
                            dateval = string.Format("{0} {1}", Enum.Parse(typeof(EnumMonth), y.Key.Month.ToString()), y.Key.Year),
                            totalAmt = y.Sum(z => (z.Quantity * z.UnitPrice))
                        });
                    grvm.entdata.Add(new ChartViewModel(i, i, new List<StringDoubleDPViewModel>()));

                    foreach (var q in timeRpt)
                    {
                        grvm.entdata[r].datapoint.Add(new StringDoubleDPViewModel(q.dateval, (double)q.totalAmt));

                    }
                    r++;
                }
            }

            #endregion

            #region Disbursements by stationery over time
            
            foreach (var i in grvm.selectstatcategory)
            {
                if (DID == "STAT")
                {
                    var timeRpt = context.TransactionDetail.Where(x => x.Disbursement.AcknowledgedBy != null && x.Stationery.Category == i).
                        OrderBy(x => x.TransactionDate).
                        GroupBy(x => new { x.Disbursement.DepartmentId, x.TransactionDate.Year, x.TransactionDate.Month }).ToArray().
                        Select(y => new
                        {
                            deptID = y.Key.DepartmentId,
                            dateval = string.Format("{0} {1}", Enum.Parse(typeof(EnumMonth), y.Key.Month.ToString()), y.Key.Year),
                            totalAmt = y.Sum(z => (z.Quantity * z.UnitPrice))
                        });
                    grvm.data.Add(new ChartViewModel(i, i, new List<StringDoubleDPViewModel>()));

                    foreach (var q in timeRpt)
                    {
                        grvm.data[r1].datapoint.Add(new StringDoubleDPViewModel(q.dateval, (double)q.totalAmt));

                    }
                    r1++;
                }

                else
                {
                    var timeRpt = context.TransactionDetail.Where(x => x.Disbursement.AcknowledgedBy != null && x.Stationery.Category == i
                    && x.Disbursement.DepartmentId == DID).
                        OrderBy(x => x.TransactionDate).
                        GroupBy(x => new { x.Disbursement.DepartmentId, x.TransactionDate.Year, x.TransactionDate.Month }).ToArray().
                        Select(y => new
                        {
                            deptID = y.Key.DepartmentId,
                            dateval = string.Format("{0} {1}", Enum.Parse(typeof(EnumMonth), y.Key.Month.ToString()), y.Key.Year),
                            totalAmt = y.Sum(z => (z.Quantity * z.UnitPrice))
                        });
                    grvm.data.Add(new ChartViewModel(i, i, new List<StringDoubleDPViewModel>()));

                    foreach (var q in timeRpt)
                    {
                        grvm.data[r1].datapoint.Add(new StringDoubleDPViewModel(q.dateval, (double)q.totalAmt));

                    }
                    r1++;
                }
            }
            
            #endregion
            
            return View(grvm);
        }

        [HttpPost]
        public ActionResult GenerateDashboard(DateTime? fromDateTP, DateTime? toDateTP, string module, List<string> selstatcat, List<string> seldeptcat, List<string> seleecat, List<string> selsscat)
        {
          
            LogicDB context = new LogicDB();
            String userId = User.Identity.GetUserId();

            String DID = context.AspNetUsers.Where(x => x.Id == userId).First().DepartmentId;
            int r = 0;
            int r1 = 0;

            #region Build VM

            var grvm = GenerateReportViewModel.InitGRVM(DID, fromDateTP,toDateTP, module, selstatcat, seldeptcat, seleecat, selsscat);
            
            
            #endregion

            if (module =="Disbursements")
            { 

                #region Disbursement by DeptId
                
                        var gendeptRpt = context.TransactionDetail.
                            Where(x => x.Disbursement.AcknowledgedBy != null && 
                            grvm.selectstatcategory.Any(id=>id==x.Stationery.Category) && 
                            grvm.selectentcategory.Any(id=>id==x.Disbursement.DepartmentId) && 
                            x.TransactionDate >= fromDateTP && x.TransactionDate <= toDateTP).
                            GroupBy(y => new { y.Disbursement.DepartmentId }).
                            Select(z => new { DeptID = z.Key.DepartmentId, TotalAmt = z.Sum(a => (a.Quantity * a.UnitPrice)) });

                        foreach (var i in gendeptRpt)
                        {
                            grvm.deptDP.datapoint.Add(new StringDoubleDPViewModel(i.DeptID, (double)i.TotalAmt));
                        }

                #endregion

                #region Disbursement by Stationery Category
                        
                        var genstatRpt = context.TransactionDetail.
                            Where(x => x.Disbursement.AcknowledgedBy != null &&
                            grvm.selectstatcategory.Any(id => id == x.Stationery.Category) &&
                            grvm.selectentcategory.Any(id => id == x.Disbursement.DepartmentId) && 
                            x.TransactionDate >= fromDateTP && x.TransactionDate <= toDateTP).
                                GroupBy(y => new { y.Stationery.Category }).
                                Select(z => new { itemCat = z.Key.Category, totalAmt = z.Sum(a => (a.Quantity * a.UnitPrice)) });

                        foreach (var i in genstatRpt)
                        {
                            grvm.statDP.datapoint.Add(new StringDoubleDPViewModel(i.itemCat, (double)i.totalAmt));
                        }

                #endregion

                #region Disbursements by entity over time

                foreach (var i in grvm.selectentcategory)
                {
                    var etimeRpt = context.TransactionDetail.Where(x => x.Disbursement.AcknowledgedBy != null && x.Disbursement.DepartmentId == i &&
                            grvm.selectstatcategory.Any(id => id == x.Stationery.Category) &&
                            grvm.selectentcategory.Any(id => id == x.Disbursement.DepartmentId) &&
                            x.TransactionDate >= fromDateTP && x.TransactionDate <= toDateTP).
                                            OrderBy(x => x.TransactionDate).
                                            GroupBy(x => new { x.Stationery.Category, x.TransactionDate.Year, x.TransactionDate.Month }).ToArray().
                                            Select(y => new { catID = y.Key.Category, dateval = string.Format("{0} {1}", Enum.Parse(typeof(EnumMonth), y.Key.Month.ToString()), y.Key.Year), totalAmt = y.Sum(z => (z.Quantity * z.UnitPrice)) });

                    grvm.entdata.Add(new ChartViewModel(i, i, new List<StringDoubleDPViewModel>()));

                    foreach (var j in etimeRpt)
                    {
                        grvm.entdata[r1].datapoint.Add(new StringDoubleDPViewModel(j.dateval, (double)j.totalAmt));
                    }
                    r1++;
                }
                #endregion

                #region Disbursements by stationery over time
                foreach (var i in grvm.selectstatcategory)
                {
                    var timeRpt = context.TransactionDetail.Where(x => x.Disbursement.AcknowledgedBy != null && x.Stationery.Category == i &&
                            grvm.selectstatcategory.Any(id => id == x.Stationery.Category) &&
                            grvm.selectentcategory.Any(id => id == x.Disbursement.DepartmentId) &&
                            x.TransactionDate >= fromDateTP && x.TransactionDate <= toDateTP).
                                            OrderBy(x => x.TransactionDate).
                                            GroupBy(x => new { x.Disbursement.DepartmentId, x.TransactionDate.Year, x.TransactionDate.Month }).ToArray().
                                            Select(y => new { deptID = y.Key.DepartmentId, dateval = string.Format("{0} {1}", Enum.Parse(typeof(EnumMonth), y.Key.Month.ToString()), y.Key.Year), totalAmt = y.Sum(z => (z.Quantity * z.UnitPrice)) });

                    grvm.data.Add(new ChartViewModel(i, i, new List<StringDoubleDPViewModel>()));
                    
                        foreach (var j in timeRpt)
                    {
                        grvm.data[r].datapoint.Add(new StringDoubleDPViewModel(j.dateval, (double)j.totalAmt));
                    }
                    r++;
                }
                #endregion
            }

            if (module == "Requests")
            {
                #region Requests by Dept

                var gendeptRpt = context.TransactionDetail.
                    Where(x => x.StationeryRequest.Status != "Pending Approval" && x.StationeryRequest.Status != "Rejected" && x.TransactionDate >= fromDateTP && x.TransactionDate <= toDateTP &&
                            grvm.selectstatcategory.Any(id => id == x.Stationery.Category) &&
                            grvm.selectentcategory.Any(id => id == x.StationeryRequest.DepartmentId)).
                    GroupBy(y=> new { y.StationeryRequest.DepartmentId}).
                    Select(z => new { DeptID = z.Key.DepartmentId, TotalAmt = z.Sum(a => (a.Quantity * a.UnitPrice)) });
                foreach (var i in gendeptRpt)
                {
                    grvm.deptDP.datapoint.Add(new StringDoubleDPViewModel(i.DeptID, (double)i.TotalAmt));
                }

                #endregion

                #region Requests by Stationery Category

                var genstatRpt = context.TransactionDetail.
                    Where(x => x.StationeryRequest.Status != "Pending Approval" && x.StationeryRequest.Status != "Rejected" && x.StationeryRequest.RequestDate >= fromDateTP && x.TransactionDate <= toDateTP &&
                            grvm.selectstatcategory.Any(id => id == x.Stationery.Category) &&
                            grvm.selectentcategory.Any(id => id == x.StationeryRequest.DepartmentId)).
                        GroupBy(y => new { y.Stationery.Category }).
                        Select(z => new { itemCat = z.Key.Category, totalAmt = z.Sum(a => (a.Quantity * a.UnitPrice)) });

                foreach (var i in genstatRpt)
                {
                    grvm.statDP.datapoint.Add(new StringDoubleDPViewModel(i.itemCat, (double)i.totalAmt));
                }

                #endregion

                #region Requests by entity over time

                foreach (var j in grvm.selectentcategory)
                {

                    var timeRpt = context.TransactionDetail.Where(x => x.StationeryRequest.Status != "Pending Approval" && x.StationeryRequest.Status != "Rejected" && x.StationeryRequest.DepartmentId == j
                    && x.TransactionDate >= fromDateTP && x.TransactionDate <= toDateTP &&
                                grvm.selectstatcategory.Any(id => id == x.Stationery.Category) &&
                                grvm.selectentcategory.Any(id => id == x.StationeryRequest.DepartmentId)).
                        OrderBy(x => x.TransactionDate).
                        GroupBy(x => new { x.TransactionDate.Year, x.TransactionDate.Month }).ToArray().
                        Select(y => new { dateval = string.Format("{0} {1}", Enum.Parse(typeof(EnumMonth), y.Key.Month.ToString()), y.Key.Year), totalAmt = y.Sum(z => (z.Quantity * z.UnitPrice)) });

                    grvm.entdata.Add(new ChartViewModel(j, j, new List<StringDoubleDPViewModel>()));

                    foreach (var i in timeRpt)
                    {
                        grvm.entdata[r1].datapoint.Add(new StringDoubleDPViewModel(i.dateval, (double)i.totalAmt));
                    }
                    r1++;
                }

                #endregion

                #region Requests by stationery over time

                foreach (var j in grvm.selectstatcategory) { 

                var timeRpt = context.TransactionDetail.Where(x => x.StationeryRequest.Status != "Pending Approval" && x.StationeryRequest.Status != "Rejected" && x.Stationery.Category == j 
                && x.StationeryRequest.RequestDate >= fromDateTP && x.TransactionDate <= toDateTP && 
                            grvm.selectstatcategory.Any(id => id == x.Stationery.Category) &&
                            grvm.selectentcategory.Any(id => id == x.StationeryRequest.DepartmentId)).
                    OrderBy(x => x.TransactionDate).
                    GroupBy(x => new { x.TransactionDate.Year, x.TransactionDate.Month }).ToArray().
                    Select(y => new { dateval = string.Format("{0} {1}", Enum.Parse(typeof(EnumMonth), y.Key.Month.ToString()), y.Key.Year), totalAmt = y.Sum(z => (z.Quantity * z.UnitPrice)) });

                    grvm.data.Add(new ChartViewModel(j, j, new List<StringDoubleDPViewModel>()));

                    foreach (var i in timeRpt)
                {
                    grvm.data[r].datapoint.Add(new StringDoubleDPViewModel(i.dateval, (double)i.totalAmt));
                }
                    r++;
                }
                #endregion

            }

            if(module == "ChargeBack")
            {
                #region Charge back by DeptId

                var gendeptRpt = context.TransactionDetail.
                    Where(x => x.TransactionDate >= fromDateTP && x.TransactionDate <= toDateTP &&
                            grvm.selectstatcategory.Any(id => id == x.Stationery.Category) &&
                            grvm.selectentcategory.Any(id => id == x.Disbursement.DepartmentId)).
                    GroupBy(y => new { y.Disbursement.DepartmentId }).
                    Select(z => new { DeptID = z.Key.DepartmentId, TotalAmt = z.Sum(a => (a.Quantity * a.UnitPrice)) });

                foreach (var i in gendeptRpt)
                {
                    grvm.deptDP.datapoint.Add(new StringDoubleDPViewModel(i.DeptID, (double)i.TotalAmt));
                }

                #endregion

                #region Charge back by Stationery Category

                var genstatRpt = context.TransactionDetail.
                    Where(x => x.Disbursement.DepartmentId != null && x.TransactionDate >= fromDateTP && 
                    x.TransactionDate <= toDateTP &&
                            grvm.selectstatcategory.Any(id => id == x.Stationery.Category) &&
                            grvm.selectentcategory.Any(id => id == x.Disbursement.DepartmentId)).
                        GroupBy(y => new { y.Stationery.Category }).
                        Select(z => new { itemCat = z.Key.Category, totalAmt = z.Sum(a => (a.Quantity * a.UnitPrice)) });

                foreach (var i in genstatRpt)
                {
                    grvm.statDP.datapoint.Add(new StringDoubleDPViewModel(i.itemCat, (double)i.totalAmt));
                }

                #endregion

                #region Charge back by entity over time
                foreach (var j in grvm.selectentcategory)
                {
                    var etimeRpt = context.TransactionDetail.Where(x => x.Stationery.Category != null && x.Disbursement.DepartmentId == j &&
                x.TransactionDate >= fromDateTP && x.TransactionDate <= toDateTP &&
                            grvm.selectstatcategory.Any(id => id == x.Stationery.Category) &&
                            grvm.selectentcategory.Any(id => id == x.Disbursement.DepartmentId)).
                    OrderBy(x => x.TransactionDate).
                    GroupBy(x => new { x.TransactionDate.Year, x.TransactionDate.Month }).ToArray().
                    Select(y => new { dateval = string.Format("{0} {1}", Enum.Parse(typeof(EnumMonth), y.Key.Month.ToString()), y.Key.Year), totalAmt = y.Sum(z => (z.Quantity * z.UnitPrice)) });

                    grvm.entdata.Add(new ChartViewModel(j, j, new List<StringDoubleDPViewModel>()));

                    foreach (var i in etimeRpt)
                    {
                        grvm.entdata[r1].datapoint.Add(new StringDoubleDPViewModel(i.dateval, (double)i.totalAmt));
                    }
                    r1++;
                }
                #endregion

                #region Charge back by stationery over time

                foreach (var j in grvm.selectstatcategory)
                {
                    var timeRpt = context.TransactionDetail.Where(x => x.Disbursement.DepartmentId != null && x.Stationery.Category == j && 
                x.TransactionDate >= fromDateTP && x.TransactionDate <= toDateTP &&
                            grvm.selectstatcategory.Any(id => id == x.Stationery.Category) &&
                            grvm.selectentcategory.Any(id => id == x.Disbursement.DepartmentId)).
                    OrderBy(x => x.TransactionDate).
                    GroupBy(x => new { x.TransactionDate.Year, x.TransactionDate.Month }).ToArray().
                    Select(y => new { dateval = string.Format("{0} {1}", Enum.Parse(typeof(EnumMonth), y.Key.Month.ToString()), y.Key.Year), totalAmt = y.Sum(z => (z.Quantity * z.UnitPrice)) });

                    grvm.data.Add(new ChartViewModel(j, j, new List<StringDoubleDPViewModel>()));

                    foreach (var i in timeRpt)
                    {
                        grvm.data[r].datapoint.Add(new StringDoubleDPViewModel(i.dateval, (double)i.totalAmt));
                    }
                    r++;
                }
                #endregion
            }

            if (DID == "STAT")
            {
                if (module == "Purchases")
                {
                    #region Purchases by SupplierID

                    var gendeptRpt = context.TransactionDetail.
                        Where(x => x.TransactionDate >= fromDateTP && x.TransactionDate <= toDateTP &&
                        grvm.selectstatcategory.Any(id => id == x.Stationery.Category) &&
                                grvm.selectentcategory.Any(id => id == x.PurchaseOrder.SupplierId)).
                        GroupBy(y => new { y.PurchaseOrder.SupplierId }).
                        Select(z => new { DeptID = z.Key.SupplierId, TotalAmt = z.Sum(a => (a.Quantity * a.UnitPrice)) });

                    foreach (var i in gendeptRpt)
                    {
                        grvm.deptDP.datapoint.Add(new StringDoubleDPViewModel(i.DeptID, (double)i.TotalAmt));
                    }

                    #endregion

                    #region Purchases by Stationery Category

                    var genstatRpt = context.TransactionDetail.
                        Where(x => x.PurchaseOrder.Status == "Completed" && x.TransactionDate >= fromDateTP &&
                        x.TransactionDate <= toDateTP &&
                        grvm.selectstatcategory.Any(id => id == x.Stationery.Category) &&
                                grvm.selectentcategory.Any(id => id == x.PurchaseOrder.SupplierId)).
                            GroupBy(y => new { y.Stationery.Category }).
                            Select(z => new { itemCat = z.Key.Category, totalAmt = z.Sum(a => (a.Quantity * a.UnitPrice)) });

                    foreach (var i in genstatRpt)
                    {
                        grvm.statDP.datapoint.Add(new StringDoubleDPViewModel(i.itemCat, (double)i.totalAmt));
                    }

                    #endregion

                    #region Purchases by entity over time
                    foreach (var j in grvm.selectentcategory)
                    {
                        var etimeRpt = context.TransactionDetail.Where(x => x.PurchaseOrder.Status == "Completed" && x.PurchaseOrder.SupplierId == j &&
                    x.TransactionDate >= fromDateTP && x.TransactionDate <= toDateTP &&
                        grvm.selectstatcategory.Any(id => id == x.Stationery.Category) &&
                                grvm.selectentcategory.Any(id => id == x.PurchaseOrder.SupplierId)).
                        OrderBy(x => x.TransactionDate).
                        GroupBy(x => new { x.TransactionDate.Year, x.TransactionDate.Month }).ToArray().
                        Select(y => new { dateval = string.Format("{0} {1}", Enum.Parse(typeof(EnumMonth), y.Key.Month.ToString()), y.Key.Year), totalAmt = y.Sum(z => (z.Quantity * z.UnitPrice)) });

                        grvm.entdata.Add(new ChartViewModel(j, j, new List<StringDoubleDPViewModel>()));

                        foreach (var i in etimeRpt)
                        {
                            grvm.entdata[r1].datapoint.Add(new StringDoubleDPViewModel(i.dateval, (double)i.totalAmt));
                        }
                        r1++;
                    }
                    #endregion

                    #region Purchases by stationery over time
                    foreach (var j in grvm.selectstatcategory)
                    {
                        var timeRpt = context.TransactionDetail.Where(x => x.PurchaseOrder.Status == "Completed" && x.Stationery.Category == j &&
                    x.TransactionDate >= fromDateTP && x.TransactionDate <= toDateTP &&
                        grvm.selectstatcategory.Any(id => id == x.Stationery.Category) &&
                                grvm.selectentcategory.Any(id => id == x.PurchaseOrder.SupplierId)).
                        OrderBy(x => x.TransactionDate).
                        GroupBy(x => new { x.TransactionDate.Year, x.TransactionDate.Month }).ToArray().
                        Select(y => new { dateval = string.Format("{0} {1}", Enum.Parse(typeof(EnumMonth), y.Key.Month.ToString()), y.Key.Year), totalAmt = y.Sum(z => (z.Quantity * z.UnitPrice)) });

                        grvm.data.Add(new ChartViewModel(j, j, new List<StringDoubleDPViewModel>()));

                        foreach (var i in timeRpt)
                        {
                            grvm.data[r].datapoint.Add(new StringDoubleDPViewModel(i.dateval, (double)i.totalAmt));
                        }
                        r++;
                    }
                    #endregion
                }

                if (module == "Retrieval")
                {
                    #region Retrieval by Employee

                    var gendeptRpt = context.TransactionDetail.
                        Where(x => x.TransactionDate >= fromDateTP && x.TransactionDate <= toDateTP &&
                        grvm.selectstatcategory.Any(id => id == x.Stationery.Category) &&
                                grvm.selectentcategory.Any(id => id == x.StationeryRetrieval.AspNetUsers.EmployeeName)).
                        GroupBy(y => new { y.StationeryRetrieval.AspNetUsers.EmployeeName }).
                        Select(z => new { DeptID = z.Key.EmployeeName, TotalAmt = z.Sum(a => (a.Quantity * a.UnitPrice)) });

                    foreach (var i in gendeptRpt)
                    {
                        grvm.deptDP.datapoint.Add(new StringDoubleDPViewModel(i.DeptID, (double)i.TotalAmt));
                    }

                    #endregion

                    #region Retrieval by Stationery Category

                    var genstatRpt = context.TransactionDetail.
                        Where(x => x.StationeryRetrieval.RetrievalId != null && x.TransactionDate >= fromDateTP &&
                        x.TransactionDate <= toDateTP &&
                        grvm.selectstatcategory.Any(id => id == x.Stationery.Category) &&
                                grvm.selectentcategory.Any(id => id == x.StationeryRetrieval.AspNetUsers.EmployeeName)).
                            GroupBy(y => new { y.Stationery.Category }).
                            Select(z => new { itemCat = z.Key.Category, totalAmt = z.Sum(a => (a.Quantity * a.UnitPrice)) });

                    foreach (var i in genstatRpt)
                    {
                        grvm.statDP.datapoint.Add(new StringDoubleDPViewModel(i.itemCat, (double)i.totalAmt));
                    }

                    #endregion

                    #region Retrieval by entity over time

                    foreach (var j in grvm.selectentcategory)
                    {
                        var etimeRpt = context.TransactionDetail.Where(x => x.StationeryRetrieval.RetrievalId != null && x.StationeryRetrieval.AspNetUsers.EmployeeName == j &&
                    x.TransactionDate >= fromDateTP && x.TransactionDate <= toDateTP &&
                        grvm.selectstatcategory.Any(id => id == x.Stationery.Category) &&
                                grvm.selectentcategory.Any(id => id == x.StationeryRetrieval.AspNetUsers.EmployeeName)).
                        OrderBy(x => x.TransactionDate).
                        GroupBy(x => new { x.TransactionDate.Year, x.TransactionDate.Month }).ToArray().
                        Select(y => new { dateval = string.Format("{0} {1}", Enum.Parse(typeof(EnumMonth), y.Key.Month.ToString()), y.Key.Year), totalAmt = y.Sum(z => (z.Quantity * z.UnitPrice)) });

                        grvm.entdata.Add(new ChartViewModel(j, j, new List<StringDoubleDPViewModel>()));

                        foreach (var i in etimeRpt)
                        {
                            grvm.entdata[r1].datapoint.Add(new StringDoubleDPViewModel(i.dateval, (double)i.totalAmt));
                        }
                        r1++;
                    }

                    #endregion

                    #region Retrieval by stationery over time
                    foreach (var j in grvm.selectstatcategory)
                    {
                        var timeRpt = context.TransactionDetail.Where(x => x.StationeryRetrieval.RetrievalId != null && x.Stationery.Category == j &&
                    x.TransactionDate >= fromDateTP && x.TransactionDate <= toDateTP &&
                        grvm.selectstatcategory.Any(id => id == x.Stationery.Category) &&
                                grvm.selectentcategory.Any(id => id == x.StationeryRetrieval.AspNetUsers.EmployeeName)).
                        OrderBy(x => x.TransactionDate).
                        GroupBy(x => new { x.TransactionDate.Year, x.TransactionDate.Month }).ToArray().
                        Select(y => new { dateval = string.Format("{0} {1}", Enum.Parse(typeof(EnumMonth), y.Key.Month.ToString()), y.Key.Year), totalAmt = y.Sum(z => (z.Quantity * z.UnitPrice)) });

                        grvm.data.Add(new ChartViewModel(j, j, new List<StringDoubleDPViewModel>()));

                        foreach (var i in timeRpt)
                        {
                            grvm.data[r].datapoint.Add(new StringDoubleDPViewModel(i.dateval, (double)i.totalAmt));
                        }
                        r++;
                    }
                }
                #endregion

            }
                return View(grvm);
            
        }

        public FileResult ExportRpt(DateTime? fromDateTP, DateTime? toDateTP, string module, List<string> selstatcat, List<string> seldeptcat)
        {
            String userId = User.Identity.GetUserId();

            DataTable dt = new DataTable("Report");

            LogicDB context = new LogicDB();

            if(seldeptcat == null)
            {
                seldeptcat = context.Department.Select(x => x.DepartmentId).ToList();
            }

            if(selstatcat == null)
            {
                selstatcat = context.Stationery.Select(x => x.Category).ToList();
            }

            if (module == "Disbursements")
            {

                dt.Columns.AddRange(new DataColumn[6] { new DataColumn("Disbursement ID"),
                new DataColumn("Department"),
            new DataColumn("Acknowledged By"),
                new DataColumn("Disbursement Date"),
                new DataColumn("RequestID"),
                new DataColumn("Status")});

                var reportData = context.TransactionDetail.Where(x => x.Disbursement.Date >= fromDateTP && x.Disbursement.Date <= toDateTP && x.Disbursement.AcknowledgedBy != null &&
                seldeptcat.Any(id => id == x.Disbursement.DepartmentId) && selstatcat.Any(id => id == x.Stationery.Category)).ToList().Select(y => new
                {
                    y.Disbursement.DisbursementId,
                    y.Disbursement.Department.DepartmentName,
                    y.Disbursement.AspNetUsers.EmployeeName,
                    y.Disbursement.Date,
                    y.Disbursement.RequestId,
                    y.Disbursement.Status
                });

                foreach (var i in reportData)
                {
                    dt.Rows.Add(i.DisbursementId, i.DepartmentName, i.EmployeeName, i.Date, i.RequestId, i.Status);
                }

            }
                        
            if(module == "ChargeBack")
            {

                dt.Columns.AddRange(new DataColumn[6] { new DataColumn("Disbursement ID"),
                new DataColumn("Department"),
            new DataColumn("Acknowledged By"),
                new DataColumn("Disbursement Date"),
                new DataColumn("RequestID"),
                new DataColumn("Status")});

                var reportData = context.TransactionDetail.Where(x => x.Disbursement.Date >= fromDateTP && x.Disbursement.Date <= toDateTP &&
                seldeptcat.Any(id => id == x.Disbursement.DepartmentId) && selstatcat.Any(id => id == x.Stationery.Category)).ToList().Select(y => new
                {
                    y.Disbursement.DisbursementId,
                    y.Disbursement.Department.DepartmentName,
                    y.Disbursement.AspNetUsers.EmployeeName,
                    y.Disbursement.Date,
                    y.Disbursement.RequestId,
                    y.Disbursement.Status
                });

                foreach (var i in reportData)
                {
                    dt.Rows.Add(i.DisbursementId, i.DepartmentName, i.EmployeeName, i.Date, i.RequestId, i.Status);
                }
            }
           
                using (XLWorkbook wb = new XLWorkbook())
            {
                wb.Worksheets.Add(dt);
                using (MemoryStream st = new MemoryStream())
                {
                    wb.SaveAs(st);
                    return File(st.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", String.Format("{0}_Report.xlsx",module));
                }
            }

        }
    }

             #endregion


    }
