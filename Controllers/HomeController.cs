using FastReport;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Diagnostics;
using System.Globalization;
using System.IO.Compression;
using System.Net;
using System.Xml.Linq;
using transcript.Models;

namespace transcript.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class HomeController : Controller
    {
        private readonly DataBase _dataBase;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public HomeController(DataBase dataBase, IWebHostEnvironment webHostEnvironment)
        {
            _dataBase = dataBase;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IActionResult> Index()
        {
            var (studentCount, courseCount, gradeCount) = await _dataBase.GetStatsAsync();
            ViewBag.StudentCount = studentCount;
            ViewBag.CourseCount = courseCount;
            ViewBag.GradeCount = gradeCount;
            return View();
        }

        public async Task<IActionResult> Transcript()
        {
            DataTable dt = new DataTable();
            ViewBag.DT = await _dataBase.ShowStuAsync(dt);
            return View();
        }

        public async Task<IActionResult> generate(string stuno)
        {
            try
            {
                List<Courses> courses = await _dataBase.GetCourseAsync(stuno);
                List<Student> stu = await _dataBase.GetStudentAsync(stuno);
                byte[] pdf = GeneratePDF(stu, courses, stuno);

                if (pdf == null || pdf.Length == 0)
                {
                    Console.WriteLine("generate: PDF is null or empty");
                    return BadRequest("Failed to generate PDF");
                }

                return File(pdf, "application/pdf", DateTime.Now.ToString("yyyyMMdd_") + stuno + ".pdf");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"generate exception: {ex}");
                Console.WriteLine($"generate exception (stack): {ex.StackTrace}");
                return StatusCode(500, $"Error generating PDF: {ex.Message}");
            }
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        private byte[] GeneratePDF(List<Student> stu, List<Courses> courses, string stuno)
        {
            string ContentRootPath = _webHostEnvironment.ContentRootPath;
            List<Sign> sign = new List<Sign>();
            Report? rep = null;
            FastReport.Export.PdfSimple.PDFSimpleExport? pdfExport = null;

            try
            {
                Console.WriteLine("GeneratePDF: start");
                Console.WriteLine($"ContentRootPath: {ContentRootPath}");
                Console.WriteLine($"FastReport WebMode before set: {FastReport.Utils.Config.WebMode}");

                FastReport.Utils.Config.WebMode = true;
                Console.WriteLine($"FastReport WebMode after set: {FastReport.Utils.Config.WebMode}");

                sign.Add(new Sign(Path.Combine(ContentRootPath, "img")));
                Console.WriteLine($"Sign count: {sign.Count}");
                if (sign.Count > 0)
                {
                    Console.WriteLine($"SignReg length: {sign[0].SignReg?.Length}");
                    Console.WriteLine($"SignDean length: {sign[0].SignDean?.Length}");
                }

                rep = new Report();
                Console.WriteLine("Report: loading template");
                rep.Load(Path.Combine(ContentRootPath, "testc.frx"));
                Console.WriteLine("Report: loaded template");

                rep.RegisterData(stu, "StudentRef");
                rep.RegisterData(courses, "CoursesRef");
                rep.RegisterData(sign, "SignRef");
                Console.WriteLine("Report: data registered");

                Console.WriteLine("Report: preparing");
                bool prepared = false;
                try
                {
                    prepared = rep.Prepare();
                    Console.WriteLine($"Report: Prepare returned {prepared}");
                }
                catch (Exception prepEx)
                {
                    Console.WriteLine($"Report.Prepare threw: {prepEx}");
                    Console.WriteLine($"Report.Prepare stack trace: {prepEx.StackTrace}");
                    throw;
                }

                if (!prepared)
                {
                    Console.WriteLine("Report: Prepare returned false");
                    return Array.Empty<byte>();
                }

                pdfExport = new FastReport.Export.PdfSimple.PDFSimpleExport();
                pdfExport.ShowProgress = true;
                pdfExport.Subject = "Transcript";
                pdfExport.Title = stuno;
                pdfExport.Author = "YZU";

                using (var ms = new MemoryStream())
                {
                    Console.WriteLine("Report: starting Export");
                    rep.Export(pdfExport, ms);
                    Console.WriteLine("Report: Export completed");

                    var result = ms.ToArray();
                    Console.WriteLine($"Report: PDF size: {result.Length} bytes");

                    return result;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"GeneratePDF exception: {ex}");
                Console.WriteLine($"GeneratePDF exception type: {ex.GetType().Name}");
                Console.WriteLine($"GeneratePDF exception (stack): {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"GeneratePDF inner exception: {ex.InnerException}");
                    Console.WriteLine($"GeneratePDF inner exception stack: {ex.InnerException.StackTrace}");
                }

                return Array.Empty<byte>();
            }
            finally
            {
                try
                {
                    if (pdfExport != null)
                    {
                        Console.WriteLine("Report: disposing pdfExport");
                        pdfExport.Dispose();
                        Console.WriteLine("pdfExport disposed successfully");
                    }
                }
                catch (Exception d1)
                {
                    Console.WriteLine($"pdfExport.Dispose threw: {d1}");
                }

                try
                {
                    if (rep != null)
                    {
                        Console.WriteLine("Report: disposing rep");
                        rep.Dispose();
                        Console.WriteLine("rep disposed successfully");
                    }
                }
                catch (Exception d2)
                {
                    Console.WriteLine($"rep.Dispose threw: {d2}");
                }

                Console.WriteLine("GeneratePDF: finally block completed");
            }
        }
    }
}