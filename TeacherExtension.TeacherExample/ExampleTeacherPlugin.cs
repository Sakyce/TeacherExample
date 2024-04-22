using BepInEx;
using MTM101BaldAPI;
using MTM101BaldAPI.Registers;
using TeacherAPI;
using static BepInEx.BepInDependency;

namespace TeacherExample
{
    [BepInPlugin("local.baldiplus.teacherextension.example", "Example Teacher for MoreTeachers", "1.0.0.0")]

    [BepInDependency("sakyce.baldiplus.teacherapi", DependencyFlags.HardDependency)]
    [BepInDependency("mtm101.rulerp.bbplus.baldidevapi", DependencyFlags.HardDependency)]
    public class ExampleTeacherPlugin : BaseUnityPlugin
    {
        public ExampleTeacher ExampleTeacher { get; private set; }
        public static ExampleTeacherPlugin Instance { get; private set; }

        internal void Awake()
        {
            Instance = this;
            Configuration.Setup();
            //TeacherPlugin.RequiresAssetsFolder(this); // Uncomment this when you'll use custom audios/sprites!!!
            LoadingEvents.RegisterOnAssetsLoaded(OnassetsLoaded, false);
        }

        private void OnassetsLoaded()
        {
            // Create a NPC using MTMModdingAPI
            var teacher = ObjectCreators.CreateNPC<ExampleTeacher>(
                "ExampleTeacher",
                EnumExtensions.ExtendEnum<Character>("ExampleTeacher"),
                NPCMetaStorage.Instance.Get(Character.Beans).value.Poster // Check the source code of TeacherAPI 
            );

            // Add it to the meta storage and register it to TeacherAPI (very important!)
            NPCMetaStorage.Instance.Add(new NPCMetadata(Info, new NPC[] { teacher }, "ExampleTeacher", NPCFlags.Standard));
            TeacherPlugin.RegisterTeacher(teacher);
            ExampleTeacher = teacher;

            // Register the teacher to the generator
            GeneratorManagement.Register(this, GenerationModType.Addend, EditGenerator);
        }

        private void EditGenerator(string floorName, int floorNumber, LevelObject floorObject)
        {
            // It is good practice to check if the level starts with F to make sure to not clash with other mods.
            // INF stands for Infinite Floor
            // END stands for Endless mode
            // F* stands for Vanilla
            if (floorName.StartsWith("F") || floorName.StartsWith("END") || floorName.Equals("INF"))
            {
                // The SpawnWeight comes from the configuration. Dont hardcode it, it's not cool :(
                floorObject.AddPotentialTeacher(ExampleTeacher, Configuration.SpawnWeight.Value);
                print($"Added {ExampleTeacher.Character} to {floorName} (Floor {floorNumber})");
            }
        }
    }
}
