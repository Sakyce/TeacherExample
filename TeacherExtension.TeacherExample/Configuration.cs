using BepInEx.Configuration;

namespace TeacherExample
{
    public class Configuration
    {
        /// <summary>
        /// The odds of the teacher spawning. Compared to the other teachers.
        /// <para>
        /// Examples : 
        /// <code>
        /// Baldi : 100
        /// Foxo  : 100
        /// Null  : 20
        /// </code>
        /// </para>
        /// </summary>
        public static ConfigEntry<int> SpawnWeight { get; internal set; }

        internal static void Setup()
        {
            SpawnWeight = ExampleTeacherPlugin.Instance.Config.Bind(
                "ExampleTeacher", 
                "SpawnWeight", 
                10000,
                "More it is higher, more there is a chance of him spawning. (Defaults to 100. For comparison, Baldi weight is 100) (Requires Restart)"
            );
        }
    }
}
