using Microsoft.AspNetCore.Http;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Mime;
using System.Web;
using System.Web.Mvc;
using UPFleet.Models;
using UPFleet.Repositories;
using UPFleet.ViewModels;

namespace UPFleet.Controllers
{
    public class MaintenanceController : Controller
    {
        private readonly IRepository _repository;
        public MaintenanceController()
        {
            _repository = new Repository();
        }

        public ActionResult OwnerUpdate(int Id)
        {
            List<Owner> ownerList = _repository.GetOwnerList().ToList();

            int currentIndex = ownerList.FindIndex(m => m.ID == Id);

            if (currentIndex >= 0)
            {
                Owner obj = ownerList[currentIndex];
                int totalRecords = ownerList.Count;

                TempData["data"] = totalRecords;

                // Create an anonymous object that includes the currentIndex
                obj.CurrentIndex = currentIndex;

                return View(obj);
            }
            else
            {
                return RedirectToAction("OwnerUpdate", new { Id = Id + 1 });
            }
        }



        [HttpPost]
        public ActionResult OwnerUpdate(Owner model)
        {
            if (ModelState.IsValid)
            {
                if (_repository.GetOwnerList().Any(m => m.ID == model.ID && m.OwnerName == model.OwnerName))
                {
                    var result = _repository.UpdateOwner(model);
                    if (!result)
                    {
                        throw new Exception("Internal Issue Found. Try Again after some time.");
                    }
                    return RedirectToAction("OwnerUpdate", new { Id = model.ID });
                }
                else
                {
                    Owner obj = new Owner()
                    {
                        OwnerName = model.OwnerName,
                        Company = model.Company,
                        Account = model.Account
                    };
                    var result = _repository.AddOwner(obj);
                    if (!result)
                    {
                        throw new Exception("Internal Issue Found. Try Again after some time.");
                    }
                    int maxid = _repository.GetOwnerList().Max(m => m.ID);
                    return RedirectToAction("OwnerUpdate", new { Id = maxid });
                }
            }
            return RedirectToAction("OwnerUpdate", new { Id = model.ID });
        }

        public ActionResult AutocompleteBarge(string term)
        {
            List<string> suggestions = new List<string>();
            suggestions = _repository.GetBargeNameList(term);

            return Json(suggestions, JsonRequestBehavior.AllowGet);

        }
        public ActionResult AutocompleteOwner(string term)
        {
            List<string> suggestions = new List<string>();
            suggestions = _repository.GetOwnerList().Where(b => b.OwnerName.ToLower().Contains(term.ToLower())).Select(b => b?.OwnerName).ToList();

            return Json(suggestions, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CreateBarge()
        {
            var Ownerslist = _repository.GetOwnerList().OrderBy(m => m?.OwnerName).ToList();
            ViewBag.message = Ownerslist;
            return View();
        }
        [HttpPost]
        public ActionResult CreateBarge(Barge model)
        {
            if (model != null)
            {
                if (ModelState.IsValid)
                {
                    Barge obj = new Barge()
                    {
                        Barge_Name = model.Barge_Name,
                        Size = model.Size,
                        Description = model.Description,
                        Rate = model.Rate,
                        Owner = model.Owner
                    };
                    var result = _repository.AddBarge(obj);
                    if (!result)
                    {
                        throw new Exception("Internal Issue Found. Try Again after some time.");
                    }
                    return RedirectToAction("IndexPage", "Home", new { BargeName = model.Barge_Name });
                }
            }
            return View();
        }

        public ActionResult ExportBargesToExcel()
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;

            // Retrieve all Barge data from your data source (e.g., database)
            var barges = _repository.GetBargeList().OrderBy(m => m.Barge_Name).ToList();

            // Create an Excel package using EPPlus
            using (var package = new ExcelPackage())
            {
                var worksheet = package.Workbook.Worksheets.Add("Barges");

                var headers = typeof(Barge).GetProperties();
                for (int col = 1; col <= headers.Length - 2; col++)
                {
                    worksheet.Cells[1, col].Value = headers[col].Name;
                }

                // Add data
                for (int row = 2; row <= barges.Count() + 1; row++)
                {
                    for (int col = 1; col <= headers.Length - 2; col++)
                    {
                        worksheet.Cells[row, col].Value = headers[col].GetValue(barges[row - 2]);
                    }
                }
                // Save the Excel package to a stream
                using (var memoryStream = new MemoryStream())
                {
                    package.SaveAs(memoryStream);
                    var excelBytes = memoryStream.ToArray();

                    // Set response headers
                    var contentDisposition = new ContentDisposition
                    {
                        FileName = "Barges.xlsx",
                        Inline = false
                    };

                    Response.Headers.Add("Content-Disposition", contentDisposition.ToString());
                    Response.Headers.Add("Content-Type", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet");

                    // Return the Excel bytes as a FileContentResult
                    return new FileContentResult(excelBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet")
                    {
                        FileDownloadName = "Barges.xlsx"
                    };
                }
            }
        }
        [HttpPost]
        public ActionResult CheckDuplicateBarge(HttpPostedFileBase excelFile)
        {
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            if (excelFile != null && excelFile.ContentLength > 0)
            {
                try
                {
                    // Use a library like EPPlus or ClosedXML to read the Excel file
                    using (var package = new ExcelPackage(excelFile.InputStream))
                    {
                        var worksheet = package.Workbook.Worksheets[0];
                        var bargeNames = worksheet.Cells[2, 1, worksheet.Dimension.Rows, 1]
                                             .Select(cell => cell.Value?.ToString())
                                             .Where(name => !string.IsNullOrEmpty(name))
                                             .ToList();

                        var existingBargeNames = new HashSet<string>(_repository.GetBargeList().Select(b => b.Barge_Name));
                        var duplicates = bargeNames.Where(name => existingBargeNames.Contains(name)).ToList();

                        var totalnewbargescount = bargeNames.Count() - duplicates.Count();
                        if (duplicates.Count > 0)
                        {
                            return Json(new { isDuplicate = true, message = "Duplicate barge names found: " + string.Join(", ", duplicates), totalduplicatebarge = duplicates.Count(), totalnewbarges = totalnewbargescount }, JsonRequestBehavior.AllowGet);
                        }
                        else
                        {
                            return Json(new { isDuplicate = false }, JsonRequestBehavior.AllowGet);
                        }
                    }
                }
                catch (Exception)
                {
                    return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "An error occurred during duplicate check.");
                }
            }
            else
            {
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "An error occurred importing. Please try again later.");
            }
        }



        [HttpPost]
        public ActionResult ImportBargesFromExcel(IFormFile excelFile)
        {
            // Validate the uploaded file
            if (excelFile == null || excelFile.Length <= 0)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "No file uploaded.");
            }
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
            try
            {
                using (var stream = excelFile.OpenReadStream())
                {
                    // Use a library like EPPlus or ClosedXML to read the Excel file
                    using (var package = new ExcelPackage(stream))
                    {
                        var worksheet = package.Workbook.Worksheets[0];

                        // Assuming the data starts from row 2 (header in row 1)
                        for (int row = 2; row <= worksheet.Dimension.Rows; row++)
                        {
                            var bargeName = worksheet.Cells[row, 1].Value?.ToString();
                            if (string.IsNullOrWhiteSpace(bargeName) || bargeName == "")
                            {
                                // Handle balnk data type for barge column
                                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "No Barge Name found in row" + row);
                            }

                            // Check if a barge with the same name already exists
                            if (_repository.GetBargeList().Any(b => b.Barge_Name == bargeName))
                            {
                                // Show an alert or handle the duplicate case here
                                // Example: Return a message indicating the duplicate
                                continue;
                            }

                            // Extract other properties as before
                            var size = worksheet.Cells[row, 2].Value?.ToString();
                            if (string.IsNullOrWhiteSpace(size) || size == "")
                            {
                                // Handle balnk data type for barge column
                                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "No Size found in row" + row);
                            }
                            var rateValue = worksheet.Cells[row, 3].Value;
                            if (!double.TryParse(rateValue?.ToString(), out double rate))
                            {
                                // Handle incorrect data type for rate column
                                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, "Invalid data type for Rate in row" + row + ": Expected numeric value.");
                            }
                            var owner = worksheet.Cells[row, 4].Value?.ToString();
                            if (!_repository.GetOwnerList().Any(m => m?.OwnerName == owner))
                            {
                                return new HttpStatusCodeResult(HttpStatusCode.BadRequest, owner + " is unknown Owner in row " + row + ".");
                            }
                            var description = worksheet.Cells[row, 5].Value?.ToString();

                            // Perform database insert or update here
                            // Example using Entity Framework:
                            var barge = new Barge
                            {
                                Barge_Name = bargeName,
                                Size = size,
                                Rate = rate,
                                Owner = owner,
                                Description = description
                            };
                            var result = _repository.AddBarge(barge);
                        }
                        return new HttpStatusCodeResult(HttpStatusCode.OK, "Import successful.");
                    }
                }
            }
            catch (Exception)
            {
                // Handle exceptions, log errors, and return appropriate response
                return new HttpStatusCodeResult(HttpStatusCode.InternalServerError, "An error occurred during import.");
            }
        }

        public ActionResult CheckNextId(int id, string actionmessage)
        {
            var bargeList = _repository.GetBargeList();

            if (id != 0 && !string.IsNullOrEmpty(actionmessage))
            {
                int resultId = 0;

                if (actionmessage.ToLower() == "next")
                {
                    var nextId = bargeList.Where(m => m.ID > id).OrderBy(m => m.ID).Select(m => m.ID).FirstOrDefault();
                    resultId = nextId != 0 ? nextId : 0;
                }
                else
                {
                    var prevId = bargeList.Where(m => m.ID < id).OrderByDescending(m => m.ID).Select(m => m.ID).FirstOrDefault();
                    resultId = prevId != 0 ? prevId : 0;
                }

                if (resultId != 0)
                {
                    return Json(resultId, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(id, JsonRequestBehavior.AllowGet);
        }

        public ActionResult CheckNextownerId(int id, string actionmessage)
        {
            var ownerList = _repository.GetOwnerList();

            if (id != 0 && !string.IsNullOrEmpty(actionmessage))
            {
                int resultId = 0;

                if (actionmessage.ToLower() == "next")
                {
                    var nextId = ownerList.Where(m => m.ID > id).OrderBy(m => m.ID).Select(m => m.ID).FirstOrDefault();
                    resultId = nextId != 0 ? nextId : 0;
                }
                else
                {
                    var prevId = ownerList.Where(m => m.ID < id).OrderByDescending(m => m.ID).Select(m => m.ID).FirstOrDefault();
                    resultId = prevId != 0 ? prevId : 0;
                }

                if (resultId != 0)
                {
                    return Json(resultId, JsonRequestBehavior.AllowGet);
                }
            }

            return Json(id, JsonRequestBehavior.AllowGet);
        }




        public ActionResult BargeUpdate(int Id)
        {
            List<Barge> bargelist = _repository.GetBargeList().ToList();

            int currentIndex = bargelist.FindIndex(m => m.ID == Id);
            var Ownerslist = _repository.GetOwnerList().OrderBy(m => m.OwnerName).ToList();
            ViewBag.message = Ownerslist;
            if (currentIndex >= 0)
            {
                Barge obj = bargelist[currentIndex];
                int totalRecords = bargelist.Count;

                TempData["MaxID"] = totalRecords;

                // Create an anonymous object that includes the currentIndex
                obj.CurrentIndex = currentIndex;

                return View(obj);
            }
            else
            {
                return RedirectToAction("OwnerUpdate", new { Id = Id + 1 });
            }
        }

        [HttpPost]
        public ActionResult BargeUpdate(Barge model)
        {
            if (ModelState.IsValid)
            {
                var data = _repository.GetBargeList().FirstOrDefault(m => m.Barge_Name == model.Barge_Name);
                if (data != null)
                {
                    var result = _repository.UpdateBarge(model);
                    if (!result)
                    {
                        throw new Exception("Internal Issue Found. Try Again after some time.");
                    }
                    return RedirectToAction("BargeUpdate", new { Id = data.ID });
                }
                else
                {
                    Barge obj = new Barge()
                    {
                        Barge_Name = model.Barge_Name,
                        Size = model.Size,
                        Description = model.Description,
                        Rate = model.Rate,
                        Owner = model.Owner
                    };
                    var result = _repository.AddBarge(obj);
                    if (!result)
                    {
                        throw new Exception("Internal Issue Found. Try Again after some time.");
                    }
                    int maxid = _repository.GetBargeList().Max(m => m.ID);
                    return RedirectToAction("BargeUpdate", new { Id = maxid });
                }
            }

            return RedirectToAction("BargeUpdate", new { Id = model.ID });
        }

        public ActionResult GoBack()
        {
            var bargename = TempData["BargeName"]?.ToString();
            TempData.Keep("BargeName");
            TempData.Keep("tranactionNo");
            return RedirectToAction("IndexPage", "Home", new { BargeName = bargename });
        }
        public ActionResult SaveTransfers()
        {
            return View();
        }

        //Saving a new/old transfers
        [HttpPost]
        public ActionResult SaveTransfers(List<UPFleetViewModel> transferlist)
        {
            var transaction = TempData["tranactionNo"]?.ToString();
            TempData.Keep("BargeName");
            TempData.Keep("tranactionNo");

            foreach (var transfer in transferlist)
            {
                if (transfer != null)
                {
                    //New Transfer..
                    if (transfer.Transfer?.ID == 0)
                    {
                        var result = _repository.AddTransfer(transfer.Transfer, transaction);
                        if (!result)
                        {
                            throw new Exception("Internal Issue Found. Try Again after some time.");
                        }
                    }
                    //Updating Transfer..
                    else
                    {
                        var result = _repository.UpdateTransfer(transfer.Transfer);
                        if (!result)
                        {
                            throw new Exception("Internal Issue Found. Try Again after some time.");
                        }

                    }
                }
            }

            return RedirectToAction("IndexPage", "Home", new { Transactionno = transaction });
        }

        //Gettng Barge details and adding a new transaction by clicking on Add new Tranaction
        [HttpGet]
        public ActionResult GetBargeDetails(string barge, string status)
        {
            var bargeDetails = _repository.GetBargeList().FirstOrDefault(b => b.Barge_Name == barge);

            if (bargeDetails == null)
            {
                return HttpNotFound();
            }

            var count = _repository.GetTransactionList().Count(m => m.Barge == barge && _repository.GetTransferList().Any(tr => tr.Transaction == m.TransactionNo));
            var TransId = _repository.GetTransactionCount() + 1;
            var response = new
            {
                Rate = bargeDetails.Rate,
                Owner = bargeDetails.Owner,
                Transaction = TransId,
                record = count + 1
            };

            // Save Transaction
            Transaction data = new Transaction()
            {
                TransactionNo = TransId,
                Rate = (double)response.Rate,
                Barge = barge,
                Status = status
            };
            var result = _repository.AddTransaction(data);
            TempData["tranactionNo"] = TransId.ToString();
            TempData["BargeName"] = barge;
            return Json(response, JsonRequestBehavior.AllowGet);
        }

        //Getting data of Barge/Owner for BargeUpdate/OwnerUpdate page after filling a Barge name or Owner Name..
        [HttpGet]
        public ActionResult GetDetails(string barge = null, string owner = null)
        {
            if (barge != null)
            {
                var bargeDetails = _repository.GetBargeList().FirstOrDefault(b => b.Barge_Name?.ToLower() == barge.ToLower());
                if (bargeDetails != null)
                {
                    var response = new
                    {
                        bargeid = bargeDetails.ID
                    };
                    return Json(response, JsonRequestBehavior.AllowGet);
                }
            }
            else if (owner != null)
            {
                var ownerdetails = _repository.GetOwnerList().FirstOrDefault(b => b.OwnerName?.ToLower() == owner.ToLower());
                if (ownerdetails != null)
                {
                    var response = new
                    {
                        ownerid = ownerdetails.ID
                    };
                    return Json(response, JsonRequestBehavior.AllowGet);
                }
            }
            return Json(null);
        }

        //Updating Transaction deatils on clicking Update transaction button..
        [HttpGet]
        public ActionResult Update_transaction(double? transactionInput, string status, double Rate)
        {

            var result = _repository.UpdateTransaction(transactionInput, status, Rate);
            if (_repository.GetTransferList().Any(m => m.Transaction == transactionInput))
            {
                var response = new
                {
                    currentTransactionType = "Update"
                };
                return Json(response, JsonRequestBehavior.AllowGet);
            }
            else
            {
                var response = new
                {
                    currentTransactionType = "New"
                };
                return Json(response, JsonRequestBehavior.AllowGet);
            }
        }

        //Deleting a transaction and all transfer realated to that transaction... 
        [HttpGet]
        public ActionResult Delete_transaction(double? transactionInput)
        {
            var result = _repository.DeleteTransaction(transactionInput);
            var response = "Data saved Successfully.";
            return Json(response, JsonRequestBehavior.AllowGet);

        }
    }
}
