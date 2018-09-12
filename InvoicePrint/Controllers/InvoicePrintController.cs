using System;
using ContractFeatures.Models;
using InvoicePrint.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using SelectPdf;

namespace InvoicePrint.Controllers
{
    public class InvoicePrintController : Controller
    {
       
        public ActionResult Index()
        {
            var invoiceData = new Invoice()
            {
                BillToName = "Donald Duck",
                BilltoAddress = "123 Main St.",
                InvoiceAmount = (decimal) 23.00,
                InvoiceDue = DateTime.Now.AddDays(10)


            };
            return View(invoiceData);
        }

        //// GET: InvoiceReport/Details/5
        public ActionResult Details(int id)
        {
            var invoiceData = new Invoice()
            {
                BillToName = "Donald Duck",
                BilltoAddress = "123 Main St.",
                InvoiceAmount = (decimal)23.00,
                InvoiceDue = DateTime.Now.AddDays(10)


            };
            return View(invoiceData);
        }

        public ActionResult Edit()
        {
            var invoiceData = new Invoice()
            {
                BillToName = "Donald Duck",
                BilltoAddress = "123 Main St.",
                InvoiceAmount = (decimal)23.00,
                InvoiceDue = DateTime.Now.AddDays(10)


            };
            return View(invoiceData);
        }

        // POST: InvoiceReport/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                // TODO: Add update logic here
                HtmlToPdf converter = new HtmlToPdf();

                PdfDocument doc = converter.ConvertUrl("http://localhost:11908/InvoicePrint/Details");

                doc.Save(@"d:\temp\Sample1.pdf");

                doc.Close();

                // put in test code here
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }





    }
}