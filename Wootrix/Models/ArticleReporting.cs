using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WootrixV2.Models
{
    public class ArticleReporting
    {
        [Key]
        [ScaffoldColumn(false)]
        public int ID { get; set; }

        [ScaffoldColumn(false)]
        public int CompanyID { get; set; }

        [ScaffoldColumn(false)]
        public int UserID { get; set; }

        [Display(Name = "User Name")]
        public string UserName { get; set; }

        [ScaffoldColumn(false)]
        public int SegmentID { get; set; }

        [Display(Name = "Magazine Name")]
        public string SegmentName { get; set; }

        [ScaffoldColumn(false)]
        public int ArticleID { get; set; }

        [Display(Name = "Article Name")]
        public string ArticleName { get; set; }

        [Display(Name = "Device Type")]
        public string DeviceType { get; set; }

        [Display(Name = "OS Type")]
        public string OSType { get; set; }

        [DisplayFormat(DataFormatString = "{0:yyyy MMM dd}")]
        [Display(Name = "Article Viewed On")]
        public DateTime ArticleReadTime { get; set; }

        [Display(Name = "Country", Prompt = "Country", Description = "Country")]
        public string Country { get; set; }

        [Display(Name = "State", Prompt = "State", Description = "State")]
        public string State { get; set; }

        [Display(Name = "City", Prompt = "City", Description = "City")]
        public string City { get; set; }

        [ScaffoldColumn(false)]
        public float Latitude { get; set; }

        [ScaffoldColumn(false)]
        public float Longitude { get; set; }


    }

    public class FilterData 
    {
        [ScaffoldColumn(false)]
        public int ID { get; set; }

        [ScaffoldColumn(false)]
        public int CompanyID { get; set; }

        [ScaffoldColumn(false)]
        public string CompanyName { get; set; }

        [Display(Name = "Magazine Name", Prompt = "Magazine Name", Description = "Magazine Name")]
        public string Segments { get; set; }
        public IList<string> SelectedSegments { get; set; }
        public IList<SelectListItem> AvailableSegments { get; set; }

        [Display(Name = "Article Name", Prompt = "Article Name", Description = "Article Name")]
        public string Articles { get; set; }
        public IList<string> SelectedArticles { get; set; }
        public IList<SelectListItem> AvailableArticles { get; set; }

        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{0:MM/dd/yyyy}")]
        public DateTime? FromDate { get; set; }
        //[Required(ErrorMessage = "Date is required")]
        [DataType(DataType.Date)]
        [DisplayFormat(DataFormatString = "{1:MM/dd/yyyy}")]
        public DateTime? ToDate { get; set; }

        [Display(Name = "Interface Language")]
        public string InterfaceLanguage { get; set; }
        public IEnumerable<SelectListItem> InterfaceLanguages { get; set; }

        [Required]
        [Display(Name = "Role", Prompt = "Role", Description = "Role")]
        public Roles Role { get; set; }

        //[DataType(DataType.PhoneNumber)]
        //[StringLength(15, ErrorMessage = "The {0} must be at max {1} characters long.")]
        //[RegularExpression(@"^\(?([0-9]{3})\)?[-. ]?([0-9]{3})[-. ]?([0-9]{4})$", ErrorMessage = "Not a valid phone number")]
        [Display(Name = "Phone Number", Prompt = "Phone Number", Description = "Phone Number")]
        public string PhoneNumber { get; set; }


        [Display(Name = "Gender", Prompt = "Gender", Description = "Gender")]
        public string Gender { get; set; }
        public IEnumerable<SelectListItem> Genders { get; set; }

        [Display(Name = "Article Languages")]
        public string WebsiteLanguage { get; set; }
        public IList<string> SelectedLanguages { get; set; }
        public IList<SelectListItem> AvailableLanguages { get; set; }

        [ScaffoldColumn(false)]
        public string WebsiteLanguageID { get; set; }

        [Display(Name = "Limit editors to this Department", Prompt = "Limit editors to this Department", Description = "Limit editors to this Department")]
        public string Categories { get; set; }
        public IEnumerable<SelectListItem> Departments { get; set; }

        [Display(Name = "User groups", Prompt = "User groups", Description = "User groups")]
        public string Groups { get; set; }
        public IList<string> SelectedGroups { get; set; }
        public IList<SelectListItem> AvailableGroups { get; set; }

        [Display(Name = "User Topics", Prompt = "User Topics", Description = "User Topics")]
        public string Topics { get; set; }
        public IList<string> SelectedTopics { get; set; }
        public IList<SelectListItem> AvailableTopics { get; set; }

        [Display(Name = "Type Of User", Prompt = "Type Of User", Description = "Type Of User")]
        public string TypeOfUser { get; set; }
        public IList<string> SelectedTypeOfUser { get; set; }
        public IList<SelectListItem> AvailableTypeOfUser { get; set; }

        [Display(Name = "User Country", Prompt = "User Country", Description = "User Country")]
        public string Country { get; set; }
        public IEnumerable<SelectListItem> Countries { get; set; }

        [Display(Name = "State", Prompt = "State", Description = "State")]
        public string State { get; set; }
        public IEnumerable<SelectListItem> States { get; set; }

        [Display(Name = "City", Prompt = "City", Description = "City")]
        public string City { get; set; }
        public IEnumerable<SelectListItem> Cities { get; set; }

        public FilterData()
        {
            InterfaceLanguages =new List<SelectListItem>();

            SelectedSegments = new List<string>();
            AvailableSegments = new List<SelectListItem>();

            SelectedArticles = new List<string>();
            AvailableArticles = new List<SelectListItem>();

            SelectedGroups = new List<string>();
            AvailableGroups = new List<SelectListItem>();

            SelectedTopics = new List<string>();
            AvailableTopics = new List<SelectListItem>();

            SelectedTypeOfUser = new List<string>();
            AvailableTypeOfUser = new List<SelectListItem>();

            SelectedLanguages = new List<string>();
            AvailableLanguages = new List<SelectListItem>();

            Countries = new List<SelectListItem>();
            States = new List<SelectListItem>();
            Cities = new List<SelectListItem>();

        }
    }

    public class Magazine
    {
        public int SegmentId { get; set; }
        public string SegmentName { get; set; }
        public int Previews { get; set; }
        public int percentageMagazine { get; set; }
    }
    public class Article
    {
        public int ArticleID { get; set; }
        public string ArticleName { get; set; }
        public int Previews { get; set; }
        public int percentageArticle { get; set; }
    }

    public class Topics
    {
        public int TopicID { get; set; }
        public string TopiceName { get; set; }
        public int Previews { get; set; }
        public int percentageTopic { get; set; }
    }

    public class Groups
    {
        public int GroupID { get; set; }
        public string GroupName { get; set; }
        public int Previews { get; set; }
        public int percentageGroup { get; set; }
    }

    public class Users
    {
        public int UserID { get; set; }
        public string UserName { get; set; }
        public int Previews { get; set; }
        public int percentageUser { get; set; }
    }

    public class Languages
    {
        public int LanguageID { get; set; }
        public string LanguageName { get; set; }
        public int Previews { get; set; }
        public int percentageLanguage { get; set; }
    }

    public class City
    {
        public int CityID { get; set; }
        public string Citys { get; set; }
        public int Previews { get; set; }
        public int percentageCity { get; set; }
    }

    public class States
    {
        public int StateID { get; set; }
        public string State { get; set; }
        public int Previews { get; set; }
        public int percentageState { get; set; }
    }

    public class Countrys
    {
        public int CountryID { get; set; }
        public string Country { get; set; }
        public int Previews { get; set; }
        public int percentageCountry { get; set; }
    }

}







