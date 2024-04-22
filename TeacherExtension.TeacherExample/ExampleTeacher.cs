using MTM101BaldAPI.Registers;
using TeacherAPI;
using UnityEngine;

namespace TeacherExample
{
    public class ExampleTeacher : Teacher
    {
        // As Teacher inherits from Baldi, Make sure to override the method with this if you don't want his slap mechanic.
        // (If you forget, the teacher won't move at all)
        public override float DistanceCheck(float val) => val;

        public override void Initialize()
        {
            base.Initialize();

            var beans = (Beans)NPCMetaStorage.Instance.Get(Character.Beans).value;

            // Add a sound when CaughtPlayer is called
            AddLoseSound(beans.audNPCHitSounds[0], 100);

            // Replace the event texts (The ones naturally generated, doesn't works with mod menus) with custom
            ReplaceEventText<RulerEvent>("Beans lost his bubble gum.");
            ReplaceEventText<PartyEvent>("Party!!!!");

            // No arguments = No music
            ReplaceMusic();

            // Very important!!!
            behaviorStateMachine.ChangeState(new Example_Happy(this));
        }
    }

    /// <summary>
    /// A base for all your custom teacher state, you will need it if your teacher has custom fields.
    /// </summary>
    public class Example_StateBase : TeacherState
    {
        public Example_StateBase(ExampleTeacher example) : base(example)
        {
            this.example = example;
        }
        protected ExampleTeacher example;
    }

    /// <summary>
    /// The starting state the teacher will be at the start of the floor. Used as a gateway between Happy and Angry too.
    /// </summary>
    public class Example_Happy : Example_StateBase
    {
        public Example_Happy(ExampleTeacher example) : base(example) { }
        public override void PlayerExitedSpawn()
        {
            // Will change state when the player exits the spawn, like Baldi when starting to count.
            base.PlayerExitedSpawn();
            example.behaviorStateMachine.ChangeState(new Example_Chase(example));
            example.ActivateSpoopMode(); // Ctrl + Click the method name to see why it's important
        }
    }

    /// <summary>
    /// The angry state of the teacher.
    /// </summary>
    public class Example_Chase : Example_StateBase
    {
        public Example_Chase(ExampleTeacher example) : base(example) { }

        public override void Enter()
        {
            base.Enter();
            Debug.Log("IT's cRAZY TiME!");
            ChangeNavigationState(new NavigationState_WanderRandom(example, 0)); // For reference, PartyEvent has a priority of 31.
        }

        public override void Update()
        {
            base.Update();
            example.Navigator.SetSpeed(example.Navigator.Speed + Time.deltaTime * 100f);
        }


        public override void OnStateTriggerStay(Collider other)
        {
            // Obvious
            if (example.IsTouchingPlayer(other))
            {
                example.CaughtPlayer(other.GetComponent<PlayerManager>());
            }
        }
    }

}
