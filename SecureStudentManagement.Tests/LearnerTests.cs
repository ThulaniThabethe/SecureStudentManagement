using Xunit;
using SecureStudentManagement.Models;

namespace SecureStudentManagement.Tests
{
    public class LearnerTests
    {
        [Fact]
        public void Learner_Should_Create_Valid_Object()
        {
            var learner = new Learner
            {
                FirstName = "Anele",
                LastName = "Zulu",
                Email = "Anelezulu123@test.com",
                MobileNumber = "+27693130017",
                EnrolmentStatus = "Active"
            };

            Assert.Equal("Anele", learner.FirstName);
        }
    }
}