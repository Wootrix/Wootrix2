using CsvHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Options;
using OfficeOpenXml;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Wootrix.Data;
using WootrixV2.Data;
using WootrixV2.Models;

namespace WootrixV2.Controllers
{
    [Authorize]
    public class ArticleReportingController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private WootrixV2.Models.User _user;
        private ApplicationUser _user1;
        private readonly IOptions<RequestLocalizationOptions> _rlo;

        public ArticleReportingController(IOptions<RequestLocalizationOptions> rlo, ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
            _rlo = rlo;
        }

        // GET: ArticleReportings
        public async Task<IActionResult> Index(int? id)
        {
            _user = _context.User.FirstOrDefault(p => p.EmailAddress == _userManager.GetUserAsync(User).GetAwaiter().GetResult().Email);
            var companyID = _user.CompanyID;
            //We need to build the datasets for the graphs into the viewbag
            var articlesRead = _context.ArticleReporting.Where(m => m.CompanyID == companyID);
            var totalArticles = _context.SegmentArticle.Where(m => m.CompanyID == companyID);
            ViewBag.Magazine = false;
            ViewBag.Magazine = _context.ArticleReporting.GroupBy(x => new { x.SegmentID, x.SegmentName }).Select(y => new Magazine
            {
                SegmentId = y.Key.SegmentID,
                SegmentName = y.Key.SegmentName,
                Previews = y.Count(),
                percentageMagazine = (y.Count()) * 100 / (_context.ArticleReporting.Select(s => s.SegmentName).Count())
            }).Take(10).Distinct();

            ViewBag.Article = _context.ArticleReporting.GroupBy(x => new { x.ArticleID, x.ArticleName }).Select(y => new Article
            {
                ArticleID = y.Key.ArticleID,
                ArticleName = y.Key.ArticleName,
                Previews = y.Count(),
                percentageArticle = (y.Count()) * 100 / (_context.ArticleReporting.Select(s => s.ArticleName).Count())

            }).Take(10).Distinct();

            ViewBag.Topics = _context.SegmentArticle.Where(y => !string.IsNullOrEmpty(y.Topics)).GroupBy(x => new { x.CompanyID, x.Topics }).Select(y => new Topics
            {
                TopicID = y.Key.CompanyID,
                TopiceName = y.Key.Topics,
                Previews = y.Count(),
                percentageTopic = (y.Count()) * 100 / (_context.SegmentArticle.Select(s => s.Topics).Count())

            }).Take(10).Distinct();

            ViewBag.Groups = _context.SegmentArticle.Where(y => !string.IsNullOrEmpty(y.Groups)).GroupBy(x => new { x.CompanyID, x.Groups }).Select(y => new Groups
            {
                GroupID = y.Key.CompanyID,
                GroupName = y.Key.Groups,
                Previews = y.Count(),
                percentageGroup = (y.Count()) * 100 / (_context.SegmentArticle.Select(s => s.Groups).Count())

            }).Take(10).Distinct();

            ViewBag.User = _context.SegmentArticle.Where(y => !string.IsNullOrEmpty(y.TypeOfUser)).GroupBy(x => new { x.CompanyID, x.TypeOfUser }).Select(y => new Users
            {
                UserID = y.Key.CompanyID,
                UserName = y.Key.TypeOfUser,
                Previews = y.Count(),
                percentageUser = (y.Count()) * 100 / (_context.SegmentArticle.Select(s => s.TypeOfUser).Count())

            }).Take(10).Distinct();

            ViewBag.Languages = _context.SegmentArticle.Where(y => !string.IsNullOrEmpty(y.Languages)).GroupBy(x => new { x.CompanyID, x.Languages }).Select(y => new Languages
            {
                LanguageID = y.Key.CompanyID,
                LanguageName = y.Key.Languages,
                Previews = y.Count(),
                percentageLanguage = (y.Count()) * 100 / (_context.SegmentArticle.Select(s => s.Languages).Count())

            }).Take(10).Distinct();

            ViewBag.City = _context.ArticleReporting.Where(y => !string.IsNullOrEmpty(y.City)).GroupBy(x => new { x.CompanyID, x.City }).Select(y => new City
            {
                CityID = y.Key.CompanyID,
                Citys = y.Key.City,
                Previews = y.Count(),
                percentageCity = (y.Count()) * 100 / (_context.ArticleReporting.Select(s => s.City).Count())

            }).Take(10).Distinct();

            ViewBag.States = _context.ArticleReporting.Where(y => !string.IsNullOrEmpty(y.State)).GroupBy(x => new { x.CompanyID, x.State }).Select(y => new States
            {
                StateID = y.Key.CompanyID,
                State = y.Key.State,
                Previews = y.Count(),
                percentageState = (y.Count()) * 100 / (_context.ArticleReporting.Select(s => s.State).Count())

            }).Take(10).Distinct();

            ViewBag.Country = _context.ArticleReporting.Where(y => !string.IsNullOrEmpty(y.Country)).GroupBy(x => new { x.CompanyID, x.Country }).Select(y => new Countrys
            {
                CountryID = y.Key.CompanyID,
                Country = y.Key.Country,
                Previews = y.Count(),
                percentageCountry = (y.Count()) * 100 / (_context.ArticleReporting.Select(s => s.Country).Count())

            }).Take(10).Distinct();


            AddFilters(companyID);

            SetupOSReport(articlesRead);
            SetupPlatformReport(articlesRead);
            SetupViewsByLocation(articlesRead);
            SetupViewsPerArticleReport(articlesRead);

            object FilterData = new FilterData();

            if (!string.IsNullOrWhiteSpace(id.ToString()))
            {
                //Admin user has selected a companyID so set the current session stuff appropriately
                var companyName = _context.Company.FirstOrDefaultAsync(n => n.ID == id).GetAwaiter().GetResult().CompanyName;
                HttpContext.Session.SetInt32("id", id ?? 0);
                HttpContext.Session.SetString("CompanyName", companyName);
            }

            DataAccessLayer dla = new DataAccessLayer(_context);
            _user1 = _userManager.GetUserAsync(User).GetAwaiter().GetResult();

            var reportingModel = new Reporting()
            {
                Reports = await articlesRead.ToListAsync(),
                FilterDetaails = GetFilterData(dla)
            };

            return View(reportingModel);
        }

        [HttpPost]
        public async Task<IActionResult> Index(FilterData data, int? id)
        {
            _user = _context.User.FirstOrDefault(p => p.EmailAddress == _userManager.GetUserAsync(User).GetAwaiter().GetResult().Email);
            var companyID = _user.CompanyID;
            //We need to build the datasets for the graphs into the viewbag
            var totalArticles = _context.SegmentArticle.Where(m => m.CompanyID == companyID
                                                                   && (data.SelectedGroups.Count() <= 0 || data.SelectedGroups.Any(x => x == m.Groups))
                                                                   && (data.SelectedTypeOfUser.Count() <= 0 || data.SelectedTypeOfUser.Any(x => x == m.TypeOfUser))
                                                                   && (data.SelectedTopics.Count() <= 0 || data.SelectedTopics.Any(x => x == m.Topics))
                                                                   && (data.SelectedLanguages.Count <= 0 || data.SelectedLanguages.Any(x => x == m.Languages)));

            var articlesRead = _context.ArticleReporting.Where(m => m.CompanyID == companyID
                                                                    && (data.FromDate == null || m.ArticleReadTime >= data.FromDate)
                                                                    && (data.ToDate == null || m.ArticleReadTime <= data.ToDate)
                                                                    && (data.SelectedSegments.Count() <= 0 || data.SelectedSegments.Any(y => y == m.SegmentName))
                                                                    && (data.SelectedArticles.Count() <= 0 || data.SelectedArticles.Any(y => y == m.ArticleName))                                                                    
                                                                    && (data.Country == null || m.Country == data.Country)
                                                                    && (data.State == null || m.State == data.State)
                                                                    && (data.City == null || m.City == data.City));            

            var segmentArticle = _context.SegmentArticle.Where(x => x.CompanyID == companyID).ToList();

            var segments = from sa in _context.SegmentArticle
                           join ar in _context.ArticleReporting on sa.CompanyID equals ar.CompanyID into joinedSegment
                           from ar in joinedSegment.DefaultIfEmpty()
                           where sa.CompanyID == companyID
                                 && (data.FromDate == null || ar.ArticleReadTime >= data.FromDate)
                                 && (data.ToDate == null || ar.ArticleReadTime <= data.ToDate)
                                 && (data.Country == null || sa.Country == data.Country)
                                 && (data.State == null || sa.State == data.State)
                                 && (data.City == null || sa.City == data.City)
                                 && (data.SelectedGroups.Count <= 0 || data.SelectedGroups.Any(x => x == sa.Groups))
                                 && (data.SelectedTypeOfUser.Count <= 0 || data.SelectedTypeOfUser.Any(x => x == sa.TypeOfUser))
                                 && (data.SelectedTopics.Count <= 0 || data.SelectedTopics.Any(x => x == sa.Topics))
                                 && (data.SelectedLanguages.Count <= 0 || data.SelectedLanguages.Any(x => x == sa.Languages))
                                 && (data.SelectedSegments.Count <= 0 || data.SelectedSegments.Any(x => x == ar.SegmentName))
                                 && (data.SelectedArticles.Count() <= 0 || data.SelectedArticles.Any(y => y == ar.ArticleName))                                 
                           group new { sa, ar } by new { ar.SegmentID, ar.SegmentName } into sr
                           select new
                           {
                               CompanyID = companyID,
                               SegmentID = sr.Key.SegmentID,
                               SegmentName = sr.Key.SegmentName,
                               PreviewCount = sr.Select(x => x.ar).Count(),
                               PreviewInPercentage = (sr.Select(x => x.ar).Count()) * 100 / (_context.ArticleReporting.Select(s => s.SegmentName).Count())
                           };

            ViewBag.Magazine = segments.Select(y => new Magazine
            {
                SegmentId = y.SegmentID,
                SegmentName = y.SegmentName,
                Previews = y.PreviewCount,
                percentageMagazine = y.PreviewInPercentage
            }).Take(10).Distinct();
            
            ViewBag.Article = _context.ArticleReporting.Where(x => segments.Any(sg => sg.SegmentID == x.SegmentID))
                                                       .Select(ar => new Article()
                                                       {
                                                           ArticleID = ar.ArticleID,
                                                           ArticleName = ar.ArticleName,
                                                           Previews = segments.FirstOrDefault(x => x.SegmentID == ar.SegmentID).PreviewCount,
                                                           percentageArticle = segments.FirstOrDefault(x => x.SegmentID == ar.SegmentID).PreviewInPercentage,
                                                       }).Take(10).Distinct();          

            ViewBag.Topics = segmentArticle.Where(x => segments.Any(sg => x.Segments.Contains(sg.SegmentName)))
                                                     .Select(sa => new Topics()
                                                     {
                                                         TopicID = sa.CompanyID,
                                                         TopiceName = sa.Topics,
                                                         Previews = segments.FirstOrDefault(x => x.CompanyID == sa.CompanyID).PreviewCount,
                                                         percentageTopic = segments.FirstOrDefault(x => x.CompanyID == sa.CompanyID).PreviewInPercentage,
                                                     }).ToList();

            ViewBag.Groups = segmentArticle.Where(x => segments.Any(sg => x.Segments.Contains(sg.SegmentName)))
                                                    .Select(sa => new Groups
                                                    {
                                                        GroupID = sa.CompanyID,
                                                        GroupName = sa.Groups,
                                                        Previews = segments.FirstOrDefault(x => x.CompanyID == sa.CompanyID).PreviewCount,
                                                        percentageGroup = segments.FirstOrDefault(x => x.CompanyID == sa.CompanyID).PreviewInPercentage
                                                    }).Take(10).Distinct();

            ViewBag.User = segmentArticle.Where(x => segments.Any(sg => x.Segments.Contains(sg.SegmentName)))
                                                  .Select(sa => new Users
                                                  {
                                                      UserID = sa.CompanyID,
                                                      UserName = sa.TypeOfUser,
                                                      Previews = segments.FirstOrDefault(x => x.CompanyID == sa.CompanyID).PreviewCount,
                                                      percentageUser = segments.FirstOrDefault(x => x.CompanyID == sa.CompanyID).PreviewInPercentage
                                                  }).Take(10).Distinct();

            ViewBag.Languages = segmentArticle.Where(x => segments.Any(sg => x.Segments.Contains(sg.SegmentName)))
                                                       .Select(sa => new Languages
                                                       {
                                                           LanguageID = sa.CompanyID,
                                                           LanguageName = sa.Languages,
                                                           Previews = segments.FirstOrDefault(x => x.CompanyID == sa.CompanyID).PreviewCount,
                                                           percentageLanguage = segments.FirstOrDefault(x => x.CompanyID == sa.CompanyID).PreviewInPercentage
                                                       }).Take(10).Distinct();

            ViewBag.City = _context.ArticleReporting.Where(x => segments.Any(sg => sg.SegmentID == x.SegmentID))
                                                    .Select(ar => new City
                                                    {
                                                        CityID = ar.CompanyID,
                                                        Citys = ar.City,
                                                        Previews = segments.FirstOrDefault(x => x.SegmentID == ar.SegmentID).PreviewCount,
                                                        percentageCity = segments.FirstOrDefault(x => x.SegmentID == ar.SegmentID).PreviewInPercentage,
                                                    }).Take(10).Distinct();

            ViewBag.States = _context.ArticleReporting.Where(x => segments.Any(sg => sg.SegmentID == x.SegmentID))
                                                    .Select(ar => new States
                                                    {
                                                        StateID = ar.CompanyID,
                                                        State = ar.State,
                                                        Previews = segments.FirstOrDefault(x => x.SegmentID == ar.SegmentID).PreviewCount,
                                                        percentageState = segments.FirstOrDefault(x => x.SegmentID == ar.SegmentID).PreviewInPercentage,
                                                    }).Take(10).Distinct();

            ViewBag.Country = _context.ArticleReporting.Where(x => segments.Any(sg => sg.SegmentID == x.SegmentID))
                                                       .Select(ar => new Countrys
                                                       {
                                                           CountryID = ar.CompanyID,
                                                           Country = ar.State,
                                                           Previews = segments.FirstOrDefault(x => x.SegmentID == ar.SegmentID).PreviewCount,
                                                           percentageCountry = segments.FirstOrDefault(x => x.SegmentID == ar.SegmentID).PreviewInPercentage,
                                                       }).Take(10).Distinct();

            AddFilters(companyID);

            SetupOSReport(articlesRead);
            SetupPlatformReport(articlesRead);
            SetupViewsByLocation(articlesRead);
            SetupViewsPerArticleReport(articlesRead);

            if (!string.IsNullOrWhiteSpace(id.ToString()))
            {
                //Admin user has selected a companyID so set the current session stuff appropriately
                var companyName = _context.Company.FirstOrDefaultAsync(n => n.ID == id).GetAwaiter().GetResult().CompanyName;
                HttpContext.Session.SetInt32("id", id ?? 0);
                HttpContext.Session.SetString("CompanyName", companyName);
            }

            DataAccessLayer dla = new DataAccessLayer(_context);
            _user1 = _userManager.GetUserAsync(User).GetAwaiter().GetResult();

            var reportingModel = new Reporting()
            {
                Reports = await articlesRead.ToListAsync(),
                FilterDetaails = GetFilterData(dla)

            };

            return View(reportingModel);
        }

        public bool CheckListItemContainedInString(string input, List<string> list)
        {
            bool result = false;
            return result;
        }

        public IActionResult ExportToCSV()
        {
            var memoryStream = new MemoryStream();
            var streamWriter = new StreamWriter(memoryStream, Encoding.UTF8, 1024, true);

            var exportData = _context.ArticleReporting.GroupBy(m => m.UserName)
                .Select(m => m.First())
                .Join(_context.SegmentArticle, x => x.CompanyID, m => m.CompanyID, (x, m) =>
                    new
                    {
                       UserName = x.UserName,
                       TypeOfUser = m.TypeOfUser,
                       InterfaceLanguage = m.Languages,
                       City = x.City,
                       State = x.State,
                       Country = x.Country,
                       OSType = x.OSType,
                       DeviceType = x.DeviceType
                    }
                );

            using (var csvWriter = new CsvWriter(streamWriter))
            {
                csvWriter.WriteRecords(exportData);
            }
            memoryStream.Position = 0;
            return File(memoryStream, "text/csv", "ExportedFileToCSV.csv");
        }

        public IActionResult ExportToExcel()
        {

            var comlumHeadrs = new string[]
            {
                "UserName",
                "TypeOfUser",
                "Languages",
                "City",
                "Country",
                "State",
                "OperationalSyatem",
                "DeviceType"
            };

            byte[] result;

            using (var package = new ExcelPackage())
            {
                // add a new worksheet to the empty workbook

                var worksheet = package.Workbook.Worksheets.Add("Export Data"); //Worksheet name
                using (var cells = worksheet.Cells[1, 1, 1, 8]) //(1,1) (1,5)
                {
                    cells.Style.Font.Bold = true;
                }

                //First add the headers
                for (var i = 0; i < comlumHeadrs.Count(); i++)
                {
                    worksheet.Cells[1, i + 1].Value = comlumHeadrs[i];
                }

                //Add values
                var j = 2;
                var data = (from ar in _context.ArticleReporting
                    join sa in _context.SegmentArticle on ar.CompanyID equals sa.CompanyID
                
                    select new 
                    {
                        UserName = ar.UserName,
                        TypeOfUser = sa.TypeOfUser,
                        InterfaceLanguage = sa.Languages,
                        City = ar.City,
                        State = sa.State,
                        Country = sa.Country,
                        OSType = ar.OSType,
                        DeviceType = ar.DeviceType
                    }).Distinct();
                foreach (var EData in data.Distinct().ToList())
                {
                    worksheet.Cells["A" + j].Value = EData.UserName;
                    worksheet.Cells["B" + j].Value = EData.TypeOfUser;
                    worksheet.Cells["C" + j].Value = EData.InterfaceLanguage;
                    worksheet.Cells["D" + j].Value = EData.City;
                    worksheet.Cells["E" + j].Value = EData.Country;
                    worksheet.Cells["F" + j].Value = EData.State;
                    worksheet.Cells["G" + j].Value = EData.OSType;
                    worksheet.Cells["H" + j].Value = EData.DeviceType;

                    j++;
                }
                result = package.GetAsByteArray();
            }

            var result1 = new FileContentResult(result, "application/octet-stream");
            result1.FileDownloadName = "ExportedfileToExcel.xlsx";
            return result1;

        }

        private FilterData GetFilterData(DataAccessLayer dla)
        {
            var _filters = new FilterData();
            var r = HttpContext.Session.GetString("ManageType");

            var cp = HttpContext.Session.GetInt32("id") ?? 0;
            _filters.CompanyID = HttpContext.Session.GetInt32("id") ?? 0;
            _filters.CompanyName = HttpContext.Session.GetString("CompanyName") ?? "";
            if (r != null)
            {
                _filters.Role = (Roles)Enum.Parse(typeof(Roles), r);
            }
            _filters.Departments = dla.GetDepartments(cp);
            _filters.Genders = dla.GetGenders();
            _filters.InterfaceLanguages = dla.GetInterfaceLanguages(cp, _rlo);

            // Add group checkboxes
            var listOfAllGroups = dla.GetListGroups(_user1.companyID);
            foreach (var seg in listOfAllGroups)
            {
                _filters.AvailableGroups.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem() { Text = seg.Value, Value = seg.Value });
            }

            // Add topic checkboxes
            var listOfAllTopics = dla.GetListTopics(_user.CompanyID);
            foreach (var seg in listOfAllTopics)
            {
                _filters.AvailableTopics.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem() { Text = seg.Value, Value = seg.Value });
            }

            // Add Magazines
            var listOfMagazines = dla.GetArticleSegments(_user.CompanyID);
            foreach (var seg in listOfMagazines)
            {
                _filters.AvailableSegments.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem() { Text = seg.Value, Value = seg.Value });
            }

            //Add Articles
            var listOfArticles = dla.GetArticleNames(_user.CompanyID);
            foreach (var seg in listOfArticles)
            {
                _filters.AvailableArticles.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem() { Text = seg.Value, Value = seg.Value });
            }

            // Add type checkboxes
            var listOfAllTypeOfUser = dla.GetListTypeOfUser(_user.CompanyID);
            foreach (var seg in listOfAllTypeOfUser)
            {
                _filters.AvailableTypeOfUser.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem() { Text = seg.Value, Value = seg.Value });
            }

            // Add language checkboxes
            var listOfAllLanguages = dla.GetListLanguages(_user.CompanyID, _rlo);
            foreach (var seg in listOfAllLanguages)
            {
                _filters.AvailableLanguages.Add(new Microsoft.AspNetCore.Mvc.Rendering.SelectListItem() { Text = seg.Value, Value = seg.Value });
            }

            // Get location dropdown data
            _filters.Countries = dla.GetCountries();
            _filters.States = dla.GetNullStatesOrCities();
            _filters.Cities = dla.GetNullStatesOrCities();

            return _filters;
        }

        [HttpPost]
        public IActionResult UpdateGraphs(GraphFilters filters)
        {
            if (!string.IsNullOrWhiteSpace(filters.ToString()))
            {
                _user = _context.User.FirstOrDefault(p => p.EmailAddress == _userManager.GetUserAsync(User).GetAwaiter().GetResult().Email);
                var companyID = _user.CompanyID;

                //We need to build the datasets for the graphs into the viewbag
                var articlesRead = _context.ArticleReporting.Where(m => m.CompanyID == companyID && m.SegmentName == filters.magazine/* && m.ArticleReadTime >= dateFrom&&m.ArticleReadTime<=dateTo*/);

                string[] articles = articlesRead.Select(d => d.ArticleName).Distinct().ToArray();
                SetupViewsPerArticleReport(articlesRead);
                return Json(articles);
            }
            return null;
        }

        public class GraphFilters
        {
            public string magazine { get; set; }
        }

        public void AddFilters(int companyID)
        {
            ViewBag.Segments = _context.CompanySegment.Where(m => m.CompanyID == companyID).Select(m => m.Title).Distinct().ToList();

            ViewBag.Articles = _context.SegmentArticle.Select(m => m.Title).Distinct().ToList();
            //ViewBag.TypeOfUser = _context.CompanyTypeOfUser.Where(m => m.CompanyID == companyID).Select(m => m.TypeOfUser).Distinct().ToList();
            //ViewBag.Topics = _context.CompanyTopics.Where(m => m.CompanyID == companyID).Select(m => m.Topic).Distinct().ToList();
            //ViewBag.Languages = _context.CompanyLanguages.Where(m => m.CompanyID == companyID).Select(m => m.LanguageName).Distinct().ToList();
            //ViewBag.Groups = _context.CompanyGroups.Where(m => m.CompanyID == companyID).Select(m => m.GroupName).Distinct().ToList();
            //ViewBag.Countries = _context.CompanyLocCountries.Select(m => m.country_name).Distinct().ToList();
            //ViewBag.States = _context.CompanyLocStates.Select(m => m.state_name).Distinct().ToList();
            //ViewBag.Cities = _context.CompanyLocCities.Select(m => m.city_name_ascii).Distinct().ToList();
        }

        public void SetupOSReport(IQueryable<ArticleReporting> articlesRead)
        {
            //Need the count and name of each OS type put into arrays
            List<string> osTypes = articlesRead/*.Where(d => d.ArticleReadTime <= dateFrom && d.ArticleReadTime >= dateTo)*/.Select(d => d.OSType).Distinct().ToList();
            int[] osTypeCountArray = new int[osTypes.Count];
            string[] osTypeColorArray = new string[osTypes.Count];

            for (int i = 0; i < osTypes.Count; i++)
            {
                osTypeCountArray[i] = articlesRead.Where(d => d.OSType == osTypes[i]).Count();
                osTypeColorArray[i] = GetRandomColor();
            }

            ViewBag.osTypes = osTypes.ToArray();
            ViewBag.osTypeCountArray = osTypeCountArray;
            ViewBag.osTypeColorArray = osTypeColorArray;
        }

        public void SetupPlatformReport(IQueryable<ArticleReporting> articlesRead)
        {
            //Need the count and name of each OS type put into arrays
            string[] platformTypes = new string[3] { "Desktop", "Mobile", "Other" };
            int[] platformTypesCountArray = new int[platformTypes.Length];
            string[] platformTypesColorArray = new string[platformTypes.Length];

            platformTypesCountArray[0] = articlesRead.Where(d => d.OSType.Contains("Windows") || d.OSType.Contains("Linux") || d.OSType.Contains("Mac")/* && d.ArticleReadTime <= dateFrom && d.ArticleReadTime >= dateTo*/).Count();
            platformTypesCountArray[1] = articlesRead.Where(d => d.OSType.Contains("Android") || d.OSType.Contains("iOS") /*&& d.ArticleReadTime <= dateFrom && d.ArticleReadTime >= dateTo*/).Count();
            platformTypesCountArray[2] = (articlesRead.Count() - platformTypesCountArray[0] - platformTypesCountArray[1]);

            for (int i = 0; i < platformTypes.Length; i++)
            {

                platformTypesColorArray[i] = GetRandomColor();
            }

            ViewBag.platformTypes = platformTypes.ToArray();
            ViewBag.platformTypesCountArray = platformTypesCountArray;
            ViewBag.platformTypesColorArray = platformTypesColorArray;
        }

        public void SetupViewsByLocation(IQueryable<ArticleReporting> articlesRead)
        {
            //This lists the views by user location
            List<string> locations = articlesRead/*.Where(d => d.ArticleReadTime <= dateFrom && d.ArticleReadTime >= dateTo)*/.Select(d => d.City).Distinct().ToList();
            //locations.RemoveAt(locations.Count-1);
            int[] locationsCountArray = new int[locations.Count];
            string[] locationsColorArray = new string[locations.Count];
            //List<ChartDataSets> cdsArray = new List<ChartDataSets>();

            for (int i = 0; i < locations.Count; i++)
            {

                locationsCountArray[i] = articlesRead.Where(d => d.City == locations[i]).Count();
                locationsColorArray[i] = GetRandomColor();
                if (locations[i] == null) locations[i] = "No Location Data";

                //ChartDataSets cds = new ChartDataSets();
                //cds.backgroundColor = locationsColorArray[i];
                //cds.borderColor = locationsColorArray[i];
                //cds.borderWidth = 1;
                //cds.data = new string[] { locationsCountArray[i].ToString() };
                //cds.label = locations[i];

                //cdsArray.Add(cds);               
            }
            //BarChartData bcd = new BarChartData();
            //bcd.labels = new string[] { "Users by City" };
            //bcd.datasets = cdsArray.ToArray<ChartDataSets>();
            //var x = JsonConvert.SerializeObject(bcd);
            //ViewBag.barChartDataStruct = x;
            ViewBag.locations = locations.ToArray();//.Skip(1).ToArray();
            ViewBag.locationsCountArray = locationsCountArray;//.Skip(1).ToArray();
            ViewBag.locationsColorArray = locationsColorArray;//.Skip(1).ToArray();
        }

        public void SetupViewsPerArticleReport(IQueryable<ArticleReporting> articlesRead)
        {
            //Need the count and name of each OS type put into arrays
            string[] articles = articlesRead/*.Where(d => d.ArticleReadTime <= dateFrom && d.ArticleReadTime >= dateTo)*/.Select(d => d.ArticleName).Distinct().ToArray();
            int[] articlesCountArray = new int[articles.Length];
            string[] articlesColorArray = new string[articles.Length];

            for (int i = 0; i < articles.Length; i++)
            {
                articlesCountArray[i] = articlesRead.Where(d => d.ArticleName == articles[i]).Count();
                articlesColorArray[i] = GetRandomColor();
            }

            ViewBag.articles = articles;
            ViewBag.articlesCountArray = articlesCountArray;
            ViewBag.articlesColorArray = articlesColorArray;
        }

        public class ChartDataSets
        {
            public string Label { get; set; }
            public string BackgroundColor { get; set; }
            public string BorderColor { get; set; }
            public int BorderWidth { get; set; }
            public string[] Data { get; set; }
        }

        public class BarChartData
        {
            public string[] Labels { get; set; }
            public ChartDataSets[] Datasets { get; set; }
        }

        public string GetRandomColor()
        {
            var random = new Random();
            var color = String.Format("#{0:X6}", random.Next(0x1000000));
            return color;
        }
    }
}
