using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Dynamic;
using transcript.Models;

namespace transcript.Controllers
{
    [Microsoft.AspNetCore.Authorization.Authorize]
    public class InsertController : Controller
    {
        private readonly DataBase _dataBase;

        public InsertController(DataBase dataBase)
        {
            _dataBase = dataBase;
        }

        public async Task<IActionResult> student(stu Student)
        {
            await _dataBase.SetStuAsync(Student);
            return RedirectToAction("Addstudent");
        }

        public async Task<IActionResult> course(stu_crs Crs)
        {
            await _dataBase.SetCrsAsync(Crs);
            return RedirectToAction("Addcourse");
        }

        public async Task<IActionResult> Addstudent(string? searchTerm, int page = 1)
        {
            int pageSize = 5;
            DataTable students = await _dataBase.GetPagedStudentsAsync(searchTerm, page, pageSize);
            int totalStudents = await _dataBase.GetStudentsCountAsync(searchTerm);
            int totalPages = (int)Math.Ceiling((double)totalStudents / pageSize);

            ViewBag.StudentsList = students;
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = totalPages == 0 ? 1 : totalPages;
            ViewBag.SearchTerm = searchTerm;

            return View("Addstudent");
        }

        [HttpPost]
        public async Task<IActionResult> UpdateStudent(stu Student)
        {
            await _dataBase.UpdateStuAsync(Student);
            return RedirectToAction("Addstudent");
        }

        [HttpPost]
        public async Task<IActionResult> DeleteStudent(int stuno)
        {
            await _dataBase.DelStuAsync(stuno);
            return RedirectToAction("Addstudent");
        }

        public async Task<IActionResult> Addcourse() 
        {
            DataTable dt1 = new DataTable();
            DataTable dt2 = new DataTable();
            ViewBag.DT1 = await _dataBase.ShowStuAsync(dt1);
            ViewBag.DT2 = await _dataBase.ShowCrsAsync(dt2);
            return View("Addcourse");
        }

        [HttpPost]
        public async Task<JsonResult> GetStuCrs([FromBody]int stuno)
        {
            List<stu_crs> crs = await _dataBase.GetStuCrsAsync(stuno);
            Debug.WriteLine(crs);
            return Json(crs);
        }

        [HttpPut]
        public async Task<IActionResult> DelStuCrs([FromBody] stu_crs crs)
        {
            Debug.WriteLine(crs.stu_course_no);
            await _dataBase.DelStuCrsAsync(crs);
            return Ok();
        }
    }
}
