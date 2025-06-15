using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using PharmaPlus.Models;

namespace PharmaPlus.Controllers
{
    public class AboutController : Controller
    {
        private PharmaContext db = new PharmaContext();

        public ActionResult Index()
        {
            try
            {
                var viewModel = new AboutViewModel
                {
                    CompanyStats = GetCompanyStats(),
                    TeamMembers = GetTeamMembers(),
                    Achievements = GetAchievements(),
                    Timeline = GetCompanyTimeline(),
                    Partners = GetPartners()
                };
                
                return View(viewModel);
            }
            catch (Exception ex)
            {
                // Log error
                System.Diagnostics.Debug.WriteLine("Error in AboutController.Index: " + ex.Message);
                
                // Return simple view for testing
                ViewBag.ErrorMessage = ex.Message;
                return View(GetDefaultModel());
            }
        }

        private AboutViewModel GetDefaultModel()
        {
            return new AboutViewModel
            {
                CompanyStats = new CompanyStats
                {
                    YearsOfExperience = 15,
                    HappyCustomers = 50000,
                    ProductsSold = 250000,
                    PharmacistsCount = 25,
                    LocationsCount = 12,
                    CertificationsCount = 8
                },
                TeamMembers = GetTeamMembers(),
                Achievements = GetAchievements(),
                Timeline = GetCompanyTimeline(),
                Partners = GetPartners()
            };
        }

        private CompanyStats GetCompanyStats()
        {
            try
            {
                return new CompanyStats
                {
                    YearsOfExperience = 15,
                    HappyCustomers = db.Users.Count(u => u.role == "customer"),
                    ProductsSold = 250000,
                    PharmacistsCount = 25,
                    LocationsCount = 12,
                    CertificationsCount = 8
                };
            }
            catch
            {
                return new CompanyStats
                {
                    YearsOfExperience = 15,
                    HappyCustomers = 50000,
                    ProductsSold = 250000,
                    PharmacistsCount = 25,
                    LocationsCount = 12,
                    CertificationsCount = 8
                };
            }
        }

        private List<TeamMember> GetTeamMembers()
        {
            return new List<TeamMember>
            {
                new TeamMember
                {
                    Name = "Dược sĩ Nguyễn Văn An",
                    Position = "Giám đốc điều hành",
                    Experience = "15 năm kinh nghiệm",
                    Specialty = "Dược lâm sàng, Quản lý chuỗi cung ứng",
                    ImageUrl = "https://images.unsplash.com/photo-1612349317150-e413f6a5b16d?w=400&h=400&fit=crop&crop=face",
                    LinkedInUrl = "#",
                    FacebookUrl = "#"
                },
                new TeamMember
                {
                    Name = "Dược sĩ Trần Thị Bình",
                    Position = "Trưởng phòng Kiểm định",
                    Experience = "12 năm kinh nghiệm",
                    Specialty = "Kiểm định chất lượng, An toàn dược phẩm",
                    ImageUrl = "https://images.unsplash.com/photo-1594824575303-76d95efa61d6?w=400&h=400&fit=crop&crop=face",
                    LinkedInUrl = "#",
                    FacebookUrl = "#"
                },
                new TeamMember
                {
                    Name = "Dược sĩ Lê Minh Cường",
                    Position = "Chuyên gia Tư vấn",
                    Experience = "10 năm kinh nghiệm",
                    Specialty = "Tư vấn dược, Chăm sóc khách hàng",
                    ImageUrl = "https://images.unsplash.com/photo-1622253692010-333f2da6031d?w=400&h=400&fit=crop&crop=face",
                    LinkedInUrl = "#",
                    FacebookUrl = "#"
                },
                new TeamMember
                {
                    Name = "Dược sĩ Phạm Thị Dung",
                    Position = "Trưởng phòng R&D",
                    Experience = "8 năm kinh nghiệm",
                    Specialty = "Nghiên cứu phát triển, Công nghệ dược",
                    ImageUrl = "https://images.unsplash.com/photo-1559839734-2b71ea197ec2?w=400&h=400&fit=crop&crop=face",
                    LinkedInUrl = "#",
                    FacebookUrl = "#"
                }
            };
        }

        private List<Achievement> GetAchievements()
        {
            return new List<Achievement>
            {
                new Achievement
                {
                    Year = 2023,
                    Title = "Top 10 Nhà thuốc tin cậy nhất",
                    Organization = "Hiệp hội Dược sĩ Việt Nam",
                    IconClass = "fas fa-trophy"
                },
                new Achievement
                {
                    Year = 2022,
                    Title = "Giải thưởng Chất lượng Quốc gia",
                    Organization = "Bộ Y tế",
                    IconClass = "fas fa-medal"
                },
                new Achievement
                {
                    Year = 2021,
                    Title = "Doanh nghiệp xuất sắc ngành Dược",
                    Organization = "Phòng Thương mại Việt Nam",
                    IconClass = "fas fa-star"
                },
                new Achievement
                {
                    Year = 2020,
                    Title = "Chứng nhận ISO 9001:2015",
                    Organization = "Tổ chức Quốc tế ISO",
                    IconClass = "fas fa-certificate"
                }
            };
        }

        private List<TimelineItem> GetCompanyTimeline()
        {
            return new List<TimelineItem>
            {
                new TimelineItem
                {
                    Year = 2008,
                    Title = "Thành lập công ty",
                    Description = "PharmaPlus được thành lập với sứ mệnh mang đến dịch vụ dược phẩm chất lượng cao."
                },
                new TimelineItem
                {
                    Year = 2012,
                    Title = "Mở rộng chuỗi cửa hàng",
                    Description = "Khai trương 5 cửa hàng đầu tiên tại TP.HCM và Hà Nội."
                },
                new TimelineItem
                {
                    Year = 2018,
                    Title = "Ra mắt nền tảng trực tuyến",
                    Description = "Phát triển website và ứng dụng mobile để phục vụ khách hàng tốt hơn."
                },
                new TimelineItem
                {
                    Year = 2020,
                    Title = "Đạt chứng nhận ISO",
                    Description = "Đạt chứng nhận ISO 9001:2015 về hệ thống quản lý chất lượng."
                },
                new TimelineItem
                {
                    Year = 2023,
                    Title = "Mở rộng toàn quốc",
                    Description = "Hiện có 12 chi nhánh trên toàn quốc và phục vụ hơn 100,000 khách hàng."
                }
            };
        }

        private List<Partner> GetPartners()
        {
            return new List<Partner>
            {
                new Partner { Name = "Teva Pharmaceutical", LogoUrl = "https://via.placeholder.com/150x80/3B82F6/FFFFFF?text=TEVA" },
                new Partner { Name = "Pfizer", LogoUrl = "https://via.placeholder.com/150x80/3B82F6/FFFFFF?text=PFIZER" },
                new Partner { Name = "Johnson & Johnson", LogoUrl = "https://via.placeholder.com/150x80/3B82F6/FFFFFF?text=J%26J" },
                new Partner { Name = "Novartis", LogoUrl = "https://via.placeholder.com/150x80/3B82F6/FFFFFF?text=NOVARTIS" },
                new Partner { Name = "Abbott", LogoUrl = "https://via.placeholder.com/150x80/3B82F6/FFFFFF?text=ABBOTT" },
                new Partner { Name = "GSK", LogoUrl = "https://via.placeholder.com/150x80/3B82F6/FFFFFF?text=GSK" }
            };
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }

    // ViewModels
    public class AboutViewModel
    {
        public CompanyStats CompanyStats { get; set; }
        public List<TeamMember> TeamMembers { get; set; }
        public List<Achievement> Achievements { get; set; }
        public List<TimelineItem> Timeline { get; set; }
        public List<Partner> Partners { get; set; }

        public AboutViewModel()
        {
            CompanyStats = new CompanyStats();
            TeamMembers = new List<TeamMember>();
            Achievements = new List<Achievement>();
            Timeline = new List<TimelineItem>();
            Partners = new List<Partner>();
        }
    }

    public class CompanyStats
    {
        public int YearsOfExperience { get; set; }
        public int HappyCustomers { get; set; }
        public int ProductsSold { get; set; }
        public int PharmacistsCount { get; set; }
        public int LocationsCount { get; set; }
        public int CertificationsCount { get; set; }
    }

    public class TeamMember
    {
        public string Name { get; set; }
        public string Position { get; set; }
        public string Experience { get; set; }
        public string Specialty { get; set; }
        public string ImageUrl { get; set; }
        public string LinkedInUrl { get; set; }
        public string FacebookUrl { get; set; }
    }

    public class Achievement
    {
        public int Year { get; set; }
        public string Title { get; set; }
        public string Organization { get; set; }
        public string IconClass { get; set; }
    }

    public class TimelineItem
    {
        public int Year { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
    }

    public class Partner
    {
        public string Name { get; set; }
        public string LogoUrl { get; set; }
    }
}