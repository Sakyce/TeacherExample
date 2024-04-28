using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Components;
using System.Linq;
using System.Numerics;
using TeacherAPI;
using TeacherAPI.utils;
using TeacherExample;
using UnityEngine;

namespace TeacherExtension.Viktor
{
    public class Alice : Teacher
    {
        public override float DistanceCheck(float val) => val;
        public new CustomSpriteAnimator animator;

        internal static AssetManager AssetManager = new AssetManager();
        public static SoundObject[] quotes;
        public static Sprite[] walkcycle;

        public override WeightedTeacherNotebook GetTeacherNotebookWeight() => new WeightedTeacherNotebook(this)
            .Weight(100)
            .Sprite(AssetManager.Get<Sprite>("notebook"))
            .CollectSound(AssetManager.Get<SoundObject>("notebookjingle"));

        public static void LoadAssets()
        {
            AudioClip Clip(params string[] path) => AssetLoader.AudioClipFromMod(ViktorPlugin.Instance, path);
            SoundObject Voiceline(AudioClip clip, string subtitle) => ObjectCreators.CreateSoundObject(clip, subtitle, SoundType.Voice, Color.white);

            quotes = new SoundObject[]
            {
                // I'm not writing the subtitles, any contributors welcome
                Voiceline(Clip("alice", "quote1.ogg"), ""),
                Voiceline(Clip("alice", "quote2.ogg"), ""),
                Voiceline(Clip("alice", "quote3.ogg"), ""),
                Voiceline(Clip("alice", "quote4.ogg"), ""),
                Voiceline(Clip("alice", "quote5.ogg"), ""),
                Voiceline(Clip("alice", "quote6.ogg"), ""),
                Voiceline(Clip("alice", "quote7.ogg"), ""),
                Voiceline(Clip("alice", "quote8.ogg"), ""),
                Voiceline(Clip("alice", "quote9.ogg"), ""),
                Voiceline(Clip("alice", "quote10.ogg"), ""),
                Voiceline(Clip("alice", "quote11.ogg"), ""),
                Voiceline(Clip("alice", "quote12.ogg"), ""),
                Voiceline(Clip("alice", "quote13.ogg"), "")
            };

            walkcycle = TeacherPlugin.TexturesFromMod(ViktorPlugin.Instance, "alice/walk{0}.png", (1, 4)).ToSprites(25f);
            AssetManager.Add("happy", AssetLoader.TextureFromMod(ViktorPlugin.Instance, "alice", "default.png").ToSprite(25f));

            AssetManager.Add("transform1", Voiceline(Clip("alice", "transform1.ogg"), ""));
            AssetManager.Add("transform2", Voiceline(Clip("alice", "transform2.ogg"), ""));
            AssetManager.Add("school", ObjectCreators.CreateSoundObject(Clip("alice", "school.mp3"), "", SoundType.Music, Color.white, 0));

            AssetManager.Add("scream", ObjectCreators.CreateSoundObject(Clip("alice", "jumpscare.ogg"), "", SoundType.Effect, Color.white, 0));
            AssetManager.Add("notebook", AssetLoader.TextureFromMod(ViktorPlugin.Instance, "notebook", "history.png").ToSprite(30f));
            AssetManager.Add("notebookjingle", ObjectCreators.CreateSoundObject(Clip("notebook", "history_jingle.ogg"), "Notebook!", SoundType.Effect, Color.white, 0));
        }

        public override void Initialize()
        {
            base.Initialize();
            caughtOffset += new UnityEngine.Vector3(0, 1, 0);
            animator.animations.Add("happy", new CustomAnimation<Sprite>(new Sprite[] { AssetManager.Get<Sprite>("happy") }, 1));
            animator.animations.Add("walk", new CustomAnimation<Sprite>(walkcycle.ToArray(), 1));
            animator.animations.Add("angry", new CustomAnimation<Sprite>(walkcycle.ToArray() , 1));
            animator.Play("happy", 1f);
            animator.SetDefaultAnimation("happy", 1f);

            navigator.Entity.SetHeight(5.5f);

            ReplaceMusic(AssetManager.Get<SoundObject>("school"));
            AddLoseSound(AssetManager.Get<SoundObject>("scream"), 100);
        }

        public override TeacherState GetAngryState() => new Alice_Chase(this);
        public override TeacherState GetHappyState() => new Alice_Happy(this);
    }

    class Alice_StateBase : TeacherState
    {
        public Alice_StateBase(Alice alice) : base(alice)
        {
            this.alice = alice;
        }
        protected Alice alice;
    }
    class Alice_Happy : Alice_StateBase
    {
        public Alice_Happy(Alice alice) : base(alice) { }
        private bool isTransforming = false;
        public override void Enter()
        {
            base.Enter();
            alice.Navigator.SetSpeed(0f);
            alice.Navigator.maxSpeed = 0;
            alice.animator.SetDefaultAnimation("happy", 1);
            alice.animator.Play("happy", 1);
        }
        public override void NotebookCollected(int currentNotebooks, int maxNotebooks)
        {
            base.NotebookCollected(currentNotebooks, maxNotebooks);
            alice.ReplaceMusic();
            alice.ActivateSpoopMode();
            
            if (!alice.IsHelping())
            {
                alice.AudMan.QueueAudio(Alice.AssetManager.Get<SoundObject>("transform1"));
                isTransforming = true;
            } else
            {
                alice.behaviorStateMachine.ChangeState(new Alice_Chase(alice));
            }
        }
        public override void Update()
        {
            base.Update();
            if (isTransforming && !alice.AudMan.AnyAudioIsPlaying)
            {
                alice.AudMan.QueueAudio(Alice.AssetManager.Get<SoundObject>("transform2"));
                alice.behaviorStateMachine.ChangeState(new Alice_Chase(alice));
            }
        }
    }
    class Alice_Chase : Alice_StateBase
    {
        public Alice_Chase(Alice alice) : base(alice) { }
        public override void Enter()
        {
            base.Enter();
            alice.animator.SetDefaultAnimation("walk", 1);
            alice.animator.Play("walk", 1);
            alice.Navigator.SetSpeed(18f);
            alice.UpdateSoundTarget();
        }
        public override void PlayerInSight(PlayerManager player)
        {
            base.PlayerInSight(player);
            alice.Hear(player.transform.position, 127, false);
        }
        public override void PlayerSighted(PlayerManager player)
        {
            base.PlayerSighted(player);
            alice.AudMan.QueueRandomAudio(Alice.quotes);
        }
        public override void OnStateTriggerStay(Collider other)
        {
            if (alice.IsTouchingPlayer(other))
            {
                alice.CaughtPlayer(other.GetComponent<PlayerManager>());
            }
        }
        public override void DestinationEmpty()
        {
            base.DestinationEmpty();
            alice.UpdateSoundTarget();
        }
        public override void NotebookCollected(int currentNotebooks, int maxNotebooks)
        {
            base.NotebookCollected(currentNotebooks, maxNotebooks);
            alice.Hear(alice.players[0].transform.position, 127);
        }
    }
}
