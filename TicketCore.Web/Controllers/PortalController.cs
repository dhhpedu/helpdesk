using DNTCaptcha.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.Net.Http;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using TicketCore.Common;
using TicketCore.Data.Audit.Command;
using TicketCore.Data.EmailVerification.Command;
using TicketCore.Data.Notices.Queries;
using TicketCore.Data.Usermaster.Queries;
using TicketCore.Models.Audit;
using TicketCore.ViewModels.LoginVM;
using TicketCore.ViewModels.Usermaster;
using TicketCore.Web.Extensions;
using TicketCore.Web.Helpers;
using TicketCore.Web.Messages;
using System.Threading.Tasks;
using System.Net.Http.Json;
using System.Text.Json;
using TicketCore.Web.Models;
using TicketCore.Data;
using Microsoft.EntityFrameworkCore;

namespace TicketCore.Web.Controllers
{
    public class PortalController : Controller
    {
        private readonly IUserMasterQueries _userMasterQueries;
        private readonly INoticeQueries _noticeQueries;
        private readonly INotificationService _notificationService;
        private readonly ICheckInStatusQueries _checkInStatusQueries;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IAuditCommand _auditCommand;
        private readonly ILogger<PortalController> _logger;
        private readonly VueTicketDbContext _vueTicketDbContext;

        public PortalController(IUserMasterQueries userMasterQueries,

            INoticeQueries noticeQueries,
            IVerificationCommand verificationCommand,
            INotificationService notificationService,
            ICheckInStatusQueries checkInStatusQueries,
            VueTicketDbContext vueTicketDbContext,
            IHttpContextAccessor httpContextAccessor, IAuditCommand auditCommand, ILogger<PortalController> logger)
        {
            _userMasterQueries = userMasterQueries;
            _noticeQueries = noticeQueries;
            _notificationService = notificationService;
            _checkInStatusQueries = checkInStatusQueries;
            _httpContextAccessor = httpContextAccessor;
            _auditCommand = auditCommand;
            _logger = logger;
            _vueTicketDbContext = vueTicketDbContext;
        }
        public IActionResult Login()
        {
            var token = RandomUniqueToken.Value();
            LoginViewModel loginView = new LoginViewModel()
            {
                Hdrandomtoken = token
            };

            HttpContext.Session.SetString("Hdrandomtoken", token);

            var hiddentoken = HttpContext.Session.GetString("Hdrandomtoken");

            if (string.IsNullOrEmpty(hiddentoken))
            {
                TempData["LoginErrorMessage"] = "Creating Token Issue Retry Again";
            }

            if (_noticeQueries.ShowNotice() != null)
            {
                var notice = _noticeQueries.ShowNotice();
                ViewBag.NoticeTitle = notice.NoticeTitle;
                ViewBag.Noticebody = notice.NoticeBody;
                ViewBag.NoticeCreatedOn = notice.CreatedOn;
            }

            return View(loginView);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [ValidateDNTCaptcha(ErrorMessage = "Nhập đúng mã bảo mật.")]
        public async Task<IActionResult> Login(LoginViewModel loginViewModel)
        {
            if (ModelState.IsValid)
            {
                var http = new HttpClient();
                var endpoint = "https://authen.dhhp.edu.vn/api/user/login";
                var response = await http.PostAsJsonAsync(endpoint, new
                {
                    loginViewModel.Username,
                    loginViewModel.Password
                });
                if (response.StatusCode == HttpStatusCode.BadRequest)
                {
                    ModelState.AddModelError("", await response.Content.ReadAsStringAsync());
                    return View();
                }

                if (!response.IsSuccessStatusCode)
                {
                    ModelState.AddModelError("", $"{response.StatusCode}: Đã có lỗi xảy ra!");
                    return View();
                }    

                var userAPI = await JsonSerializer.DeserializeAsync<UserResponse>(await response.Content.ReadAsStreamAsync());
                if (userAPI is null)
                {
                    ModelState.AddModelError("", "Đã có lỗi xảy ra!");
                    return View();
                }
                if (userAPI.Data.UserType == UserType.Student)
                {
                    ModelState.AddModelError("", "Truy cập bị từ chối!");
                    return View();
                }
                var user = await _userMasterQueries.FindByNameAsync(loginViewModel.Username);
                var roleId = 2;
                if (userAPI.Data.UserType == UserType.Leader)
                {
                    // Chuyên viên -> ko hoat dong
                    roleId = 4;
                }
                if (userAPI.Data.UserType == UserType.Dean)
                {
                    // Nhóm trưởng -> ko hoat dong
                    roleId = 5;
                }
                if (userAPI.Data.UserType == UserType.Deputy)
                {
                    // Trưởng đơn vị
                    roleId = 6;
                }
                if (userAPI.Data.UserType == UserType.Administrator)
                {
                    // Trưởng đơn vị
                    roleId = 1;
                }
                if (user is null)
                {
                    user = new TicketCore.Models.Usermaster.UserMaster
                    {
                        CreatedOn = DateTime.Now,
                        UserName = loginViewModel.Username,
                        LastName = userAPI.Data.LastName,
                        FirstName = userAPI.Data.FirstName,
                        IsFirstLogin = false,
                        IsFirstLoginDate = DateTime.Now,
                        PasswordHash = MD5Encryption(loginViewModel.Password),
                        Status = true,
                        EmailId = userAPI.Data.Email
                    };
                    await _vueTicketDbContext.AddAsync(user);
                    await _vueTicketDbContext.SaveChangesAsync();
                }
                else
                {
                    user.PasswordHash = MD5Encryption(loginViewModel.Password);
                    user.FirstName = userAPI.Data.FirstName;
                    user.LastName = userAPI.Data.LastName;
                    user.EmailId = userAPI.Data.Email;
                    user.IsFirstLogin = false;
                    _vueTicketDbContext.Update(user);
                }
                var role = await _vueTicketDbContext.AssignedRoles.FirstOrDefaultAsync(x => x.UserId == user.UserId);
                if (role is null)
                {
                    await _vueTicketDbContext.AssignedRoles.AddAsync(new TicketCore.Models.Usermaster.AssignedRoles
                    {
                        RoleId = roleId,
                        CreatedOn = DateTime.Now,
                        Status = true,
                        UserId = user.UserId
                    });
                }
                else
                {
                    role.RoleId = roleId;
                    _vueTicketDbContext.AssignedRoles.Update(role);
                }

                await _vueTicketDbContext.SaveChangesAsync();
                var loggedInuserdetails = _userMasterQueries.GetCommonUserDetailsbyUserName(loginViewModel.Username);

                if (loggedInuserdetails == null)
                {
                    ModelState.AddModelError("", "Sai tên đăng nhập hoặc mật khẩu");
                    return View();
                }

                if (loggedInuserdetails.Status == false)
                {
                    ModelState.AddModelError("", "Tài khoản của bạn không hoạt động Liên hệ Quản trị viên");
                    return View();
                }
                if (loggedInuserdetails.RoleId == Convert.ToInt32(StatusMain.Roles.Agent) || loggedInuserdetails.RoleId == Convert.ToInt32(StatusMain.Roles.AgentManager) || loggedInuserdetails.RoleId == Convert.ToInt32(StatusMain.Roles.Administrator))
                {
                    if (!IsCategogryAssigned(loggedInuserdetails.UserId, loggedInuserdetails.RoleId))
                    {
                        TempData["LoginErrors"] = "Danh mục chưa được chỉ định, Vui lòng liên hệ với quản trị viên của bạn";
                        ModelState.AddModelError("", "Danh mục chưa được chỉ định, Vui lòng liên hệ với quản trị viên của bạn");
                        return View(loginViewModel);
                    }
                }

                SetAuthenticationCookie();
                SetApplicationSession(loggedInuserdetails);


                if (loggedInuserdetails.RoleId == Convert.ToInt32(StatusMain.Roles.User) && loggedInuserdetails.IsFirstLogin)
                {
                    HttpContext.Session.SetString("OnboardingUser", "1");
                    return RedirectToAction("Process", "Onboarding", new { Area = "User" });
                }

                if (loggedInuserdetails.RoleId == Convert.ToInt32(StatusMain.Roles.SuperAdmin))
                {
                    Response.Cookies.Append(
                        CookieRequestCultureProvider.DefaultCookieName,
                        CookieRequestCultureProvider.MakeCookieValue(new RequestCulture("en")),
                        new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
                    );

                }

                return RedirectionManager(loggedInuserdetails.UserId, loggedInuserdetails.RoleId);
            }

            return View();
        }

        [HttpPost]
        public IActionResult Logout()
        {
            try
            {
                AuditLogout();
                var roleid = HttpContext.Session.GetInt32(AllSessionKeys.RoleId);
                if (roleid == Convert.ToInt32(StatusMain.Roles.Agent))
                {
                    var userId = HttpContext.Session.GetSession<long>(AllSessionKeys.UserId);
                    if (_checkInStatusQueries.CheckIsalreadyCheckedIn(userId))
                    {
                        _checkInStatusQueries.StatusCheckInCheckOut(userId);
                    }
                }

                CookieOptions option = new CookieOptions();

                if (Request.Cookies[AllSessionKeys.AuthenticationToken] != null)
                {
                    option.Expires = DateTime.Now.AddDays(-1);
                    Response.Cookies.Append(AllSessionKeys.AuthenticationToken, "", option);
                }

                HttpContext.Session.Clear();

                return RedirectToAction("Login", "Portal");
            }
            catch (Exception)
            {
                throw;
            }

        }

        public string MD5Encryption(string originalPassword)
        {
            //Declarations
            Byte[] originalBytes;
            Byte[] encodedBytes;
            MD5 md5;

            //Instantiate MD5CryptoServiceProvider, get bytes for original password and compute hash (encoded password)
            md5 = new MD5CryptoServiceProvider();
            originalBytes = System.Text.ASCIIEncoding.Default.GetBytes(originalPassword);
            encodedBytes = md5.ComputeHash(originalBytes);
            StringBuilder strBuilder = new StringBuilder();
            for (int i = 0; i < encodedBytes.Length; i++)
            {
                //change it into 2 hexadecimal digits  
                //for each byte  
                strBuilder.Append(encodedBytes[i].ToString("x2"));
            }

            return strBuilder.ToString().ToUpper();
        }
        private void SetAuthenticationCookie()
        {
            string strAuthToken = Guid.NewGuid().ToString();
            HttpContext.Session.SetString(AllSessionKeys.AuthenticationToken, strAuthToken);
            Response.Cookies.Append(AllSessionKeys.AuthenticationToken, strAuthToken);
        }

        private void SetApplicationSession(CommonUserDetailsViewModel commonUser)
        {
            HttpContext.Session.SetInt32(AllSessionKeys.RoleId, commonUser.RoleId);
            HttpContext.Session.SetSession<long>(AllSessionKeys.UserId, commonUser.UserId);
            HttpContext.Session.SetString(AllSessionKeys.UserName, Convert.ToString(commonUser.UserName));

            HttpContext.Session.SetString(AllSessionKeys.RoleName, Convert.ToString(commonUser.RoleName));
            if (commonUser.FirstName != null)
                HttpContext.Session.SetString(AllSessionKeys.FirstName, Convert.ToString(commonUser.FirstName));
            if (commonUser.LastName != null)
                HttpContext.Session.SetString(AllSessionKeys.LastName, Convert.ToString(commonUser.LastName));
            HttpContext.Session.SetString(AllSessionKeys.EmailId, Convert.ToString(commonUser.EmailId));
            if (!string.IsNullOrEmpty(commonUser.MobileNo))
            {
                HttpContext.Session.SetString(AllSessionKeys.MobileNo, Convert.ToString(commonUser.MobileNo));
            }

            if (!string.IsNullOrEmpty(commonUser.FirstName) && !string.IsNullOrEmpty(commonUser.LastName))
                HttpContext.Session.SetString(AllSessionKeys.FullName, $"{commonUser.FirstName} {commonUser.LastName}");

            AuditLogin();
        }

        [NonAction]
        private string ConcateTokenandPassword(string storedDatabasePassword, string hiddenrandomtoken)
        {
            try
            {
                return ComputeSha256Hash(hiddenrandomtoken + storedDatabasePassword);
            }
            catch (Exception)
            {
                throw;
            }
        }
        private static string ComputeSha256Hash(string rawData)
        {
            // Create a SHA256   
            try
            {
                using SHA256 sha256Hash = SHA256.Create();
                // ComputeHash - returns byte array  
                byte[] bytes = sha256Hash.ComputeHash(Encoding.UTF8.GetBytes(rawData));

                // Convert byte array to a string   
                StringBuilder builder = new StringBuilder();
                foreach (var t in bytes)
                {
                    builder.Append(t.ToString("x2"));
                }
                return builder.ToString();
            }
            catch (Exception)
            {

                throw;
            }
        }

        private void Greetings(string name)
        {

            var message = "";
            var currentDateTime = DateTime.Now;
            int currentHour = currentDateTime.Hour;
            int startMorningHour = 6;
            int startAfternoonHour = 12;
            int startEveningHour = 17;
            int startNightHour = 22;

            if (startMorningHour <= currentHour && currentHour < startAfternoonHour)
            {
                message = "Good morning!";
            }
            if (startAfternoonHour <= currentHour && currentHour < startEveningHour)
            {
                message = "Good Afternoon!";
            }

            if (startEveningHour <= currentHour && currentHour < startNightHour)
            {
                message = "Good Evening!";
            }
            if (startNightHour <= currentHour || currentHour < startMorningHour)
            {
                message = "Good Night!";
            }
            _notificationService.InformationNotification("Message", NotificationType.info, $"{message}, {name}");
        }

        private bool IsCategogryAssigned(long userId, int roleId)
        {
            if (roleId == Convert.ToInt32(StatusMain.Roles.Agent) && _checkInStatusQueries.CheckIsCategoryAssignedtoAgent(userId))
            {
                return true;
            }

            if (roleId == Convert.ToInt32(StatusMain.Roles.AgentManager) && _userMasterQueries.CheckIsCategogryAssignedtoAgentAdmin(userId))
            {
                return true;
            }

            if (roleId == Convert.ToInt32(StatusMain.Roles.Administrator) && _userMasterQueries.CheckIsCategogryAssignedtoHod(userId))
            {
                return true;
            }

            return false;
        }

        public IActionResult RedirectionManager(long userId, int roleId)
        {

            if (roleId == Convert.ToInt32(StatusMain.Roles.SuperAdmin))
            {
                return RedirectToAction("Dashboard", "Dashboard", new { Area = "Administration" });
            }

            if (roleId == Convert.ToInt32(StatusMain.Roles.User))
            {
                return RedirectToAction("Dashboard", "Dashboard", new { Area = "User" });
            }

            if (roleId == Convert.ToInt32(StatusMain.Roles.Administrator))
            {
                return RedirectToAction("Dashboard", "MyDashboard", new { Area = "Administrator" });
            }

            if (roleId == Convert.ToInt32(StatusMain.Roles.AgentManager))
            {
                return RedirectToAction("Dashboard", "MyDashboard", new { Area = "AgentManager" });
            }

            if (roleId == Convert.ToInt32(StatusMain.Roles.Administrator))
            {
                return RedirectToAction("Dashboard", "MyDashboard", new { Area = "Administration" });
            }

            if (roleId == Convert.ToInt32(StatusMain.Roles.Agent))
            {
                if (_checkInStatusQueries.CheckIsalreadyCheckedIn(userId))
                {
                    HttpContext.Session.SetString("CheckInSession", "N");
                    return RedirectToAction("Dashboard", "MyDashboard", new { Area = "User" });
                }
                else
                {
                    HttpContext.Session.SetString("CheckInSession", "Y");

                    return RedirectToAction("Process", "CheckIn", new { Area = "User" });
                }
            }

            return RedirectToAction("Login", "Portal");

        }

        private void AuditLogin()
        {
            try
            {
                var objaudit = new AuditModel();
                objaudit.AuditId = 0;
                objaudit.RoleId = HttpContext.Session.GetInt32(AllSessionKeys.RoleId);
                objaudit.ControllerName = "Portal";
                objaudit.ActionName = "Login";
                objaudit.Area = "";
                objaudit.LoggedInAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                if (_httpContextAccessor.HttpContext != null)
                    objaudit.IPAddress = Convert.ToString(_httpContextAccessor.HttpContext.Connection.RemoteIpAddress);
                objaudit.UserID = HttpContext.Session.GetSession<long>(AllSessionKeys.UserId);
                objaudit.PortalToken = "";
                objaudit.PageAccessed = "";
                objaudit.SessionID = HttpContext.Session.Id;
                objaudit.CurrentDatetime = DateTime.Now;
                objaudit.Logged = true;
                _auditCommand.InsertAuditData(objaudit);
            }
            catch (Exception)
            {

                throw;
            }
        }
        private void AuditLogout()
        {
            try
            {
                var objaudit = new AuditModel();
                objaudit.AuditId = 0;
                objaudit.RoleId = HttpContext.Session.GetInt32(AllSessionKeys.RoleId);
                objaudit.ControllerName = "Portal";
                objaudit.ActionName = "Logout";
                objaudit.Area = "";
                objaudit.LoggedOutAt = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture);
                if (_httpContextAccessor.HttpContext != null)
                    objaudit.IPAddress = Convert.ToString(_httpContextAccessor.HttpContext.Connection.RemoteIpAddress);
                objaudit.UserID = HttpContext.Session.GetSession<long>(AllSessionKeys.UserId);
                objaudit.PortalToken = "";
                objaudit.PageAccessed = "";
                objaudit.SessionID = HttpContext.Session.Id;
                objaudit.CurrentDatetime = DateTime.Now;
                objaudit.Logged = true;
                _auditCommand.InsertAuditData(objaudit);
            }
            catch (Exception)
            {

                throw;
            }
        }


    }
}
