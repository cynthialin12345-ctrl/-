using Microsoft.Data.SqlClient;
using System.Globalization;
using System.Xml.Linq;
using System.Data;
using System.Diagnostics;
using Microsoft.Extensions.Configuration;

namespace transcript.Models
{
    public class regsem
    {
        public int year { get; set; }
        public int sem { get; set; }
    }

    public class stu
    {
        public int stuno { get; set; }
        public int deptno { get; set; }
        public string? name { get; set; }
        public int entryYear { get; set; }
        public string? gradDate { get; set; }
    }

    public class crs
    {
        public int cono { get; set; }
        public string? chinese_co { get; set; }
    }

    public class stu_crs
    {
        public int stuno { get; set; }
        public int year { get; set; }
        public int semester { get; set; }
        public string? stu_course_no { get; set; }
        public decimal stu_course_score { get; set; }
        public string? chinese_co { get; set; }
    }

    public class User
    {
        public string Username { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
    }

    public class Student
    {
        public string Name { get; set; } = string.Empty;
        public string Enrolled { get; set; } = string.Empty;
        public string DegreeConferred { get; set; } = string.Empty;
        public string DateConferred { get; set; } = string.Empty;
        public string Department { get; set; } = string.Empty;
        public string College { get; set; } = string.Empty;
        public string Issued { get; set; } = string.Empty;
        public decimal Score { get; set; }
        public decimal Credits { get; set; }
        public decimal GPA { get; set; }
    }

    public class Courses
    {
        public string Course { get; set; } = string.Empty;
        public decimal Credit { get; set; }
        public string Grade { get; set; } = string.Empty;
        public int DataYear { get; set; }
        public int DataSemester { get; set; }
        public decimal TotalCredit { get; set; }
        public decimal Average { get; set; }
    }

    public class Sign
    {
        public byte[] SignReg { get; set; }
        public byte[] SignDean { get; set; }
        public Sign(string path)
        {
            var regPath = Path.Combine(path ?? string.Empty, "signreg.png");
            var deanPath = Path.Combine(path ?? string.Empty, "signdean.png");

            SignReg = File.Exists(regPath) ? File.ReadAllBytes(regPath) : Array.Empty<byte>();
            SignDean = File.Exists(deanPath) ? File.ReadAllBytes(deanPath) : Array.Empty<byte>();
        }
    }

    public class DataBase
    {
        private readonly string _connectionString;
        private const int TAIWAN_YEAR_OFFSET = 1911;

        public DataBase(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") 
                ?? throw new InvalidOperationException("DefaultConnection is not configured.");
        }

        public async Task<DataTable> ShowStuAsync(DataTable dt)
        {
            if (dt == null)
            {
                dt = new DataTable();
            }
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand("SELECT * FROM Tablestufiles1132013", conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    dt.Load(reader);
                }
            }
            return dt;
        }

        public async Task<DataTable> ShowCrsAsync(DataTable dt)
        {
            if (dt == null)
            {
                dt = new DataTable();
            }
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand("SELECT * FROM Tablecourses1132013", conn))
                using (var reader = await cmd.ExecuteReaderAsync())
                {
                    dt.Load(reader);
                }
            }
            return dt;
        }

        public async Task SetStuAsync(stu student)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand("INSERT INTO Tablestufiles1132013(Stuno, Deptno, Name, EntryYear, GradDate) VALUES (@stuno, @deptno, @name, @entryYear, @gradDate)", conn))
                {
                    cmd.Parameters.AddWithValue("@stuno", student.stuno);
                    cmd.Parameters.AddWithValue("@deptno", student.deptno);
                    cmd.Parameters.AddWithValue("@name", student.name ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@entryYear", student.entryYear);
                    cmd.Parameters.AddWithValue("@gradDate", student.gradDate ?? (object)DBNull.Value);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task SetCrsAsync(stu_crs crs)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand("INSERT INTO Tablestu_courses1132013(Stuno, Year, Semester, Stu_course_no, Stu_course_score) VALUES (@stuno, @year, @semester, @stu_course_no, @stu_course_score)", conn))
                {
                    cmd.Parameters.AddWithValue("@stuno", crs.stuno);
                    cmd.Parameters.AddWithValue("@year", crs.year);
                    cmd.Parameters.AddWithValue("@semester", crs.semester);
                    cmd.Parameters.AddWithValue("@stu_course_no", crs.stu_course_no ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@stu_course_score", crs.stu_course_score);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task<List<stu_crs>> GetStuCrsAsync(int studentId)
        {
            List<stu_crs> crs = new List<stu_crs>();
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand("SELECT Year, Semester, Stu_course_no, Stu_course_score, Chinese_co FROM Tablestu_courses1132013 LEFT JOIN Tablecourses1132013 ON Tablestu_courses1132013.Stu_course_no = Tablecourses1132013.Cono WHERE Stuno = @StudentId", conn))
                {
                    cmd.Parameters.AddWithValue("@StudentId", studentId);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            crs.Add(new stu_crs()
                            {
                                stuno = studentId,
                                year = reader.GetInt32(0),
                                semester = reader.GetInt32(1),
                                stu_course_no = reader.IsDBNull(2) ? null : reader.GetString(2),
                                stu_course_score = reader.GetDecimal(3),
                                chinese_co = reader.IsDBNull(4) ? null : reader.GetString(4),
                            });
                        }
                    }
                }
            }
            return crs;
        }

        public async Task DelStuCrsAsync(stu_crs crs)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand("DELETE FROM Tablestu_courses1132013 WHERE Tablestu_courses1132013.stuno = @stuno AND Tablestu_courses1132013.year = @year AND Tablestu_courses1132013.semester = @semester AND Tablestu_courses1132013.stu_course_no = @stu_course_no", conn))
                {
                    cmd.Parameters.AddWithValue("@stuno", crs.stuno);
                    cmd.Parameters.AddWithValue("@year", crs.year);
                    cmd.Parameters.AddWithValue("@semester", crs.semester);
                    cmd.Parameters.AddWithValue("@stu_course_no", crs.stu_course_no ?? (object)DBNull.Value);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task CalculateSemesterAsync(string studentId, int year, int sem)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                decimal score_sum = 0;
                decimal obtain_credit = 0;
                decimal acc_credit = 0;
                bool exist = false;

                using (var cmd = new SqlCommand("SELECT stu_course_score, credit FROM Tablestu_courses1132013 LEFT JOIN Tablecourses1132013 ON Tablestu_courses1132013.stu_course_no = Tablecourses1132013.cono WHERE stuno = @StudentId AND year = @year AND semester = @semester", conn))
                {
                    cmd.Parameters.AddWithValue("@StudentId", studentId);
                    cmd.Parameters.AddWithValue("@year", year);
                    cmd.Parameters.AddWithValue("@semester", sem);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            decimal score = reader.GetDecimal(0);
                            decimal credit = reader.GetInt32(1);
                            score_sum += score * credit;
                            obtain_credit += credit;
                            if (score >= 60)
                                acc_credit += credit;
                        }
                    }
                }

                using (var cmd = new SqlCommand("SELECT 1 FROM Tablesemesters1132013 WHERE stuno = @StudentId AND year = @year AND semester = @semester", conn))
                {
                    cmd.Parameters.AddWithValue("@StudentId", studentId);
                    cmd.Parameters.AddWithValue("@year", year);
                    cmd.Parameters.AddWithValue("@semester", sem);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                            exist = true;
                    }
                }

                decimal scoreSemesterAvg = obtain_credit > 0 ? score_sum / obtain_credit : 0m;

                if (!exist)
                {
                    using (var cmd = new SqlCommand("INSERT INTO Tablesemesters1132013(stuno, year, semester, score_semester_sum, score_semester_avg, accumulate_credit, obtain_credit) VALUES (@stuno, @year, @semester, @score_semester_sum, @score_semester_avg, @accumulate_credit, @obtain_credit)", conn))
                    {
                        cmd.Parameters.AddWithValue("@stuno", studentId);
                        cmd.Parameters.AddWithValue("@year", year);
                        cmd.Parameters.AddWithValue("@semester", sem);
                        cmd.Parameters.AddWithValue("@score_semester_sum", score_sum);
                        cmd.Parameters.AddWithValue("@score_semester_avg", scoreSemesterAvg);
                        cmd.Parameters.AddWithValue("@accumulate_credit", acc_credit);
                        cmd.Parameters.AddWithValue("@obtain_credit", obtain_credit);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                else
                {
                    using (var cmd = new SqlCommand("UPDATE Tablesemesters1132013 SET score_semester_sum = @score_semester_sum, score_semester_avg = @score_semester_avg, accumulate_credit = @accumulate_credit, obtain_credit = @obtain_credit WHERE stuno = @StudentId AND year = @year AND semester = @semester", conn))
                    {
                        cmd.Parameters.AddWithValue("@score_semester_sum", score_sum);
                        cmd.Parameters.AddWithValue("@score_semester_avg", scoreSemesterAvg);
                        cmd.Parameters.AddWithValue("@accumulate_credit", acc_credit);
                        cmd.Parameters.AddWithValue("@obtain_credit", obtain_credit);
                        cmd.Parameters.AddWithValue("@StudentId", studentId);
                        cmd.Parameters.AddWithValue("@year", year);
                        cmd.Parameters.AddWithValue("@semester", sem);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
        }

        public async Task CalculateAccumulateAsync(string studentId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                decimal score_sum = 0;
                decimal obtain_credit = 0;
                decimal acc_credit = 0;
                decimal GPA = 0;
                bool exist = false;

                using (var cmd = new SqlCommand("SELECT stu_course_score, credit FROM Tablestu_courses1132013 LEFT JOIN Tablecourses1132013 ON Tablestu_courses1132013.stu_course_no = Tablecourses1132013.cono WHERE stuno = @StudentId", conn))
                {
                    cmd.Parameters.AddWithValue("@StudentId", studentId);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            decimal score = reader.GetDecimal(0);
                            decimal credit = reader.GetInt32(1);
                            score_sum += score * credit;
                            obtain_credit += credit;
                            if (score >= 60)
                                acc_credit += credit;

                            if (score >= 80)
                                GPA += 4 * credit;
                            else if (score >= 70)
                                GPA += 3 * credit;
                            else if (score >= 60)
                                GPA += 2 * credit;
                            else if (score >= 50)
                                GPA += 1 * credit;
                        }
                    }
                }

                using (var cmd = new SqlCommand("SELECT 1 FROM Tableaccumulate_infos1132013 WHERE stuno = @stuno", conn))
                {
                    cmd.Parameters.AddWithValue("@stuno", studentId);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                            exist = true;
                    }
                }

                decimal scoreAvg = obtain_credit > 0 ? score_sum / obtain_credit : 0m;
                decimal gpaAvg = obtain_credit > 0 ? GPA / obtain_credit : 0m;

                if (!exist)
                {
                    using (var cmd = new SqlCommand("INSERT INTO Tableaccumulate_infos1132013(stuno, score_sum, accumulate_credit, obtain_credit, score_avg, gpa_avg) VALUES (@stuno, @score_sum, @accumulate_credit, @obtain_credit, @score_avg, @gpa_avg)", conn))
                    {
                        cmd.Parameters.AddWithValue("@stuno", studentId);
                        cmd.Parameters.AddWithValue("@score_sum", score_sum);
                        cmd.Parameters.AddWithValue("@accumulate_credit", acc_credit);
                        cmd.Parameters.AddWithValue("@obtain_credit", obtain_credit);
                        cmd.Parameters.AddWithValue("@score_avg", scoreAvg);
                        cmd.Parameters.AddWithValue("@gpa_avg", gpaAvg);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
                else
                {
                    using (var cmd = new SqlCommand("UPDATE Tableaccumulate_infos1132013 SET score_sum = @score_sum, accumulate_credit = @acc_credit, obtain_credit = @obtain_credit, score_avg = @score_avg, gpa_avg = @gpa_avg WHERE stuno = @stuno", conn))
                    {
                        cmd.Parameters.AddWithValue("@score_sum", score_sum);
                        cmd.Parameters.AddWithValue("@acc_credit", acc_credit);
                        cmd.Parameters.AddWithValue("@obtain_credit", obtain_credit);
                        cmd.Parameters.AddWithValue("@score_avg", scoreAvg);
                        cmd.Parameters.AddWithValue("@gpa_avg", gpaAvg);
                        cmd.Parameters.AddWithValue("@stuno", studentId);
                        await cmd.ExecuteNonQueryAsync();
                    }
                }
            }
        }

        public async Task<List<Courses>> GetCourseAsync(string studentId)
        {
            List<Courses> crs = new List<Courses>();
            List<regsem> reg = new List<regsem>();

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand("SELECT DISTINCT year, semester FROM Tablestu_courses1132013 WHERE stuno = @stuno", conn))
                {
                    cmd.Parameters.AddWithValue("@stuno", studentId);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            reg.Add(new regsem()
                            {
                                year = reader.GetInt32(0),
                                sem = reader.GetInt32(1),
                            });
                        }
                    }
                }

                if (reg.Count == 0)
                {
                    return crs;
                }

                foreach (var item in reg)
                {
                    await CalculateSemesterAsync(studentId, item.year, item.sem);
                }
                await CalculateAccumulateAsync(studentId);

                var last = reg.Last();
                using (var cmd = new SqlCommand(
                    "SELECT chinese_co, credit, stu_course_score, Tablestu_courses1132013.year, Tablestu_courses1132013.semester, accumulate_credit, score_semester_avg " +
                    "FROM Tablestu_courses1132013 " +
                    "LEFT JOIN Tablecourses1132013 ON Tablestu_courses1132013.stu_course_no = Tablecourses1132013.cono " +
                    "LEFT JOIN Tablesemesters1132013 ON Tablestu_courses1132013.year = Tablesemesters1132013.year AND Tablestu_courses1132013.semester = Tablesemesters1132013.semester AND Tablestu_courses1132013.stuno = Tablesemesters1132013.stuno " +
                    "WHERE Tablestu_courses1132013.stuno = @stuno AND ((Tablestu_courses1132013.year < @ly) OR (Tablestu_courses1132013.year = @ly AND Tablestu_courses1132013.semester <= @ls)) " +
                    "ORDER BY Tablestu_courses1132013.year, Tablestu_courses1132013.semester", conn))
                {
                    cmd.Parameters.AddWithValue("@stuno", studentId);
                    cmd.Parameters.AddWithValue("@ly", last.year);
                    cmd.Parameters.AddWithValue("@ls", last.sem);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            crs.Add(new Courses()
                            {
                                Course = reader.IsDBNull(0) ? string.Empty : reader.GetString(0),
                                Credit = reader.GetInt32(1),
                                Grade = reader.GetDecimal(2) < 60 ? "*" + reader.GetDecimal(2).ToString() : reader.GetDecimal(2).ToString(),
                                DataYear = reader.GetInt32(3),
                                DataSemester = reader.GetInt32(4),
                                TotalCredit = reader.GetDecimal(5),
                                Average = reader.GetDecimal(6),
                            });
                        }
                    }
                }
            }
            return crs;
        }

        public async Task<List<Student>> GetStudentAsync(string studentId)
        {
            List<Student> stu = new List<Student>();
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand("SELECT name, entryYear, degreeName, gradDate, deptName, collegeName, score_avg, accumulate_credit, gpa_avg FROM Tablestufiles1132013 LEFT JOIN Tablepubdeps1132013 ON Tablestufiles1132013.deptno = Tablepubdeps1132013.deptno LEFT JOIN Tableaccumulate_infos1132013 ON Tablestufiles1132013.stuno = Tableaccumulate_infos1132013.stuno WHERE Tablestufiles1132013.stuno = @StudentId", conn))
                {
                    cmd.Parameters.AddWithValue("@StudentId", studentId);
                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        while (await reader.ReadAsync())
                        {
                            stu.Add(new Student()
                            {
                                Name = reader.IsDBNull(0) ? string.Empty : reader.GetString(0),
                                Enrolled = reader.IsDBNull(1) ? string.Empty : (reader.GetInt32(1) + TAIWAN_YEAR_OFFSET).ToString(),
                                DegreeConferred = reader.IsDBNull(2) ? string.Empty : reader.GetString(2),
                                DateConferred = reader.IsDBNull(3) ? "blank" : (int.TryParse(reader.GetString(3), out int yr) ? (yr + TAIWAN_YEAR_OFFSET).ToString() : reader.GetString(3)),
                                Department = reader.IsDBNull(4) ? string.Empty : reader.GetString(4),
                                College = reader.IsDBNull(5) ? string.Empty : reader.GetString(5),
                                Issued = DateTime.Now.ToString("MMMM dd, yyyy", new CultureInfo("en-us")),
                                Score = reader.IsDBNull(6) ? 0m : reader.GetDecimal(6),
                                Credits = reader.IsDBNull(7) ? 0m : reader.GetDecimal(7),
                                GPA = reader.IsDBNull(8) ? 0m : reader.GetDecimal(8)
                            });
                        }
                    }
                }
            }
            return stu;
        }

        public async Task<User?> ValidateUserAsync(string username, string password)
        {
            if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
                return null;

            string passwordHash;
            using (var sha256 = System.Security.Cryptography.SHA256.Create())
            {
                byte[] bytes = sha256.ComputeHash(System.Text.Encoding.UTF8.GetBytes(password));
                var sb = new System.Text.StringBuilder();
                foreach (byte b in bytes)
                {
                    sb.Append(b.ToString("x2"));
                }
                passwordHash = sb.ToString();
            }

            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand("SELECT username, name, role FROM Tableuserss1132013 WHERE username = @username AND password_hash = @password_hash", conn))
                {
                    cmd.Parameters.AddWithValue("@username", username);
                    cmd.Parameters.AddWithValue("@password_hash", passwordHash);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        if (await reader.ReadAsync())
                        {
                            return new User()
                            {
                                Username = reader.GetString(0),
                                Name = reader.GetString(1),
                                Role = reader.GetString(2)
                            };
                        }
                    }
                }
            }
            return null;
        }

        public async Task<DataTable> GetPagedStudentsAsync(string? searchTerm, int pageNumber, int pageSize)
        {
            var dt = new DataTable();
            int offset = (pageNumber - 1) * pageSize;
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string sql = "SELECT * FROM Tablestufiles1132013";
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    sql += " WHERE name LIKE @Search OR CAST(stuno AS VARCHAR) LIKE @Search";
                }
                sql += " ORDER BY stuno OFFSET @Offset ROWS FETCH NEXT @PageSize ROWS ONLY";

                using (var cmd = new SqlCommand(sql, conn))
                {
                    if (!string.IsNullOrWhiteSpace(searchTerm))
                    {
                        cmd.Parameters.AddWithValue("@Search", "%" + searchTerm + "%");
                    }
                    cmd.Parameters.AddWithValue("@Offset", offset);
                    cmd.Parameters.AddWithValue("@PageSize", pageSize);

                    using (var reader = await cmd.ExecuteReaderAsync())
                    {
                        dt.Load(reader);
                    }
                }
            }
            return dt;
        }

        public async Task<int> GetStudentsCountAsync(string? searchTerm)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                string sql = "SELECT COUNT(*) FROM Tablestufiles1132013";
                if (!string.IsNullOrWhiteSpace(searchTerm))
                {
                    sql += " WHERE name LIKE @Search OR CAST(stuno AS VARCHAR) LIKE @Search";
                }

                using (var cmd = new SqlCommand(sql, conn))
                {
                    if (!string.IsNullOrWhiteSpace(searchTerm))
                    {
                        cmd.Parameters.AddWithValue("@Search", "%" + searchTerm + "%");
                    }
                    var result = await cmd.ExecuteScalarAsync();
                    return Convert.ToInt32(result);
                }
            }
        }

        public async Task UpdateStuAsync(stu student)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand("UPDATE Tablestufiles1132013 SET deptno = @deptno, name = @name, entryYear = @entryYear, gradDate = @gradDate WHERE stuno = @stuno", conn))
                {
                    cmd.Parameters.AddWithValue("@stuno", student.stuno);
                    cmd.Parameters.AddWithValue("@deptno", student.deptno);
                    cmd.Parameters.AddWithValue("@name", student.name ?? (object)DBNull.Value);
                    cmd.Parameters.AddWithValue("@entryYear", student.entryYear);
                    cmd.Parameters.AddWithValue("@gradDate", student.gradDate ?? (object)DBNull.Value);
                    await cmd.ExecuteNonQueryAsync();
                }
            }
        }

        public async Task DelStuAsync(int studentId)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var tx = conn.BeginTransaction())
                {
                    try
                    {
                        // 1. Delete from Tablestu_courses1132013
                        using (var cmd = new SqlCommand("DELETE FROM Tablestu_courses1132013 WHERE stuno = @stuno", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@stuno", studentId);
                            await cmd.ExecuteNonQueryAsync();
                        }

                        // 2. Delete from Tablesemesters1132013
                        using (var cmd = new SqlCommand("DELETE FROM Tablesemesters1132013 WHERE stuno = @stuno", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@stuno", studentId);
                            await cmd.ExecuteNonQueryAsync();
                        }

                        // 3. Delete from Tableaccumulate_infos1132013
                        using (var cmd = new SqlCommand("DELETE FROM Tableaccumulate_infos1132013 WHERE stuno = @stuno", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@stuno", studentId);
                            await cmd.ExecuteNonQueryAsync();
                        }

                        // 4. Delete from Tablestufiles1132013
                        using (var cmd = new SqlCommand("DELETE FROM Tablestufiles1132013 WHERE stuno = @stuno", conn, tx))
                        {
                            cmd.Parameters.AddWithValue("@stuno", studentId);
                            await cmd.ExecuteNonQueryAsync();
                        }

                        await tx.CommitAsync();
                    }
                    catch
                    {
                        await tx.RollbackAsync();
                        throw;
                    }
                }
            }
        }

        public async Task<(int StudentCount, int CourseCount, int GradeCount)> GetStatsAsync()
        {
            int studentCount = 0;
            int courseCount = 0;
            int gradeCount = 0;
            using (var conn = new SqlConnection(_connectionString))
            {
                await conn.OpenAsync();
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Tablestufiles1132013", conn))
                {
                    studentCount = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Tablecourses1132013", conn))
                {
                    courseCount = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }
                using (var cmd = new SqlCommand("SELECT COUNT(*) FROM Tablestu_courses1132013", conn))
                {
                    gradeCount = Convert.ToInt32(await cmd.ExecuteScalarAsync());
                }
            }
            return (studentCount, courseCount, gradeCount);
        }
    }
}
