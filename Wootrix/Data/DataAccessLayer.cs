using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Internal;
using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Wootrix.Data;
using Microsoft.AspNetCore.Http.Features;
using WootrixV2.Models;
using System.Threading;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Options;
using Amazon.S3;
using Amazon;
using Amazon.S3.Transfer;

namespace WootrixV2.Data
{
    public class DataAccessLayer
    {

        private readonly ApplicationDbContext _context;
        private string _bucketName = "wootrixv2uploadfiles"; //this is my Amazon Bucket name

        public DataAccessLayer(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task UploadFileToS3(IFormFile file, string fileName, string fileFolder)
        {
            try
            {
                if (file.Length > 0)
                {
                    using (var client = new AmazonS3Client("AKIAIK65JJH7PZESEDCQ", "++/ANiW3uF3lPjMFf7knZk7cmmolxJUGaVw6fWxG", RegionEndpoint.USWest2))
                    {
                        using (var newMemoryStream = new MemoryStream())
                        {
                            file.CopyTo(newMemoryStream);

                            string bucketName = _bucketName + @"/" + (fileFolder ?? "");

                            var uploadRequest = new TransferUtilityUploadRequest
                            {
                                InputStream = newMemoryStream,
                                Key = fileName,
                                BucketName = bucketName,
                                CannedACL = S3CannedACL.PublicRead
                            };

                            var fileTransferUtility = new TransferUtility(client);
                            await fileTransferUtility.UploadAsync(uploadRequest);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.Message, e.InnerException);
            }
        }

        public IEnumerable<SelectListItem> GetCountries()
        {
            List<SelectListItem> item = _context.CompanyLocCountries.AsNoTracking()
                .OrderBy(n => n.country_name)
                    .Select(n =>
                    new SelectListItem
                    {
                        Value = n.country_code,
                        Text = n.country_name
                    }).ToList();
            var itemTop = new SelectListItem()
            {
                Value = null,
                Text = "--- Select Country ---"
            };
            item.Insert(0, itemTop);
            return new SelectList(item, "Value", "Text");
        }

        public IEnumerable<SelectListItem> GetNullStatesOrCities()
        {
            List<SelectListItem> item = new List<SelectListItem>();
            var itemTop = new SelectListItem()
            {
                Value = null,
                Text = "--- Select Country ---"
            };
            item.Insert(0, itemTop);
            return new SelectList(item, "Value", "Text");
        }

        public IEnumerable<SelectListItem> GetStates(string countyCode)
        {
            if (!String.IsNullOrWhiteSpace(countyCode))
            {
                IEnumerable<SelectListItem> states = _context.CompanyLocStates.AsNoTracking()
                    .OrderBy(n => n.state_name)
                    .Where(n => n.country_code == countyCode)
                    .Select(n =>
                       new SelectListItem
                       {
                           Value = n.state_code,
                           Text = n.state_name
                       }).ToList();
                return new SelectList(states, "Value", "Text");
            }
            return null;
        }


        public IEnumerable<SelectListItem> GetCities(string countryCode, string stateCode)
        {
            if (!String.IsNullOrWhiteSpace(stateCode))
            {
                IEnumerable<SelectListItem> cities = _context.CompanyLocCities.AsNoTracking()
                    .OrderBy(n => n.city_name_ascii)
                    .Where(n => n.country_code == countryCode)
                    .Where(n => n.state_code == stateCode)
                    .Select(n =>
                       new SelectListItem
                       {
                           Value = n.city_name_ascii,
                           Text = n.city_name_ascii
                       }).ToList();
                return new SelectList(cities, "Value", "Text");
            }
            return null;
        }



        public IEnumerable<SelectListItem> GetDepartments(int companyID)
        {
            //using auto disposes the object (contect - which we still need) so remove it
            //using (var context = _context)
            //{
            List<SelectListItem> deps = _context.CompanyDepartments.AsNoTracking()
            .Where(n => n.CompanyID == companyID)
                .OrderBy(n => n.CompanyDepartmentName)
                    .Select(n =>
                    new SelectListItem
                    {
                        Value = n.ID.ToString(),
                        Text = n.CompanyDepartmentName
                    }).ToList();
            var depstip = new SelectListItem()
            {
                Value = null,
                Text = "--- Select Department ---"
            };
            deps.Insert(0, depstip);
            return new SelectList(deps, "Value", "Text");
            //}
        }

        public IEnumerable<SelectListItem> GetInterfaceLanguages(int companyID, IOptions<RequestLocalizationOptions> rlo)
        {
            var cultureItems = rlo.Value.SupportedUICultures
                .Select(c => new SelectListItem { Value = c.DisplayName, Text = c.DisplayName })
                .ToList();

            return cultureItems;
        }


        public IEnumerable<SelectListItem> GetGenders()
        {
            List<SelectListItem> gen = new List<SelectListItem>();
            var gentip = new SelectListItem()
            {
                Value = "Not Identified",
                Text = "Not Identified"
            };
            var gen1 = new SelectListItem()
            {
                Value = "Male",
                Text = "Male"
            };
            var gen2 = new SelectListItem()
            {
                Value = "Female",
                Text = "Female"
            };
            var gen3 = new SelectListItem()
            {
                Value = "Other",
                Text = "Other"
            };
            gen.Insert(0, gentip);
            gen.Insert(1, gen1);
            gen.Insert(2, gen2);
            gen.Insert(3, gen3);
            return new SelectList(gen, "Value", "Text");

        }



        public IEnumerable<SelectListItem> GetCompanies()
        {
            //using (var context = _context)
            //{
            List<SelectListItem> deps = _context.Company.AsNoTracking()
                .OrderBy(n => n.CompanyName)
                    .Select(n =>
                    new SelectListItem
                    {
                        Value = n.ID.ToString(),
                        Text = n.CompanyName
                    }).ToList();
            var depstip = new SelectListItem()
            {
                Value = null,
                Text = "--- Select Company ---"
            };
            deps.Insert(0, depstip);
            return new SelectList(deps, "Value", "Text");
            //}
        }


        public List<SelectListItem> GetGroups(int companyID)
        {
            using (var context = _context)
            {
                List<SelectListItem> deps = context.CompanyGroups.AsNoTracking()
                .Where(n => n.CompanyID == companyID)
                    .OrderBy(n => n.GroupName)
                        .Select(n =>
                        new SelectListItem
                        {
                            Value = n.ID.ToString(),
                            Text = n.GroupName
                        }).ToList();
                var depstip = new SelectListItem()
                {
                    Value = null,
                    Text = "--- Select Groups ---"
                };
                deps.Insert(0, depstip);
                return deps;
            }
        }

        public List<SelectListItem> GetListGroups(int companyID)
        {
            List<SelectListItem> list = _context.CompanyGroups.AsNoTracking()
            .Where(n => n.CompanyID == companyID)
                .OrderBy(n => n.GroupName)
                    .Select(n =>
                    new SelectListItem
                    {
                        //Value = n.ID.ToString(),
                        Value = n.GroupName,
                        Text = n.GroupName
                    }).ToList();

            return list;
        }

        public List<SelectListItem> GetListTopics(int companyID)
        {
            List<SelectListItem> list = _context.CompanyTopics.AsNoTracking()
            .Where(n => n.CompanyID == companyID)
                .OrderBy(n => n.Topic)
                    .Select(n =>
                    new SelectListItem
                    {
                        Value = n.Topic,
                        Text = n.Topic
                    }).ToList();

            return list;
        }

        public List<SelectListItem> GetListTypeOfUser(int companyID)
        {
            List<SelectListItem> list = _context.CompanyTypeOfUser.AsNoTracking()
            .Where(n => n.CompanyID == companyID)
                .OrderBy(n => n.TypeOfUser)
                    .Select(n =>
                    new SelectListItem
                    {
                        Value = n.TypeOfUser,
                        Text = n.TypeOfUser
                    }).ToList();

            return list;
        }



        public List<SelectListItem> GetListLanguages(int companyID, IOptions<RequestLocalizationOptions> rlo)
        {

            List<SelectListItem> list = rlo.Value.SupportedUICultures
                .Select(c => new SelectListItem { Value = c.DisplayName, Text = c.DisplayName })
                .ToList();

            return list;
        }


        public List<SelectListItem> GetArticleSegments(int companyID)
        {
            List<SelectListItem> deps = _context.ArticleReporting.AsNoTracking()
            .Where(n => n.CompanyID == companyID)
                .OrderBy(n => n.SegmentName)
                    .Select(n =>
                    new SelectListItem
                    {
                        //Value = n.ID.ToString(),
                        Value = n.SegmentName,
                        Text = n.SegmentName
                    }).Distinct().ToList();

            return deps;

        }

        public List<SelectListItem> GetArticleNames(int companyID)
        {
            List<SelectListItem> deps = _context.ArticleReporting.AsNoTracking()
                .Where(n => n.CompanyID == companyID)
                .OrderBy(n => n.ID)
                .Select(n =>
                    new SelectListItem
                    {
                        //Value = n.ID.ToString(),
                        Value = n.ArticleName,
                        Text = n.ArticleName
                    }).Distinct().ToList();

            return deps;

        }


        public List<SelectListItem> GetArticleSegments(int companyID, string departmentID)
        {
            List<SelectListItem> deps = _context.CompanySegment.AsNoTracking().Distinct()
            .Where(n => n.CompanyID == companyID && n.Department == departmentID)
                .OrderBy(n => n.Title)
                    .Select(n =>
                    new SelectListItem
                    {
                        //Value = n.ID.ToString(),
                        Value = n.Title,
                        Text = n.Title
                    }).ToList();

            return deps;

        }

        

        public List<WootrixV2.Models.SegmentArticle> GetArticlesList(int companyID)
        {

            List<WootrixV2.Models.SegmentArticle> articles = _context.SegmentArticle.AsNoTracking()
            .Where(n => n.CompanyID == companyID)
                .OrderBy(n => n.Order)
                    .ToList();
            //TODO Add all the user filtering here
            return articles;

        }

        /// <summary>
        /// So pass 4 of the filtering rules:
        /// If user has no filters show all articles
        /// If user has filter x, any article shown must contain x in that group but it will show if it has other filters in that group also
        /// If user has filters x1 and x2, any article shown will have either x1 or x2
        /// If user has filters x and y set from different groups, any article shown must contain those filters in their respective groups
        /// </summary>
        /// <param name="usr"></param>
        /// <param name="articleSearchString"></param>
        /// <param name="seg"></param>
        /// <returns></returns>
        public List<WootrixV2.Models.SegmentArticle> GetArticlesListBasedOnThisUsersFilters(User usr, string articleSearchString, CompanySegment seg)
        {
            List<SegmentArticle> articles = new List<SegmentArticle>();
            try
            {
                var possibleArticles = _context.SegmentArticle.Where(n => n.CompanyID == usr.CompanyID && n.PublishFrom < DateTime.Now && n.Segments.Contains(seg.Title)).AsNoTracking();

                foreach (SegmentArticle art in possibleArticles)
                {
                    if (seg != null && !string.IsNullOrEmpty(seg.Title))
                    {
                        //If no filters for user all articles will show that have no filters. 
                        if (ArticleMatchesUserFilters(art, usr))
                        {
                            articles.Add(art);
                        }
                    }
                }

                //Allow for searches too           
                if (!string.IsNullOrEmpty(articleSearchString))
                {
                    articles = articles.Where(m => (m.Title.Contains(articleSearchString) || m.Tags.Contains(articleSearchString))).ToList();
                }
            }
            catch (Exception e)
            {
                Console.WriteLine("Retrieving articles for this user failed with inner exception: " + e);
            }

            // We need better ordering.
            if (articles != null) articles.OrderBy(m => m.Order);


            return articles.ToList();
        }



        public List<WootrixV2.Models.CompanySegment> GetSegmentsList(int companyID, User usr, string segmentSearchString, string articleSearchString)
        {
            // Have to do this backwards - get all the articles the user can see, then get a list of all segments which have articles
            List<CompanySegment> segments = new List<CompanySegment>();
            List<SegmentArticle> articles = new List<SegmentArticle>();

            // Our filters are Country, State, City, Language, Topics, Groups, TypeOfUser....also publish date

            //Allow for searches too
            var setOfArticles = _context.SegmentArticle.ToList();
            if (!string.IsNullOrEmpty(articleSearchString))
            {
                setOfArticles = _context.SegmentArticle.AsNoTracking()
                    .Where(n => n.CompanyID == companyID)
                    .Where(m => (m.Title.Contains(articleSearchString) || m.Tags.Contains(articleSearchString))).ToList();
            }

            foreach (SegmentArticle art in _context.SegmentArticle.Where(n => n.CompanyID == usr.CompanyID && n.PublishFrom < DateTime.Now))
            {
                //If no filters for user all articles will show that have no filters. If filter x on user, all articles with filter x will show. If filters x and y are set for user, any article with either x OR y will show.
                if (ArticleMatchesUserFilters(art, usr))
                {
                    articles.Add(art);
                }
            }

            // So now we have the articles that are valid for the user
            // We need to loop through them and for each Segement found, if it is not already in the list, add it
            foreach (SegmentArticle item in articles)
            {
                if (item.Segments != null)
                {

                    var articleSegments = item.Segments.Split('|').ToList();
                    foreach (var segmentTitle in articleSegments)
                    {
                        //If the article actually has a segment selected it passes here
                        if (!string.IsNullOrEmpty(segmentTitle))
                        {
                            var justSegTitle = segmentTitle.Split('/');
                            // Check if the segment title is in the existing segment list
                            var findSeg = segments.FirstOrDefault(p => p.Title == justSegTitle[0].ToString());

                            
                            if (findSeg == null)
                            {
                                // Not in list so add it
                                if (string.IsNullOrEmpty(segmentSearchString))
                                {
                                    // No search filter
                                    // If a magazine title is changed after an article is set to be listed in it the below query will fail. In that case if
                                    // we can't find the segment then skip this step
                                    var segementInContextWithListedTitle = _context.CompanySegment.FirstOrDefault(p => p.Title == justSegTitle[0].ToString());
                                    if (segementInContextWithListedTitle != null)
                                    {
                                        segments.Add(segementInContextWithListedTitle);
                                    }
                                }
                                else
                                {
                                    //Have to filter on search string too
                                    var seg = _context.CompanySegment.FirstOrDefault(p => p.Title == justSegTitle[0].ToString() && (p.Title.Contains(segmentSearchString) || p.Tags.Contains(segmentSearchString)));
                                    if (seg != null) segments.Add(seg);
                                }
                            }
                        }
                    }
                }
            }

            //TODO Add all the user and article filtering here
            return segments;
        }

        /// <summary>
        /// So pass 4 of the filtering rules:
        /// If user has no filters show all articles
        /// If user has filter x, any article shown must contain x in that group but it will show if it has other filters in that group also
        /// If user has filters x1 and x2, any article shown will have either x1 or x2
        /// If user has filters x and y set from different groups, any article shown must contain those filters in their respective groups
        /// <param name="art">Article to test</param>
        /// <param name="usr">User who's filters to compare to the passed article</param>
        /// <returns></returns>
        public bool ArticleMatchesUserFilters(SegmentArticle art, User usr)
        {
            bool matches = false;

            //If no filters for user all articles will show that have no filters.
            if (string.IsNullOrEmpty(usr.Groups) && string.IsNullOrEmpty(usr.TypeOfUser) && string.IsNullOrEmpty(usr.Topics) && string.IsNullOrEmpty(usr.WebsiteLanguage) && string.IsNullOrEmpty(usr.Country) && string.IsNullOrEmpty(usr.State) && string.IsNullOrEmpty(usr.City))
            {
                //So the user has absolutely no filtering set so we should show all articles
                matches = true;
            }
            
            else if (PassesFilter(art.Country, usr.Country) && PassesFilter(art.State, usr.State) && PassesFilter(art.City, usr.City)
                     && PassesFilter(art.Groups, usr.Groups) && PassesFilter(art.TypeOfUser, usr.TypeOfUser) && PassesFilter(art.Topics, usr.Topics) && PassesFilter(art.Languages, usr.WebsiteLanguage))
            {
                matches = true;
            }
            return matches;
        }

        /// <summary>
        /// So pass 4 of the filtering rules:
        /// 1 -If user has no filters show all articles 
        /// 2- If user has filter x, any article shown must contain x in that group but it will show if it has other filters in that group also
        /// 3- If user has filters x1 and x2, any article shown will have either x1 or x2
        /// 4- If user has filters x and y set from different groups, any article shown must contain those filters in their respective groups
        /// </summary>
        /// <param name="articleGroupFilters"></param>
        /// <param name="userGroupFilters"></param>
        /// <returns></returns>
        public bool PassesFilter(string articleGroupFilters, string userGroupFilters)
        {
            bool isInFilter = false;
            if (articleGroupFilters == null)
            {
                //can't split a null
                //no filter show all articles
                return true;
            }
            

            var articleGroupFilterList = articleGroupFilters.Split('|').ToList();
            foreach (var articleGroupFilterX in articleGroupFilterList)
            {
                // 1 - If the filter is "" or null for this User group then the article should not be blocked (If user has no filters show all articles)
                if (string.IsNullOrEmpty(articleGroupFilterX) || string.IsNullOrEmpty(userGroupFilters))
                {
                    return true;
                }

                //Otherwise a filter is set so we need to compare them
                if (!string.IsNullOrEmpty(articleGroupFilterX))
                {

                    if (userGroupFilters.Contains(articleGroupFilterX))
                    {                       
                        return true;
                    }
                }
            }

            return isInFilter;
        }

        public List<WootrixV2.Models.SegmentArticleComment> GetArticleCommentsList(int ArticleID)
        {

            List<WootrixV2.Models.SegmentArticleComment> comments = _context.SegmentArticleComment.AsNoTracking()
            .Where(n => n.SegmentArticleID == ArticleID && n.Status == "Approved")
                .OrderBy(n => n.CreatedDate)
                    .ToList();
            //TODO Add all the user and article filtering here
            return comments;
        }

        /// <summary>
        /// Get the approved comments count for the article 
        /// </summary>
        /// <param name="ArticleID">The article to get the comment count for</param>
        /// <returns></returns>
        public int GetArticleApprovedCommentCount(int ArticleID)
        {
            var commentCount = _context.SegmentArticleComment
                 .Where(m => m.SegmentArticleID == ArticleID && m.Status == "Approved")
                 .Count();
            return commentCount;
        }

        /// <summary>
        /// Get the "Review" status comments count for the company 
        /// </summary>
        /// <param name="ArticleID">The article to get the comment count for</param>
        /// <returns></returns>
        public int GetArticleReviewCommentCount(int CompanyID)
        {
            var commentCount = _context.SegmentArticleComment
                 .Where(m => m.CompanyID == CompanyID && m.Status == "Review")
                 .Count();
            return commentCount;
        }

        public string GetCompanyDepartmentName(string DepartmentID)
        {
            CompanyDepartments cd = _context.CompanyDepartments.AsNoTracking().FirstOrDefault(n => n.ID.ToString() == DepartmentID);
            return cd.CompanyDepartmentName;
        }

        public string GetCountryName(string CountryCode)
        {
            List<SelectListItem> item = _context.CompanyLocCountries.AsNoTracking()
               .OrderBy(n => n.country_name)
               .Where(n => n.country_code == CountryCode)
                   .Select(n =>
                   new SelectListItem
                   {
                       Value = n.country_code,
                       Text = n.country_name
                   }).ToList();
            var cz = _context.CompanyLocCountries.Find(CountryCode);
            var cq = _context.CompanyLocCountries.Where(m => m.country_code == CountryCode);
            var ct = _context.CompanyLocCountries.AsNoTracking().FirstOrDefault(m => m.country_code == CountryCode);
            return ct.country_name;
        }

        public string GetStateName(string StateCode)
        {
            var cs = _context.CompanyLocStates.AsNoTracking().FirstOrDefault(p => p.state_code == StateCode);
            return cs.state_name;
        }

        public void RemoveSegmentFromArticleSegments(SegmentArticle art, string segmentTitle)
        {
            var updatedSegmentsAndOrders = "";
            //Split the segments into a list, grab their order and increment it then save the change
            var segments = art.Segments;
            var segmentsList = art.Segments.Split("|");
            foreach (string segmentTitleAndOrder in segmentsList)
            {
                if (!segmentTitleAndOrder.Contains(segmentTitle))
                {

                    updatedSegmentsAndOrders += segmentTitleAndOrder;
                }
            }
            art.Segments = updatedSegmentsAndOrders;
            _context.Update(art);
            _context.SaveChanges();
        }

        // Decrement everything below it
        public void DeleteArticleAndUpdateOthersOrder(SegmentArticle article, string segmentTitle)
        {
            foreach (var art in _context.SegmentArticle.Where(m => m.CompanyID == article.CompanyID && m.Order > article.Order))
            {
                var updatedSegmentsAndOrders = "";
                //Split the segments into a list, grab their order and increment it then save the change
                var segments = art.Segments;
                var segmentsList = art.Segments.Split("|");
                foreach (string segmentTitleAndOrder in segmentsList)
                {
                    var ender = "";
                    // If this isn't the last title, add a delimited
                    if (segmentsList.Last() != segmentTitleAndOrder) ender = "|";

                    if (segmentTitleAndOrder.Contains(segmentTitle))
                    {
                        //Get the order and increment
                        var titleAndOrder = segmentTitleAndOrder.Split("/");
                        int ord;
                        bool success2 = int.TryParse(titleAndOrder[1], out ord);
                        ord--;
                        updatedSegmentsAndOrders += titleAndOrder[0] + "/" + ord + ender;
                    }
                    else
                    {
                        // just add it unchanged 
                        updatedSegmentsAndOrders += segmentTitleAndOrder + ender;
                    }
                }
                art.Segments = updatedSegmentsAndOrders;
                art.Order--;
                _context.Update(art);
            }

            // Remove the current segement and order from the passed article
            RemoveSegmentFromArticleSegments(article, segmentTitle);
            _context.SaveChanges();
        }

        // Decrement everything below it
        public void DeleteArticleAndUpdateOthersOrder(SegmentArticle article, string segmentTitle, int orderDeletedArticleIsAt)
        {

            List<SegmentArticle> segmentArticle = _context.SegmentArticle.Where(m => m.CompanyID == article.CompanyID && m.Segments.Contains(segmentTitle)).ToList();

            foreach (var art in segmentArticle)
            {
                var updatedSegmentsAndOrders = "";
                //Split the segments into a list, grab their order and increment it then save the change
                var segments = art.Segments;
                var segmentsList = art.Segments.Split("|");
                foreach (string segmentTitleAndOrder in segmentsList)
                {
                    var ender = "";
                    // If this isn't the last title, add a delimited
                    if (segmentsList.Last() != segmentTitleAndOrder) ender = "|";

                    if (segmentTitleAndOrder.Contains(segmentTitle))
                    {
                        //Get the order and increment
                        var titleAndOrder = segmentTitleAndOrder.Split("/");
                        int ord;
                        bool success2 = int.TryParse(titleAndOrder[1], out ord);
                        if (ord > orderDeletedArticleIsAt)
                        {
                            --ord;
                        }
                        updatedSegmentsAndOrders += titleAndOrder[0] + "/" + ord + ender;
                    }
                    else
                    {
                        // just add it unchanged 
                        updatedSegmentsAndOrders += segmentTitleAndOrder + ender;
                    }
                }
                art.Segments = updatedSegmentsAndOrders;
                _context.Update(art);
            }
        }

        public int GetCompanyNumberOfCurrentTotalUsers(int CompanyID)
        {
            var userCount = _context.User.AsNoTracking().Where(p => p.CompanyID == CompanyID).Count();
            return userCount;
        }

        public int GetCompanyNumberOfCurrentOnlyUsers(int CompanyID)
        {
            //Role 1 = admin
            //Role 2 = user
            //Role 3 = superadmin
            var userCount = _context.User.AsNoTracking().Where(p => p.CompanyID == CompanyID && p.Role.ToString() == "2").Count();
            return userCount;
        }

        public int GetCompanyNumberOfCurrentAdminUsers(int CompanyID)
        {
            //Role 1 = admin
            //Role 2 = user
            //Role 3 = superadmin
            var userCount = _context.User.AsNoTracking().Where(p => p.CompanyID == CompanyID && p.Role.ToString() == "1").Count();
            return userCount;
        }

        public int GetCompanyNumberOfUsedPushNotifications(int CompanyID)
        {            
            var pnCount = _context.Company.AsNoTracking().FirstOrDefault(p => p.ID == CompanyID).CompanyUsedPushNotifications;
            return pnCount;
        }


        public void UpdateCompanyUserCounts()
        {
            var companies = _context.Company.ToList();
            foreach (Company cp in companies)
            {
                var userCount = GetCompanyNumberOfCurrentOnlyUsers(cp.ID);
                cp.CompanyNumberOfCurrentUsers = userCount;
                
            }
            _context.SaveChanges();           
        }


        public List<CompanyPushNotification> GetNotificationsMatchingUserFilters(User usr)
        {
            var listOfCompanyNotifications = _context.CompanyPushNotification.Where(m => m.CompanyID == usr.CompanyID && m.SentAt > usr.LastViewedNotificationsOn).OrderByDescending(m => m.SentAt);
            List<CompanyPushNotification> listOfMatchingNotifications = new List<CompanyPushNotification>();

            foreach (CompanyPushNotification cpn in listOfCompanyNotifications)
            {

                bool matches = false;

                //If no filters for user all Notifications will show it
                if (string.IsNullOrEmpty(usr.Groups) && string.IsNullOrEmpty(usr.TypeOfUser) && string.IsNullOrEmpty(usr.Topics) && string.IsNullOrEmpty(usr.InterfaceLanguage) && string.IsNullOrEmpty(usr.Country) && string.IsNullOrEmpty(usr.State) && string.IsNullOrEmpty(usr.City))
                {
                    //So the user has absolutely no filtering set so we should show all Notification
                    matches = true;
                }

                else if (PassesFilter(cpn.Country, usr.Country) && PassesFilter(cpn.State, usr.State) && PassesFilter(cpn.City, usr.City)
                         && PassesFilter(cpn.Groups, usr.Groups) && PassesFilter(cpn.TypeOfUser, usr.TypeOfUser) && PassesFilter(cpn.Topics, usr.Topics) && PassesFilter(cpn.Languages, usr.InterfaceLanguage))
                {
                    matches = true;
                }


                if (matches) listOfMatchingNotifications.Add(cpn);

            }
            return listOfMatchingNotifications;
        }
    }
}
