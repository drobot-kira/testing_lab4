using FluentAssertions;
using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using Online_Course_Enrollment_System_Properties.Domain;
using Online_Course_Enrollment_System_Properties.Tests.Arbitraries;

namespace Online_Course_Enrollment_System_Properties.Tests.Properties
{
    public class ServiceProperties
    {
        // P5 · Оракул
        [Property(Arbitrary = new[] { typeof(CourseArbitraries) })]
        public void Service_enroll_matches_direct_course_enroll(Course course, Student[] students)
        {
            // Arrange
            var repo = new EnrollmentRepository();
            var service = new EnrollmentService(repo);
            service.CreateCourse(course.Id, course.Capacity);

            var directCourse = new Course(course.Id, course.Capacity);
            var serviceResults = new List<bool>();
            var directResults = new List<bool>();

            // Act
            foreach (var student in students)
            {
                serviceResults.Add(service.Enroll(course.Id, student));
                directResults.Add(directCourse.Enroll(student));
            }

            // Assert
            var savedCourse = repo.Get(course.Id);
            serviceResults.Should().Equal(directResults);
            savedCourse.Students.Count.Should().Be(directCourse.Students.Count);
            savedCourse.FreeSeats.Should().Be(directCourse.FreeSeats);
        }

        // P7 · Легко перевірити
        [Property(Arbitrary = new[] { typeof(CourseArbitraries) })]
        public void Total_enrollments_matches_successful_enroll_count(Course[] courses, Student[] students)
        {
            // Arrange
            var repo = new EnrollmentRepository();
            var service = new EnrollmentService(repo);
            var uniqueCourses = courses.DistinctBy(c => c.Id).ToList();

            foreach (var course in uniqueCourses)
            {
                service.CreateCourse(course.Id, course.Capacity);
            }

            var successfulCount = 0;

            // Act
            foreach (var course in uniqueCourses)
            {
                foreach (var student in students)
                {
                    if (service.Enroll(course.Id, student))
                    {
                        successfulCount++;
                    }
                }
            }

            // Assert
            service.TotalEnrollments().Should().Be(successfulCount);
        }
        
        // Перевірка винятку
        [Property(Arbitrary = new[] { typeof(CourseArbitraries) })]
        public void Create_duplicate_course_throws_invalid_operation(Course course)
        {
            // Arrange
            var repo = new EnrollmentRepository();
            var service = new EnrollmentService(repo);
            service.CreateCourse(course.Id, course.Capacity);

            // Act
            Action act = () => service.CreateCourse(course.Id, course.Capacity);

            // Assert
            act.Should().Throw<InvalidOperationException>();
        }

        // Перевірка винятку
        [Property(Arbitrary = new[] { typeof(CourseArbitraries) })]
        public void Enroll_to_unknown_course_throws_key_not_found(Course course, Student student)
        {
            // Arrange
            var repo = new EnrollmentRepository();
            var service = new EnrollmentService(repo);

            // Act
            Action act = () => service.Enroll(course.Id, student);

            // Assert
            act.Should().Throw<KeyNotFoundException>();
        }
    }
}