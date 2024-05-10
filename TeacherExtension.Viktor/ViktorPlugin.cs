using BepInEx;
using HarmonyLib;
using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Components;
using MTM101BaldAPI.ObjectCreation;
using MTM101BaldAPI.Registers;
using TeacherAPI;
using TeacherExtension.Viktor;
using static BepInEx.BepInDependency;

namespace TeacherExample
{
    [BepInPlugin("sakyce.baldiplus.teacherextension.viktor", "Viktor Teacher for MoreTeachers", "1.0.0.0")]

    [BepInDependency("sakyce.baldiplus.teacherapi", DependencyFlags.HardDependency)]
    [BepInDependency("mtm101.rulerp.bbplus.baldidevapi", DependencyFlags.HardDependency)]
    public class ViktorPlugin : BaseUnityPlugin
    {
        public Viktor Viktor { get; private set; }
        public Alice Alice { get; private set; }
        public static ViktorPlugin Instance { get; private set; }

        internal void Awake()
        {
            Instance = this;
            Configuration.Setup();
            TeacherPlugin.RequiresAssetsFolder(this); 
            LoadingEvents.RegisterOnAssetsLoaded(OnAssetsLoaded, false);
            new Harmony("sakyce.baldiplus.teacherextension.viktor").PatchAllConditionals();
        }

        private void OnAssetsLoaded()
        {
            Viktor.LoadAssets();
            Alice.LoadAssets();

            var viktor = new NPCBuilder<Viktor>(Info)
                .SetName("Viktor")
                .SetEnum("Viktor")
                .SetPoster(ObjectCreators.CreatePosterObject(new UnityEngine.Texture2D[] { AssetLoader.TextureFromMod(this, "viktor", "poster.png") }))
                .AddLooker()
                .AddTrigger()
                .SetMinMaxAudioDistance(0, 500)
                .SetMetaTags(new string[] { "Teacher" })
                .Build();
            viktor.audMan = viktor.GetComponent<AudioManager>();
            CustomSpriteAnimator animator = viktor.gameObject.AddComponent<CustomSpriteAnimator>();
            animator.spriteRenderer = viktor.spriteRenderer[0];
            viktor.animator = animator;

            TeacherPlugin.RegisterTeacher(viktor);
            Viktor = viktor;

            var alice = new NPCBuilder<Alice>(Info)
                .SetName("Alice")
                .SetEnum("Alice")
                .SetPoster(ObjectCreators.CreateCharacterPoster(AssetLoader.TextureFromMod(this, "alice", "poster.png"), "Alice", "She loves her students! But she doesn't feels well lately..."))
                .AddLooker()
                .AddTrigger()
                .SetMinMaxAudioDistance(0, 500)
                .SetMetaTags(new string[] { "Teacher" })
                .Build();
            alice.audMan = alice.GetComponent<AudioManager>();
            CustomSpriteAnimator animator2 = alice.gameObject.AddComponent<CustomSpriteAnimator>();
            animator2.spriteRenderer = alice.spriteRenderer[0];
            alice.animator = animator2;

            TeacherPlugin.RegisterTeacher(alice);
            Alice = alice;

            // Register the teacher to the generator
            GeneratorManagement.Register(this, GenerationModType.Addend, EditGenerator);
        }

        private void EditGenerator(string floorName, int floorNumber, LevelObject floorObject)
        {
            if (floorName.StartsWith("F") || floorName.StartsWith("END") || floorName.Equals("INF"))
            {
                floorObject.AddPotentialTeacher(Viktor, Configuration.ViktorSpawnWeight.Value);
                floorObject.AddPotentialTeacher(Alice, Configuration.AliceSpawnWeight.Value);
                floorObject.AddPotentialAssistingTeacher(Viktor, Configuration.ViktorSpawnWeight.Value);
                floorObject.AddPotentialAssistingTeacher(Alice, Configuration.AliceSpawnWeight.Value);
                print($"Added {Viktor.Character} to {floorName} (Floor {floorNumber})");
                print($"Added {Alice.Character} to {floorName} (Floor {floorNumber})");
            }
        }
    }
}
