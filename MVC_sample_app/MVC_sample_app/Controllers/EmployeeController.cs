using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using MVC_sample_app.Data;
using MVC_sample_app.Models.Domain;
using MVC_sample_app.Models.ViewModels;
using System.Data;

public class EmployeeController : Controller
{
    private readonly ApplicationDbContext _context;

    public EmployeeController(ApplicationDbContext context)
    {
        _context = context;
    }

    // Display all employees
    public async Task<IActionResult> Index()
    {
        var employees = new List<Employee>();

        // Use ExecuteSqlRawAsync to manually handle stored procedure results
        var result = await _context.Database.ExecuteSqlRawAsync("EXEC spGetAllEmployees");

        // Use ADO.NET to read the results manually if needed
        using (var command = _context.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = "EXEC spGetAllEmployees";
            command.CommandType = System.Data.CommandType.Text;

            _context.Database.OpenConnection();

            using (var resultSet = await command.ExecuteReaderAsync())
            {
                while (await resultSet.ReadAsync())
                {
                    employees.Add(new Employee
                    {
                        ID = resultSet.GetInt32(resultSet.GetOrdinal("ID")),
                        Name = resultSet.GetString(resultSet.GetOrdinal("Name")),
                        DateOfBirth = resultSet.GetDateTime(resultSet.GetOrdinal("DateOfBirth")),
                        Email = resultSet.GetString(resultSet.GetOrdinal("Email")),
                        Picture = resultSet.IsDBNull(resultSet.GetOrdinal("Picture")) ? null : (byte[])resultSet["Picture"]
                    });
                }
            }
        }

        var employeeViewModels = employees.Select(e => new EmployeeViewModel
        {
            ID = e.ID,
            Name = e.Name,
            DateOfBirth = e.DateOfBirth,
            Email = e.Email,
            Picture = e.Picture
        }).ToList();

        return View(employeeViewModels);
    }

    // Add a new employee
    [HttpPost]
    public async Task<IActionResult> Add(EmployeeViewModel model)
    {
        byte[] pictureData = null;

        if (model.PictureFile != null)
        {
            using (var memoryStream = new MemoryStream())
            {
                await model.PictureFile.CopyToAsync(memoryStream);
                pictureData = memoryStream.ToArray();
            }
        }

        var parameters = new[]
        {
            new SqlParameter("@Name", System.Data.SqlDbType.NVarChar) { Value = model.Name },
            new SqlParameter("@DateOfBirth", System.Data.SqlDbType.Date) { Value = model.DateOfBirth },
            new SqlParameter("@Email", System.Data.SqlDbType.NVarChar) { Value = model.Email },
            new SqlParameter("@Picture", System.Data.SqlDbType.VarBinary) { Value = (object)pictureData ?? DBNull.Value }
        };

        await _context.Database.ExecuteSqlRawAsync("EXEC spAddEmployee @Name, @DateOfBirth, @Email, @Picture", parameters);
        return RedirectToAction(nameof(Index));
    }

    // Edit an existing employee
    [HttpPost]
    public async Task<IActionResult> Edit(EmployeeViewModel model)
    {
        byte[] pictureData = null;

        if (model.PictureFile != null)
        {
            using (var memoryStream = new MemoryStream())
            {
                await model.PictureFile.CopyToAsync(memoryStream);
                pictureData = memoryStream.ToArray();
            }
        }

        var parameters = new[]
        {
            new SqlParameter("@ID", System.Data.SqlDbType.Int) { Value = model.ID },
            new SqlParameter("@Name", System.Data.SqlDbType.NVarChar) { Value = model.Name },
            new SqlParameter("@DateOfBirth", System.Data.SqlDbType.Date) { Value = model.DateOfBirth },
            new SqlParameter("@Email", System.Data.SqlDbType.NVarChar) { Value = model.Email },
            new SqlParameter("@Picture", System.Data.SqlDbType.VarBinary) { Value = (object)pictureData ?? DBNull.Value }
        };

        await _context.Database.ExecuteSqlRawAsync("EXEC spUpdateEmployee @ID, @Name, @DateOfBirth, @Email, @Picture", parameters);
        return RedirectToAction(nameof(Index));
    }


    // Delete an employee
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var parameters = new[]
        {
        new SqlParameter("@ID", id)
    };

        await _context.Database.ExecuteSqlRawAsync("EXEC spDeleteEmployee @ID", parameters);
        return RedirectToAction(nameof(Index));
    }

}
