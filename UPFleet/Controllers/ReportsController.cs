﻿using Newtonsoft.Json;
using System;
using System.Linq;
using System.Web.Mvc;
using UPFleet.Models;
using UPFleet.Repositories;
using UPFleet.ViewModels;

namespace UPFleet.Controllers
{
    public class ReportsController : Controller
    {
        private readonly IRepository _repository;

        public ReportsController()
        {
            _repository = new Repository(); 
        }

        //show owner table data 
        public ActionResult Owner_reports()
        {
            var obj = _repository.GetOwnerList();
            return View(obj);
        }

        public ActionResult Barge_By_Owner()
        {
            return View();
        }

        public ActionResult Owner_list()
        {
            var data = TempData["Datalist"] as string;
            var obj = _repository.GetOwnerList().Where(m => _repository.GetBargeList().Any(b => b.Owner == m.OwnerName)).OrderBy(m => m.OwnerName).ToList();
            obj.Insert(0, new Owner { OwnerName = "All" });
            return Json(obj,JsonRequestBehavior.AllowGet);
        }
        public ActionResult BargeByOwner(string SelectOwner)
        {
            if (SelectOwner == "All")
            {
                var obj = _repository.GetBargeList().OrderBy(m => m.Barge_Name).ToList();

                return Json(obj,JsonRequestBehavior.AllowGet);
            }
            else
            {
                var obj = _repository.GetBargeList().Where(m => m.Owner == SelectOwner).OrderBy(m => m.Barge_Name).ToList();

                return Json(obj,JsonRequestBehavior.AllowGet);
            }

        }
        public ActionResult View_Exported_Archive()
        {
            var obj = _repository.GetPeachtreeExportedArchiveList();
            return View(obj);
        }

        //transfer detail table view
        public ActionResult Transfer_Details()
        {
            var Bargelist = _repository.GetBargeList();
            var Ownerlist = _repository.GetOwnerList();
            var transferlist = _repository.GetTransferList();
            var transactionlist = _repository.GetTransactionList();
            var obj = (
                from tr in transferlist
                join t in transactionlist on tr.Transaction equals t.TransactionNo
                join b in Bargelist on t.Barge equals b.Barge_Name
                join o in Ownerlist on b.Owner equals o.OwnerName
                where tr.To > new DateTime(2023, 1, 1) && tr.Status != "Billed" && !string.IsNullOrEmpty(tr.Status)
                select new HoursInFleetViewModel
                {
                    GetBarge = b,
                    GetOwner = o,
                    GetTransaction = t,
                    GetTransfer = tr
                }
            ).ToList();
            return View(obj);
        }
        public ActionResult TransferSummary()
        {
            var bargeslist = _repository.GetBargeList();
            var transactionslist = _repository.GetTransactionList().Where(m => m.Status != "Billed").ToList();
            var transferlist = _repository.GetTransferList().Where(m => m.Status != "Billed" && (m.From != null || m.To != null)).ToList();

            var viewmodelobj = (from tr in transactionslist
                                join b in bargeslist on tr.Barge equals b.Barge_Name
                                select new UPFleetViewModel()
                                {
                                    Barge = b,
                                    Transaction = tr,
                                    TransferList = transferlist.Where(m => m.Transaction == tr.TransactionNo).ToList()
                                }).ToList().OrderBy(m => m.Barge?.Barge_Name);

            return View(viewmodelobj);
        }

        public ActionResult PreviewToExport_Page()
        {
            return View();

        }
        public JsonResult PreviewReport(string SelectOwner, string SelectStatus, string fromDate, string toDate)
        {
            var transactionslist = _repository.GetTransactionListforToBill();
            var transferlist = _repository.GetTransferList().Where(m => m.Status == "To Bill").ToList();

            if (SelectStatus == "Select Status")
            {
                if (SelectOwner == "All")
                {
                    var bargeslist = _repository.GetBargeList();
                    var viewmodelobj = (
                        from tr in transactionslist
                        join b in bargeslist on tr.Barge equals b.Barge_Name into bargeGroup
                        from b in bargeGroup.DefaultIfEmpty()
                        select new UPFleetViewModel()
                        {
                            Barge = b,
                            Transaction = tr,
                            TransferList = transferlist.Where(m => m.Transaction == tr.TransactionNo && (string.IsNullOrEmpty(fromDate) || m.From >= DateTime.Parse(fromDate)) && (string.IsNullOrEmpty(toDate) || m.To <= DateTime.Parse(toDate))).ToList()
                        }).ToList();

                    return Json(viewmodelobj,JsonRequestBehavior.AllowGet);
                }
                else
                {
                    // Filter by owner, date range
                    var bargeslist = _repository.GetBargeList().Where(m => m.Owner == SelectOwner && _repository.GetTransactionList().Any(tr => tr.Barge == m.Barge_Name));

                    var viewmodelobj = (
                        from tr in transactionslist
                        join b in bargeslist on tr.Barge equals b.Barge_Name
                        select new UPFleetViewModel()
                        {
                            Barge = b,
                            Transaction = tr,
                            TransferList = transferlist.Where(m => m.Transaction == tr.TransactionNo && (string.IsNullOrEmpty(fromDate) || m.From >= DateTime.Parse(fromDate)) && (string.IsNullOrEmpty(toDate) || m.To <= DateTime.Parse(toDate))).ToList()
                        }).ToList();

                    return Json(viewmodelobj,JsonRequestBehavior.AllowGet);
                }
            }
            else
            {
                if (SelectOwner == "All")
                {
                    var bargeslist = _repository.GetBargeList();
                    // Filter by status and date range if SelectOwner is "All"
                    var viewmodelobj = (
                        from tr in transactionslist.Where(m => m.Status == SelectStatus)
                        join b in bargeslist on tr.Barge equals b.Barge_Name into bargeGroup
                        from b in bargeGroup.DefaultIfEmpty()
                        where (string.IsNullOrEmpty(SelectStatus) || tr.Status == SelectStatus)
                        select new UPFleetViewModel()
                        {
                            Barge = b,
                            Transaction = tr,
                            TransferList = transferlist.Where(m => m.Transaction == tr.TransactionNo && (string.IsNullOrEmpty(fromDate) || m.From >= DateTime.Parse(fromDate)) && (string.IsNullOrEmpty(toDate) || m.To <= DateTime.Parse(toDate))).ToList()
                        }).ToList();

                    return Json(viewmodelobj,JsonRequestBehavior.AllowGet);
                }
                else
                {
                    // Filter by owner, status, and date range
                    var bargeslist = _repository.GetBargeList().Where(m => m.Owner == SelectOwner && _repository.GetTransactionList().Any(tr => tr.Barge == m.Barge_Name));

                    var viewmodelobj = (
                        from tr in transactionslist.Where(m => m.Status == SelectStatus)
                        join b in bargeslist on tr.Barge equals b.Barge_Name
                        where (string.IsNullOrEmpty(SelectStatus) || tr.Status == SelectStatus)
                        select new UPFleetViewModel()
                        {
                            Barge = b,
                            Transaction = tr,
                            TransferList = transferlist.Where(m => m.Transaction == tr.TransactionNo && (string.IsNullOrEmpty(fromDate) || m.From >= DateTime.Parse(fromDate)) && (string.IsNullOrEmpty(toDate) || m.To <= DateTime.Parse(toDate))).ToList()
                        }).ToList();

                    return Json(viewmodelobj,JsonRequestBehavior.AllowGet);
                }
            }
        }


        public ActionResult Not_Billed_TransferSummary_reportpage()
        {
            var Viewmodelobj = _repository.GetStatusData("NotBilled");
            return View(Viewmodelobj);
        }

        public ActionResult Billed_TransferSummary_reportpage()
        {
            var Viewmodelobj = _repository.GetStatusData("Billed");
            return View(Viewmodelobj);
        }
    }
}
