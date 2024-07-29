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
                new SqlParameter("@Name", model.Name),
                new SqlParameter("@DateOfBirth", model.DateOfBirth),
                new SqlParameter("@Email", model.Email),
                new SqlParameter("@Picture", (object)pictureData ?? DBNull.Value)
            };

            await _context.Database.ExecuteSqlRawAsync("EXEC spAddEmployee @Name, @DateOfBirth, @Email, @Picture", parameters);
            return RedirectToAction(nameof(Index));
     
    }

    // Edit an employee
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

        try
        {
            // Prepare SQL parameters with explicit types
            var parameters = new[]
            {
            new SqlParameter("@ID", SqlDbType.Int) { Value = model.ID },
            new SqlParameter("@Name", SqlDbType.NVarChar, 100) { Value = model.Name },
            new SqlParameter("@DateOfBirth", SqlDbType.Date) { Value = model.DateOfBirth },
            new SqlParameter("@Email", SqlDbType.NVarChar, 100) { Value = model.Email },
            new SqlParameter("@Picture", SqlDbType.VarBinary, -1) { Value = (object)pictureData ?? DBNull.Value }
        };

            // Execute stored procedure to update employee
            await _context.Database.ExecuteSqlRawAsync("EXEC spUpdateEmployee @ID, @Name, @DateOfBirth, @Email, @Picture", parameters);

            // Redirect to the Index action upon success
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            // Log the exception (use a logger in a real application)
            Console.WriteLine($"An error occurred: {ex.Message}");

            // Optionally, add an error message to the ModelState and return to the Index view
            // ModelState.AddModelError("", "An error occurred while updating the employee.");
            // return View("Index", model);

            // Redirect to the Index action (you may want to show an error page or message)
            return RedirectToAction(nameof(Index));
        }
    }


    // Delete an employee
    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        await _context.Database.ExecuteSqlRawAsync("EXEC spDeleteEmployee @ID", new SqlParameter("@ID", id));
        return RedirectToAction(nameof(Index));
    }
}
