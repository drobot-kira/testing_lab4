using FluentAssertions;
using FsCheck;
using FsCheck.Fluent;
using FsCheck.Xunit;
using Online_Course_Enrollment_System_Properties.Domain;
using Online_Course_Enrollment_System_Properties.Tests.Arbitraries;

namespace Online_Course_Enrollment_System_Properties.Tests.Properties
{
    public class CourseProperties
    {
        // P2 · Інваріант
        [Property(Arbitrary = new[] { typeof(CourseArbitraries) })]
        public void Enrolled_students_never_exceed_capacity(Course course, Student[] students)
        {
            // Arrange
            var targetCourse = new Course(course.Id, course.Capacity);

            // Act
            foreach (var student in students)
            {
                targetCourse.Enroll(student);
            }

            // Assert
            targetCourse.Students.Count.Should().BeInRange(0, targetCourse.Capacity);
            targetCourse.FreeSeats.Should().BeGreaterThanOrEqualTo(0);
        }

        // P3 · Ідемпотентність
        [Property(Arbitrary = new[] { typeof(CourseArbitraries) })]
        public void Enrolling_same_student_twice_is_idempotent(Course course, Student student)
        {
            // Arrange
            var targetCourse = new Course(course.Id, course.Capacity);
            var firstResult = targetCourse.Enroll(student);
            var countAfterFirst = targetCourse.Students.Count;

            // Act
            var secondResult = targetCourse.Enroll(student);

            // Assert
            firstResult.Should().BeTrue();
            secondResult.Should().BeFalse();
            targetCourse.Students.Count.Should().Be(countAfterFirst);
        }

        // P8 · Без винятків
        [Property(Arbitrary = new[] { typeof(CourseArbitraries) })]
        public void Enroll_non_null_student_never_throws(Course course, Student[] students)
        {
            // Arrange
            var targetCourse = new Course(course.Id, course.Capacity);

            // Act
            Action act = () =>
            {
                foreach (var student in students)
                {
                    targetCourse.Enroll(student);
                }
            };

            // Assert
            act.Should().NotThrow();
        }
        
        // Перевірка винятку
        [Property(Arbitrary = new[] { typeof(CourseArbitraries) })]
        public void Unenroll_missing_student_throws_invalid_operation(Course course, Student student)
        {
            // Arrange
            var targetCourse = new Course(course.Id, course.Capacity);

            // Act
            Action act = () => targetCourse.Unenroll(student.Id);

            // Assert
            act.Should().Throw<InvalidOperationException>();
        }
        
        // P1 · Туди й назад
        [Property(Arbitrary = new[] { typeof(CourseArbitraries) })]
        public Property Enroll_then_unenroll_restores_course_state(
            Course course,
            Student existing,
            Student target)
        {
            // Arrange
            var targetCourse = new Course(course.Id, course.Capacity);
            targetCourse.Enroll(existing);

            bool applicable = !targetCourse.IsFull && !targetCourse.IsEnrolled(target.Id);
            bool ok = true;

            // Act & Assert
            if (applicable)
            {
                var countBefore = targetCourse.Students.Count;
                var freeSeatsBefore = targetCourse.FreeSeats;

                var enrolled = targetCourse.Enroll(target);
                targetCourse.Unenroll(target.Id);

                ok = enrolled
                     && targetCourse.Students.Count == countBefore
                     && targetCourse.FreeSeats == freeSeatsBefore
                     && !targetCourse.IsEnrolled(target.Id);
            }

            return ok.When(applicable);
        }

        // P2 · Інваріант
        [Property(Arbitrary = new[] { typeof(CourseArbitraries) })]
        public Property Enroll_when_course_is_full_returns_false(
            Course course,
            Student[] initialStudents,
            Student extraStudent)
        {
            // Arrange
            var targetCourse = new Course(course.Id, course.Capacity);
            foreach (var student in initialStudents)
            {
                targetCourse.Enroll(student);
            }

            bool applicable = targetCourse.IsFull;
            bool ok = true;

            // Act & Assert
            if (applicable)
            {
                var countBefore = targetCourse.Students.Count;

                var result = targetCourse.Enroll(extraStudent);

                ok = !result && targetCourse.Students.Count == countBefore;
            }

            return ok.When(applicable);
        }
    }
}