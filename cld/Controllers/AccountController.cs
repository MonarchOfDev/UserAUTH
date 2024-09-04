using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using cld.Models;
using System;
using System.Threading.Tasks;

namespace cld.Controllers
{
    public class AccountController : Controller
    {
        private readonly CloudTable _customerProfileTable;
        private readonly ILogger<AccountController> _accountLogger;

        public AccountController(ILogger<AccountController> logger)
        {
            var storageAccount = CloudStorageAccount.Parse("DefaultEndpointsProtocol=https;AccountName=mhleng;AccountKey=gTXLILx+IXsyfzbEYbfHUqJI0e8j7bfLDhWtHdtSoHOm0Fa+OxGEECmqRb1M9r07bR2aQQRY31t++ASt2Zooyw==;EndpointSuffix=core.windows.net");
            var tableClient = storageAccount.CreateCloudTableClient(new TableClientConfiguration());
            _customerProfileTable = tableClient.GetTableReference("CustomerProfile");
            _customerProfileTable.CreateIfNotExists();
            _accountLogger = logger;

        }

        [HttpGet]
        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            if (ModelState.IsValid)
            {
                var customerProfile = new CustomerProfile(model.Email)
                {
                    Name = model.Name,
                    Address = model.Address,
                    PhoneNumber = model.PhoneNumber,
                    Password = model.Password // Consider hashing the password
                };

                var insertOperation = TableOperation.Insert(customerProfile);

                try
                {
                    await _customerProfileTable.ExecuteAsync(insertOperation);
                    HttpContext.Session.SetString("UserProfile", customerProfile.RowKey);
                    _accountLogger.LogInformation("New User Registered");
                    return RedirectToAction("Dashboard", "Home");
                }
                catch (StorageException ex)
                {
                    Console.WriteLine($"Error code: {ex.RequestInformation.HttpStatusCode}");
                    Console.WriteLine($"Error message: {ex.Message}");
                    if (ex.RequestInformation.ExtendedErrorInformation != null)
                    {
                        Console.WriteLine($"Request URL: {ex.RequestInformation.ExtendedErrorInformation.ErrorMessage}");
                    }
                    ModelState.AddModelError("", "An error occurred while saving your profile. Please try again.");
                }
            }

            return View(model);
        }
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            if (ModelState.IsValid)
            {
                var retrieveOperation = TableOperation.Retrieve<CustomerProfile>(model.Email, model.Email);
                try
                {
                    var result = await _customerProfileTable.ExecuteAsync(retrieveOperation);
                    var customerProfile = result.Result as CustomerProfile;

                    if (customerProfile != null && customerProfile.Password == model.Password)
                    {
                        HttpContext.Session.SetString("UserProfile", customerProfile.RowKey);
                        _accountLogger.LogInformation("User Login"+customerProfile.Name);
                        return RedirectToAction("Dashboard", "Home"); // Ensure this action exists
                    }

                    ModelState.AddModelError("", "Invalid login attempt.");
                }
                catch (StorageException ex)
                {
                    Console.WriteLine($"Error code: {ex.RequestInformation.HttpStatusCode}");
                    Console.WriteLine($"Error message: {ex.Message}");
                    if (ex.RequestInformation.ExtendedErrorInformation != null)
                    {
                        Console.WriteLine($"Request URL: {ex.RequestInformation.ExtendedErrorInformation.ErrorMessage}");
                    }
                    ModelState.AddModelError("", "An error occurred while retrieving your profile. Please try again.");
                }
            }

            return View(model);
        }
    }
}
    


