using System.Collections.Generic;

namespace SecureStudentManagement.Models
{
    public class PagedLearnerViewModel
    {
        public List<Learner> Learners { get; set; }
        public int CurrentPage { get; set; }
        public int TotalPages { get; set; }
        public string Search { get; set; }
    }
}