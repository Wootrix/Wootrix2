﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Wootrix.Data;
using WootrixV2.Data;
using WootrixV2.Models;
using Microsoft.AspNetCore.Authorization;

namespace WootrixV2.Controllers
{
    [Authorize]
    public class CompanySegmentsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IHostingEnvironment _env;
        private readonly string _rootpath;
        private readonly UserManager<ApplicationUser> _userManager;
        private ApplicationUser _user;
        private int _companyID;

        public CompanySegmentsController(UserManager<ApplicationUser> userManager, IHostingEnvironment env, ApplicationDbContext context)
        {
            _context = context;
            _env = env;
            _rootpath = _env.WebRootPath;
            _userManager = userManager;

        }


        public async Task<IActionResult> ChangeArticleOrder(string id)
        {
            var orderArray = id.Split("|");
            var order = orderArray[0].ToString();
            bool success = Int32.TryParse(orderArray[1].ToString(), out int articleID);

            var _companyID = HttpContext.Session.GetInt32("CompanyID") ?? 0;
            var article = await _context.SegmentArticle.FindAsync(articleID);
            int whereItCurrentlyIs = article.Order ?? 0;
            int whereItIsMovingTo;
            success = int.TryParse(order, out whereItIsMovingTo);

            var segID = HttpContext.Session.GetInt32("SegmentID") ?? 0;
            CompanySegment cs = await _context.CompanySegment.FirstOrDefaultAsync(m => m.ID == segID && m.CompanyID == _companyID);

            var segmentArticle = _context.SegmentArticle
                .Where(n => n.CompanyID == cs.CompanyID)
                .Where(m => m.Segments.Contains(cs.Title));

            // So for each segment with an order greater than the order we need to increment the order number
            if (whereItCurrentlyIs < whereItIsMovingTo)
            {
                foreach (var art in segmentArticle.Where(m => m.CompanyID == _companyID && ((m.Order ?? 0) <= whereItIsMovingTo) && (m.Order ?? 0) > whereItCurrentlyIs))
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

                        if (segmentTitleAndOrder.Contains(cs.Title))
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
            }
            else
            {
                foreach (var art in segmentArticle.Where(m => m.CompanyID == _companyID && ((m.Order ?? 0) >= whereItIsMovingTo) && (m.Order ?? 0) < whereItCurrentlyIs))
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

                        if (segmentTitleAndOrder.Contains(cs.Title))
                        {
                            //Get the order and increment
                            var titleAndOrder = segmentTitleAndOrder.Split("/");
                            int ord;
                            bool success2 = int.TryParse(titleAndOrder[1], out ord);
                            ord++;
                            updatedSegmentsAndOrders += titleAndOrder[0] + "/" + ord + ender;

                        }
                        else
                        {
                            // just add it unchanged 
                            updatedSegmentsAndOrders += segmentTitleAndOrder + ender;
                        }
                    }
                    art.Segments = updatedSegmentsAndOrders;
                    art.Order++;
                    _context.Update(art);
                }
            }            
            // Update the order of the original Article as well
            article.Order = whereItIsMovingTo;

            var uso = "";          
            var segList = article.Segments.Split("|");
            foreach (string sto in segList)
            {
                var ender = "";
                // If this isn't the last title, add a delimited
                if (segList.Last() != sto) ender = "|";
                if (sto.Contains(cs.Title))
                {
                    //Get the order and increment
                    var to = sto.Split("/");
                    uso += to[0] + "/" + whereItIsMovingTo + ender;
                }
                else
                {
                    // just add it unchanged 
                    uso += sto + ender;
                }
            }
            article.Segments = uso;


            _context.Update(article);
            _context.SaveChanges();

            return RedirectToAction("Details", "CompanySegments", new { id = segID }); 
        }


        public async Task<IActionResult> ChangeOrder(string id)
        {
            var orderArray = id.Split("|");
            var order = orderArray[0].ToString();
            int segmentID;
            bool success = Int32.TryParse(orderArray[1].ToString(), out segmentID);
            _companyID = HttpContext.Session.GetInt32("CompanyID") ?? 0;           
            var segment = await _context.CompanySegment.FindAsync(segmentID);
            int whereItCurrentlyIs = segment.Order ?? 0;
            int whereItIsMovingTo;
            success = Int32.TryParse(order, out whereItIsMovingTo);

            // So for each segment with an order greater than the order we need to increment the order number
            if (whereItCurrentlyIs < whereItIsMovingTo)
            {
                foreach (var seg in _context.CompanySegment.Where(m => m.CompanyID == _companyID && ((m.Order ?? 0) <= whereItIsMovingTo) && (m.Order ?? 0) > whereItCurrentlyIs))
                {
                    seg.Order--;
                    _context.Update(seg);
                }
            }
            else
            {
                foreach (var seg in _context.CompanySegment.Where(m => m.CompanyID == _companyID && ((m.Order ?? 0) >= whereItIsMovingTo) && (m.Order ?? 0) < whereItCurrentlyIs))
                {
                    seg.Order++;
                    _context.Update(seg);
                }
            }           
            // Update the order of the segmentID to be as passed
            segment.Order = whereItIsMovingTo;
            _context.Update(segment);
            _context.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        // Decrement everything else
        public void InsertAtOrder1()
        {
            var _companyID = HttpContext.Session.GetInt32("CompanyID") ?? 0;
            foreach (var seg in _context.CompanySegment.Where(m => m.CompanyID == _companyID))
            {
                seg.Order++;
                _context.Update(seg);
            }
            _context.SaveChanges();
        }

        // Decrement everything else
        public void InsertAtOrder1(int oldOrder)
        {
            // If the magazine was at position 5 we only need to decrement the order
            // of magazines 1-4
            var _companyID = HttpContext.Session.GetInt32("CompanyID") ?? 0;
            foreach (var seg in _context.CompanySegment.Where(m => m.CompanyID == _companyID))
            {
                if (seg.Order < oldOrder)
                {
                    seg.Order++;
                    _context.Update(seg);
                }
            }
            _context.SaveChanges();
        }

        // Decrement everything below it
        public void DeleteSegmentAndUpdateOthersOrder(int deletedSegmentOrder)
        {
            var _companyID = HttpContext.Session.GetInt32("CompanyID") ?? 0;
            foreach (var seg in _context.CompanySegment.Where(m => m.CompanyID == _companyID && m.Order > deletedSegmentOrder))
            {
                seg.Order--;
                _context.Update(seg);
            }
            _context.SaveChanges();
        }


        // GET: CompanySegments
        public async Task<IActionResult> UserSegmentSearch(string SearchString)
        {
            _companyID = HttpContext.Session.GetInt32("CompanyID") ?? 0;

            DatabaseAccessLayer dla = new DatabaseAccessLayer(_context);

            _user = _userManager.GetUserAsync(User).GetAwaiter().GetResult();
            WootrixV2.Models.User un = _context.User.Where(x => x.EmailAddress == _user.Email).FirstOrDefaultAsync().GetAwaiter().GetResult();
            var segments = dla.GetSegmentsList(_companyID, un, SearchString, "");


            return View(segments);
        }


        // GET: CompanySegments
        public async Task<IActionResult> Index()
        {
            //Get the company name out the session and use it as a filter for the groups returned

            // We also need to filter on department.
            // So if a segment is set to only be Editable by Company Admins of Department IT, 
            // then if the current Company Admin is in Department Marketing they wont see it.
            // Note that if the segment doesn't have a department set, everyone sees it


            _companyID = HttpContext.Session.GetInt32("CompanyID") ?? 0;

            // Get current user department
            _user = _userManager.GetUserAsync(User).GetAwaiter().GetResult();
            WootrixV2.Models.User un = _context.User.Where(x => x.EmailAddress == _user.Email).FirstOrDefaultAsync().GetAwaiter().GetResult();

            var department = un.Categories; //bad naming for the old DB i know
            List<CompanySegment> ctx;

            // If the user doesn't have a department don't filter on it
            if (!string.IsNullOrWhiteSpace(department))
            {
                ctx = await _context.CompanySegment
                  .Where(m => m.CompanyID == _companyID)
                  .Where(m => m.Department == department)
                  .OrderBy(m => m.Order)
                  .ToListAsync();
            }
            else
            {
                ctx = await _context.CompanySegment
                .Where(m => m.CompanyID == _companyID)
                .OrderBy(m => m.Order)
                .ToListAsync();
            }

            return View(ctx);
        }

        // GET: SegmentArticles/Articlelist/id of segment
        public async Task<IActionResult> ArticleList(string id)
        {
            if (id == null)
            {
                return NotFound();
            }

            HttpContext.Session.SetString("SegmentListID", id);
            _companyID = HttpContext.Session.GetInt32("CompanyID") ?? 0;
            var segmentArticle = _context.SegmentArticle
                .Where(n => n.CompanyID == _companyID)
                .Where(m => m.Segments.Contains(id))
                .OrderBy(p => p.Order);

            //Also add the Segment to the Viewbag so we can get the Image
            CompanySegment cs = await _context.CompanySegment.FirstOrDefaultAsync(m => m.Title == id && m.CompanyID == _companyID);
            ViewBag.Segment = cs;
            if (segmentArticle == null)
            {
                return NotFound();
            }

            return View(await segmentArticle.ToListAsync());
        }

        // GET: SegmentArticles/Articlelist/id of segment
        public async Task<IActionResult> UserArticleSearch(string searchString)
        {
            if (searchString == null)
            {
                return NotFound();
            }

            var segmentTitle = HttpContext.Session.GetString("SegmentListID");
            _companyID = HttpContext.Session.GetInt32("CompanyID") ?? 0;

            var segmentArticle = _context.SegmentArticle
                .Where(n => n.CompanyID == _companyID)
                .Where(m => m.Segments.Contains(segmentTitle) && (m.Title.Contains(searchString) || m.Tags.Contains(searchString)));

            //Also add the Segment to the Viewbag so we can get the Image
            CompanySegment cs = await _context.CompanySegment.FirstOrDefaultAsync(m => m.Title == segmentTitle && m.CompanyID == _companyID);
            ViewBag.Segment = cs;
            if (segmentArticle == null)
            {
                return NotFound();
            }

            return View(await segmentArticle.ToListAsync());
        }

        // GET: CompanySegments/Details/5
        public async Task<IActionResult> Details(int id)
        {
            HttpContext.Session.SetInt32("SegmentID", id);
            _companyID = HttpContext.Session.GetInt32("CompanyID") ?? 0;
            CompanySegment cs = await _context.CompanySegment.FirstOrDefaultAsync(m => m.ID == id && m.CompanyID == _companyID);

            var segmentArticle = _context.SegmentArticle
                .Where(n => n.CompanyID == cs.CompanyID)
                .Where(m => m.Segments.Contains(cs.Title));

            // OK so now we are going to set the article Order field to be as show in the Segments so its easier to work with. 
            // We will need to save the segments back again if there is a change

            foreach (SegmentArticle item in segmentArticle)
            {
                //Split the segments into a list, grab their order and increment it then save the change
                var segments = item.Segments;
                var segmentsList = item.Segments.Split("|");
                foreach (string segmentTitleAndOrder in segmentsList)
                {
                    if (segmentTitleAndOrder.Contains(cs.Title))
                    {
                        //Get the order and increment
                        var titleAndOrder = segmentTitleAndOrder.Split("/");
                        int ord;
                        bool success = int.TryParse(titleAndOrder[1], out ord);

                        // Set the article Order
                        item.Order = ord;
                    }
                }
                _context.Update(item);
            }

            _context.SaveChanges();
            segmentArticle = _context.SegmentArticle
                .Where(n => n.CompanyID == cs.CompanyID)
                .Where(m => m.Segments.Contains(cs.Title))
                .OrderBy(p => p.Order);

            //Also add the Segment to the Viewbag so we can get the Image
            ViewBag.Segment = cs;

            if (segmentArticle == null)
            {
                return NotFound();
            }

            return View(await segmentArticle.ToListAsync());
        }

        // GET: CompanySegments/Create
        public IActionResult Create()
        {
            _user = _userManager.GetUserAsync(User).GetAwaiter().GetResult();
            CompanySegmentViewModel s = new CompanySegmentViewModel();
            s.Order = 1;
            s.PublishDate = DateTime.Now.Date;
            s.FinishDate = DateTime.Now.AddYears(10).Date;
            s.StandardColor = HttpContext.Session.GetString("CompanyHeaderBackgroundColor");
            s.ThemeColor = HttpContext.Session.GetString("CompanyHighlightColor");
            s.ClientName = _user.name;
            //s.ClientLogoImage = _user.photoUrl;
            var cp = _user.companyID;

            DatabaseAccessLayer dla = new DatabaseAccessLayer(_context);
            s.Departments = dla.GetDepartments(cp);
            return View(s);
        }

        // POST: CompanySegments/Create
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CompanySegmentViewModel cps)
        {
            _user = _userManager.GetUserAsync(User).GetAwaiter().GetResult();
            var companyID = HttpContext.Session.GetInt32("CompanyID") ?? 0;
            //Initialise a new companysegment
            var mySegment = new CompanySegment();
            if (ModelState.IsValid)
            {
                //ID,Order,Title,CoverImage,CoverImageMobileFriendly,PublishDate,FinishDate,ClientName,ClientLogoImage,ThemeColor,StandardColor,Draft,Department,Tags
                mySegment.CompanyID = companyID;
                mySegment.Order = cps.Order ?? 1;
                mySegment.Title = cps.Title;
                mySegment.PublishDate = cps.PublishDate;
                mySegment.FinishDate = cps.FinishDate;
                mySegment.ClientName = cps.ClientName;
                mySegment.ThemeColor = cps.ThemeColor;
                mySegment.StandardColor = cps.StandardColor;
                mySegment.Draft = DateTime.Now > cps.PublishDate ? false : true;
                mySegment.Department = cps.Department;
                mySegment.Tags = cps.Tags;
                mySegment.ClientName = cps.ClientName ?? _user.name;

                

                IFormFile coverImage = cps.CoverImage;
                if (coverImage != null)
                {
                    var filePath = Path.Combine(_rootpath, "images/Uploads", _user.companyName + "_" + coverImage.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await coverImage.CopyToAsync(stream);
                    }
                    //The file has been saved to disk - now save the file name to the DB
                    mySegment.CoverImage = coverImage.FileName;
                }

                IFormFile coverImageMB = cps.CoverImageMobileFriendly;
                if (coverImageMB != null)
                {
                    var filePath = Path.Combine(_rootpath, "images/Uploads", _user.companyName + "_" + coverImageMB.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await coverImageMB.CopyToAsync(stream);
                    }
                    //The file has been saved to disk - now save the file name to the DB
                    mySegment.CoverImageMobileFriendly = coverImageMB.FileName;
                }

                IFormFile cli = cps.ClientLogoImage;
                if (cli != null)
                {
                    var filePath = Path.Combine(_rootpath, "images/Uploads", _user.companyName + "_" + cli.FileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await cli.CopyToAsync(stream);
                    }
                    //The file has been saved to disk - now save the file name to the DB
                    mySegment.CoverImageMobileFriendly = cli.FileName;
                }
                InsertAtOrder1();
                _context.Add(mySegment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            DatabaseAccessLayer dla = new DatabaseAccessLayer(_context);
            cps.Departments = dla.GetDepartments(companyID);
            return View(cps);
        }

        // GET: CompanySegments/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var companySegment = await _context.CompanySegment.FindAsync(id);
            if (companySegment == null)
            {
                return NotFound();
            }

            _user = _userManager.GetUserAsync(User).GetAwaiter().GetResult();
            CompanySegmentViewModel s = new CompanySegmentViewModel();

            s.Order = companySegment.Order;
            s.Title = companySegment.Title;
            s.PublishDate = companySegment.PublishDate;
            s.FinishDate = companySegment.FinishDate;
            s.StandardColor = companySegment.StandardColor;
            s.ThemeColor = companySegment.ThemeColor;
            s.ClientName = (companySegment.ClientName == null ? _user.name : companySegment.ClientName);
            // s.ClientLogoImage = FormFileHelper.PhysicalToIFormFile(new FileInfo(companySegment.ClientLogoImage));
            s.Department = companySegment.Department;
            s.Tags = companySegment.Tags;

            DatabaseAccessLayer dla = new DatabaseAccessLayer(_context);
            s.Departments = dla.GetDepartments(_user.companyID);
            return View(s);
        }

        // POST: CompanySegments/Edit/5
        // To protect from overposting attacks, please enable the specific properties you want to bind to, for 
        // more details see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, CompanySegmentViewModel cps)
        {
            if (id != cps.ID)
            {
                return NotFound();
            }

            _user = _userManager.GetUserAsync(User).GetAwaiter().GetResult();
            //Initialise a new companysegment
            CompanySegment mySegment = await _context.CompanySegment.FindAsync(id);
            
            //Have to check the Segment title isn't already used - it gets weird otherwise
            var existingSeg = _context.SegmentArticle.FirstOrDefault(n => n.Title == cps.Title && n.CompanyID == cps.CompanyID);
            if (existingSeg == null)
            {
                if (ModelState.IsValid)
                {
                    //ID,Order,Title,CoverImage,CoverImageMobileFriendly,PublishDate,FinishDate,ClientName,ClientLogoImage,ThemeColor,StandardColor,Draft,Department,Tags
                    mySegment.CompanyID = _user.companyID;

                   
                    mySegment.PublishDate = cps.PublishDate;
                    mySegment.FinishDate = cps.FinishDate;
                    mySegment.ClientName = cps.ClientName;
                    mySegment.ThemeColor = cps.ThemeColor;
                    mySegment.StandardColor = cps.StandardColor;
                    mySegment.Draft = DateTime.Now > cps.PublishDate ? false : true;
                    mySegment.Department = cps.Department;
                    mySegment.Tags = cps.Tags;

                    IFormFile coverImage = cps.CoverImage;
                    if (coverImage != null)
                    {
                        var filePath = Path.Combine(_rootpath, "images/Uploads", _user.companyName + "_" + coverImage.FileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await coverImage.CopyToAsync(stream);
                        }
                        //The file has been saved to disk - now save the file name to the DB
                        mySegment.CoverImage = coverImage.FileName;
                    }

                    IFormFile coverImageMB = cps.CoverImageMobileFriendly;
                    if (coverImageMB != null)
                    {
                        var filePath = Path.Combine(_rootpath, "images/Uploads", _user.companyName + "_" + coverImageMB.FileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await coverImageMB.CopyToAsync(stream);
                        }
                        //The file has been saved to disk - now save the file name to the DB
                        mySegment.CoverImageMobileFriendly = coverImageMB.FileName;
                    }

                    IFormFile cli = cps.ClientLogoImage;
                    if (cli != null)
                    {
                        var filePath = Path.Combine(_rootpath, "images/Uploads", _user.companyName + "_" + cli.FileName);
                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await cli.CopyToAsync(stream);
                        }
                        //The file has been saved to disk - now save the file name to the DB
                        mySegment.CoverImageMobileFriendly = cli.FileName;
                    }


                    try
                    {
                        // Done later to avoid ordering failures if the image upload fails.
                        if (mySegment.Order != 1)
                        {
                            // We only need to decrement articles above it (lower order)
                            InsertAtOrder1(mySegment.Order ?? 1);
                        }
                        mySegment.Order = 1;

                        // Ok but what about the connected articles? We need to update every article segments field and change the old val to the new

                        // If the title changed
                        if (mySegment.Title != cps.Title)
                        {
                            foreach (var art in _context.SegmentArticle.Where(m=> m.CompanyID == _user.companyID))
                            {
                                if (art.Segments.Contains(mySegment.Title))
                                {
                                    var oldSegments = art.Segments;
                                    var newSegments = oldSegments.Replace(mySegment.Title, cps.Title);
                                    art.Segments = newSegments;
                                    _context.Update(art);
                                }
                            }
                        }                     

                        mySegment.Title = cps.Title;

                        _context.Update(mySegment);
                        await _context.SaveChangesAsync();
                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!CompanySegmentExists(cps.ID))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                    return RedirectToAction(nameof(Index));
                }
            }
            else
            {
                // Article title already exists
                ModelState.AddModelError(string.Empty, "Magazine Title already exists - please choose something unique");
            }

            return RedirectToAction("Edit", new { id = cps.ID });
        }

        // GET: CompanySegments/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var companySegment = await _context.CompanySegment
                .FirstOrDefaultAsync(m => m.ID == id);
            if (companySegment == null)
            {
                return NotFound();
            }

            return View(companySegment);
        }

        // POST: CompanySegments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var companySegment = await _context.CompanySegment.FindAsync(id);
            //Update order of other segments
            DeleteSegmentAndUpdateOthersOrder(companySegment.Order ?? 1);
            _context.CompanySegment.Remove(companySegment);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CompanySegmentExists(int id)
        {
            return _context.CompanySegment.Any(e => e.ID == id);
        }
    }
}
