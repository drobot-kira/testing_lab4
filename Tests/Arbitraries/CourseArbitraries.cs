using FsCheck;
using FsCheck.Fluent;
using Online_Course_Enrollment_System_Properties.Domain;

namespace Online_Course_Enrollment_System_Properties.Tests.Arbitraries
{
    public static class CourseArbitraries
    {
        private const string StudentPrefix = "S";
        private const string CoursePrefix = "C";

        public static Arbitrary<Student> Student()
        {
            var idGen = Gen.Choose(1, 20).Select(n => $"{StudentPrefix}{n}");
            var studentGen = from id in idGen
                select new Student(id);

            return Arb.From(studentGen);
        }

        public static Arbitrary<Course> Course()
        {
            var idGen = Gen.Choose(1, 20).Select(n => $"{CoursePrefix}{n}");
            var capacityGen = Gen.Frequency(
                (3, Gen.Constant(1)),
                (2, Gen.Constant(2)),
                (5, Gen.Choose(1, 30)));

            var courseGen = from id in idGen
                from capacity in capacityGen
                select new Course(id, capacity);

            return Arb.From(courseGen);
        }
    }
}