using MTM101BaldAPI;
using MTM101BaldAPI.AssetTools;
using MTM101BaldAPI.Components;
using MTM101BaldAPI.Registers;
using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Schema;
using TeacherAPI;
using TeacherExtension.Viktor.Patches;
using UnityEngine;
using UnityEngine.SocialPlatforms;
using UnityEngine.UIElements;

namespace TeacherExample
{
    public class Viktor : Teacher
    {
        // As Teacher inherits from Baldi, Make sure to override the method with this if you don't want his slap mechanic.
        // (If you forget, the teacher won't move at all)
        public override float DistanceCheck(float val) => val;

        public bool IsQuiet { get; set; }
        internal ViktorTilePollutionManager PollutionManager { get; private set; }

        public new CustomSpriteAnimator animator;
        public bool firstTimeJacketTriggered = false;
        public override WeightedTeacherNotebook GetTeacherNotebookWeight() => new WeightedTeacherNotebook(this)
            .Weight(100)
            .Sprite(AssetManager.Get<Sprite>("notebook"))
            .CollectSound(AssetManager.Get<SoundObject>("notebookjingle"));
        public override TeacherState GetAngryState() => new Viktor_Chase(this);
        public override TeacherState GetHappyState() => new Viktor_Happy(this);

        internal static AssetManager AssetManager = new AssetManager();
        internal static void LoadAssets()
        {
            AudioClip Clip(params string[] path) => AssetLoader.AudioClipFromMod(ViktorPlugin.Instance, path);
            SoundObject Voiceline(AudioClip clip, string subtitle) => ObjectCreators.CreateSoundObject(clip, subtitle, SoundType.Voice, Color.white);
            
            AssetManager.Add("default", AssetLoader.TextureFromMod(ViktorPlugin.Instance, "viktor", "default.png").ToSprite(16f));
            AssetManager.Add("angry", AssetLoader.TextureFromMod(ViktorPlugin.Instance, "viktor", "evil.png").ToSprite(16f));
            AssetManager.Add("notebook", AssetLoader.TextureFromMod(ViktorPlugin.Instance, "notebook", "math.png").ToSprite(30f));
            AssetManager.Add("notebookjingle", ObjectCreators.CreateSoundObject(Clip("notebook", "math_jingle.ogg"), "Notebook!", SoundType.Effect, Color.white, 0));


            var jackets = new SoundObject[]
            {
                Voiceline(Clip("viktor", "jacket0.ogg"), "No! My jacket, I will show you. Stay here, I will change it."),
                Voiceline(Clip("viktor", "jacket1.ogg"), "Come on! This is not funny at all."),
                Voiceline(Clip("viktor", "jacket2.ogg"), "Damn it!"),
                Voiceline(Clip("viktor", "jacket3.ogg"), "Oh my god!"),
                Voiceline(Clip("viktor", "jacket4.ogg"), "Are you joking?")
            };
            AssetManager.Add("jackets", jackets);
            AssetManager.Add("goodjob", Voiceline(Clip("viktor", "goodjob.ogg"), "Good job."));
            AssetManager.Add("youshouldnt", Voiceline(Clip("viktor", "youshouldnt.ogg"), "You should not have done that."));

            AssetManager.Add("school", ObjectCreators.CreateSoundObject(Clip("viktor", "school.ogg"), "", SoundType.Music, Color.white, 0));
            AssetManager.Add("walk", ObjectCreators.CreateSoundObject(Clip("viktor", "walk.ogg"), "*Footsteps*", SoundType.Effect, Color.white));
            AssetManager.Add("scream", ObjectCreators.CreateSoundObject(Clip("viktor", "scream.ogg"), "", SoundType.Effect, Color.white, 0));
        }

        public override void Initialize()
        {
            base.Initialize();
            caughtOffset += new Vector3(0, 1, 0);
            animator.animations.Add("default", new CustomAnimation<Sprite>(new Sprite[] { AssetManager.Get<Sprite>("default") }, 1));
            animator.animations.Add("angry", new CustomAnimation<Sprite>(new Sprite[] { AssetManager.Get<Sprite>("angry") }, 1));
            animator.Play("default", 1);

            if (ec.GetComponent<ViktorTilePollutionManager>() == null) ec.gameObject.AddComponent<ViktorTilePollutionManager>();
            PollutionManager = ec.GetComponent<ViktorTilePollutionManager>();

            AudMan.volumeMultiplier *= 2f;

            ReplaceMusic(AssetManager.Get<SoundObject>("school"));
            AddLoseSound(AssetManager.Get<SoundObject>("scream"),100);
        }

        public override void Slap()
        {
            StopCoroutine("StopDelay");
            AudMan.PlaySingle(AssetManager.Get<SoundObject>("walk"));
            Navigator.SetSpeed(Speed * 3f);
            StartCoroutine(StopDelay());
        }

        private IEnumerator StopDelay()
        {
            var time = 0.7f;
            while (time > 0)
            {
                time -= Time.deltaTime * ec.NpcTimeScale;
                yield return null;
            }
            Navigator.SetSpeed(0f);
            Navigator.maxSpeed = 0;
            yield break;
        }
    }

    /// <summary>
    /// A base for all your custom teacher state, you will need it if your teacher has custom fields.
    /// </summary>
    public class Viktor_StateBase : TeacherState
    {
        public Viktor_StateBase(Viktor viktor) : base(viktor)
        {
            this.viktor = viktor;
        }
        protected Viktor viktor;
        public override void NotebookCollected(int currentNotebooks, int maxNotebooks)
        {
            base.NotebookCollected(currentNotebooks, maxNotebooks);
        }
    }

    public class Viktor_Happy : Viktor_StateBase
    {
        public Viktor_Happy(Viktor example) : base(example) { }
        public bool veryHappy = false;
        public override void Enter()
        {
            base.Enter();
            viktor.animator.SetDefaultAnimation("default", 1);
            viktor.animator.Play("default", 1);
            viktor.Navigator.SetSpeed(0f);
            viktor.Navigator.maxSpeed = 0;
        }
        public override void NotebookCollected(int currentNotebooks, int maxNotebooks)
        {
            if (veryHappy) return;
            base.NotebookCollected(currentNotebooks, maxNotebooks);
            if (currentNotebooks >= maxNotebooks && viktor.IsHelping())
            {
                viktor.AudMan.PlaySingle(Viktor.AssetManager.Get<SoundObject>("goodjob"));
                viktor.behaviorStateMachine.ChangeState(new Viktor_Happy(viktor) { veryHappy = true });
                return;
            }
            viktor.ActivateSpoopMode();
            viktor.behaviorStateMachine.ChangeState(new Viktor_Chase(viktor));
            if (!viktor.IsHelping())
            {
                viktor.ReplaceMusic();
                viktor.AudMan.PlaySingle(Viktor.AssetManager.Get<SoundObject>("youshouldnt"));
            }
        }
    }

    public class Viktor_Jacket : Viktor_StateBase
    {
        public Viktor_Jacket(Viktor viktor, Viktor_StateBase previousState) : base(viktor) {
            this.previousState = previousState;
        }
        private Viktor_StateBase previousState;
        private bool goingToCloset = false;
        public override void Enter()
        {
            base.Enter();
            viktor.Navigator.ClearDestination();
            viktor.Navigator.SetSpeed(0f);
            if (viktor.firstTimeJacketTriggered)
                viktor.AudMan.QueueRandomAudio(Viktor.AssetManager.Get<SoundObject[]>("jackets"));
            else viktor.AudMan.QueueAudio(Viktor.AssetManager.Get<SoundObject[]>("jackets")[0]);
            viktor.StopCoroutine("StopDelay");

            viktor.firstTimeJacketTriggered = true;

            GottaSweep gottaSweep = null;
            Vector3 loc = viktor.transform.position;
            foreach (var npc in viktor.ec.npcs)
            {
                if (npc.Character == Character.Sweep)
                {
                    gottaSweep = (GottaSweep)npc;
                    loc = gottaSweep.home;
                    break;
                }
            }
            if (gottaSweep == null && viktor.ec.offices.Count > 0)
            {
                loc = viktor.ec.offices[0].RandomEntitySafeCellNoGarbage().TileTransform.position;
            }
            ChangeNavigationState(new NavigationState_TargetPosition(viktor, 127, loc));
        }
        public override void Update()
        {
            base.Update();
            if (!viktor.AudMan.AnyAudioIsPlaying)
            {
                goingToCloset = true;
                viktor.Navigator.SetSpeed(20f);
            }
        }

        public override void DestinationEmpty()
        {
            base.DestinationEmpty();
            viktor.behaviorStateMachine.ChangeState(previousState);
        }
    }

    public class Viktor_Chase : Viktor_StateBase
    {
        public Viktor_Chase(Viktor example) : base(example) { }
        private float delay;
        public override void Enter()
        {
            base.Enter();
            delay = 4f; // First time delay to avoid surprises
            viktor.animator.SetDefaultAnimation("angry", 1);
            viktor.animator.Play("angry", 1);
        }

        public override void Update()
        {
            base.Update();
            delay -= Time.deltaTime * viktor.ec.NpcTimeScale;
            if (delay <= 0)
            {
                ChangeNavigationState(new NavigationState_TargetPlayer(viktor, 0, viktor.players[0].transform.position));
                viktor.Slap();
                delay = viktor.Delay * 2f;
            }

            IntVector2 gridPosition = IntVector2.GetGridPosition(viktor.transform.position);
            Cell cell = viktor.ec.CellFromPosition(gridPosition);
            if (cell != null && viktor.PollutionManager.IsCellPolluted(cell))
            {
                viktor.StopCoroutine("StopDelay");
                viktor.behaviorStateMachine.ChangeState(new Viktor_Jacket(viktor, this));
            }
        }

        public override void NotebookCollected(int currentNotebooks, int maxNotebooks)
        {
            base.NotebookCollected(currentNotebooks, maxNotebooks);
            if (currentNotebooks >= maxNotebooks && viktor.IsHelping())
            {
                viktor.AudMan.PlaySingle(Viktor.AssetManager.Get<SoundObject>("goodjob"));
                viktor.behaviorStateMachine.ChangeState(new Viktor_Happy(viktor) { veryHappy = true });
            }
        }

        public override void OnStateTriggerStay(Collider other)
        {
            // Obvious
            if (viktor.IsTouchingPlayer(other))
            {
                var plr = other.GetComponent<PlayerManager>();
                var itm = plr.itm;
                if (itm.Has(Items.Apple))
                {
                    itm.Remove(Items.Apple);
                    viktor.behaviorStateMachine.ChangeState(new Viktor_Jacket(viktor, this));
                    return;
                }
                viktor.CaughtPlayer(plr);
            }
        }
    }

}
