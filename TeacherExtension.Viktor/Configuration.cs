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
        public static ConfigEntry<int> ViktorSpawnWeight { get; internal set; }
        public static ConfigEntry<int> AliceSpawnWeight { get; internal set; }

        internal static void Setup()
        {
            ViktorSpawnWeight = ViktorPlugin.Instance.Config.Bind(
                "Viktor",
                "SpawnWeight",
                100,
                "More it is higher, more there is a chance of him spawning. (Defaults to 100. For comparison, Baldi weight is 100) (Requires Restart)"
            );
            AliceSpawnWeight = ViktorPlugin.Instance.Config.Bind(
                "Alice",
                "SpawnWeight",
                100,
                "More it is higher, more there is a chance of him spawning. (Defaults to 100. For comparison, Baldi weight is 100) (Requires Restart)"
            );
        }
    }
}
