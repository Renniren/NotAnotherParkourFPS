using System.IO;
using System.Linq;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using System;
using Random = UnityEngine.Random;
using System.Runtime.InteropServices;

namespace ParkourFPS
{

	public sealed class World : MonoBehaviorExtended
	{
		public static Entity PlayerEntity;

		public static void SetupWorld()
		{

		}
	}

	[System.Serializable]
	public struct SaveData
	{
		public List<string> UnlockedDataEntries;
		public string[] serializedUnlockedDataEntries;
		public float playtime;
		public bool exists;

		public void initialize()
		{
			UnlockedDataEntries = new List<string>();
			exists = true;
		}
	}

	public sealed class Game : MonoBehaviorExtended
	{
		public static readonly string ProjectCompletedOn =
			"??/??/2023 - 4:50 PM";
		private Game() { }

		public enum GState { Menu, PauseMenu, NormalGameplay, Cutscene, LevelEditor }
		public enum Difficulty { Softie, Normal, Diffcult, Lunatic }

		public static GState State;
		public static Gamemode Mode;

		public static Difficulty difficulty;
		public static float Volume = 1;

		public static bool Paused;
		public static bool InCutscene;
		public static bool UsedContinue;
		public static float LevelTime;

		public delegate void onLevelComplete(bool LastLevel);
		public static event onLevelComplete OnLevelComplete;

		public delegate void onGamePause();
		public static event onGamePause OnGamePaused;


		public delegate void onGameUnpause();
		public static event onGameUnpause OnGameUnpaused;
		public static bool initialized;

		public static SaveData save;

		public static void InitializeVariables()
		{
			bool AllVariablesAssigned =
					Player != null &&
					PlayerEntity != null;
			int failsafe = 0;
			while (AllVariablesAssigned == false)
			{
				failsafe++;
				AllVariablesAssigned =
					Player != null &&
					PlayerEntity != null;

				Player = GameObject.FindWithTag("Player");
				if (Player) PlayerEntity = Player.GetEntity();

				if (failsafe > 100)
				{
					Debug.Log("Got stuck in variable assignment loop.");
					break;
				}
			}
			System.GC.Collect();
			Debug.Log("Successfully assigned variables.");
		}

		public static void init()
		{
			if (initialized) return;
			save.initialize();
			SaveDataManager.SetupDirectories();
			difficulty = Difficulty.Normal;
			Volume = PlayerPrefs.GetFloat("GameVolume", 0.8f);
			State = GState.Menu;
			InitializeVariables();
			SaveDataManager.LoadFile();
			initialized = true;
		}

		static GState lastStateBeforePausing;
		public static void PauseGame(bool st)
		{
			Paused = st;
			if (Paused)
			{
				lastStateBeforePausing = State;
				OnGamePaused?.Invoke();
				State = GState.PauseMenu;
			}

			if (!Paused)
			{
				State = lastStateBeforePausing;
				OnGameUnpaused?.Invoke();
			}
		}

		public static void CompleteLevel(bool LastLevel)
		{
			OnLevelComplete?.Invoke(LastLevel);
		}
	}

	public static class SaveDataManager
	{
		static string save_path = Application.persistentDataPath + "\\Saves\\";
		static string name = "save", extension = ".insdata";
		public static void SaveFile()
		{
			void AddText(FileStream fs, string value)
			{
				byte[] info = new UTF8Encoding(true).GetBytes(value);
				fs.Write(info, 0, info.Length);
			}

			Game.save.serializedUnlockedDataEntries = Game.save.UnlockedDataEntries.ToArray();

			string filename = name + extension;
			string data = JsonUtility.ToJson(Game.save);
			if (!Directory.Exists(save_path)) Directory.CreateDirectory(save_path);

			if (File.Exists(save_path + filename))
			{
				using (FileStream fuckyourself = File.OpenWrite(save_path + filename))
				{
					AddText(fuckyourself, data);
				}
			}
			else
			{
				using (FileStream fuckyourself = File.Create(save_path + filename))
				{
					AddText(fuckyourself, data);
				}
			}
			//File.Delete(save_path + filename);

		}

		//Because Unity likes to make messes.
		//Without cleaning up the save file, there's a decent chance that it could be "corrupted", breaking everything in the game.
		public static void CleanUpData(ref string txt)
		{
			int end = -1;
			for (int i = 0; i < txt.Length; i++)
			{
				if (txt[i] == '}')
				{
					end = i;
					break;
				}
			}

			if (end + 1 < txt.Length) txt = txt.Remove(end + 1);
		}

		public static void LoadFile()
		{
			string text = File.ReadAllText(save_path + name + extension);
			CleanUpData(ref text);
			SaveData data = (SaveData)JsonUtility.FromJson(text, typeof(SaveData));
			data.UnlockedDataEntries = data.serializedUnlockedDataEntries.ToList();
			if (data.exists) //"exists" is set by the save's Initialization function. If it is false, then there was no save data.
			{
				Game.save = data;
			}
		}

		public static void SetupDirectories()
		{
			bool setup = Directory.Exists(save_path);
			if (!setup)
			{
				Directory.CreateDirectory(save_path);
				Game.save.UnlockedDataEntries.Add("help");
				SaveFile();
			}
		}
	}

	public enum DefenseType { None, Light, Heavy }

	public enum Gamemode { Campaign, Endless, Intro, Editor }

	public enum TestMode { Victim, VictimParent, VictimRoot }

	[System.Serializable]
	public struct Explosion
	{
		public string Effect;
		public float Force;
		public float Radius;
		public int Damage;
		public bool CreatesDebris;

		public void Copy(Explosion e)
		{
			this.Force = e.Force;
			this.Effect = e.Effect;
			this.Radius = e.Radius;
			this.Damage = e.Damage;
			this.CreatesDebris = e.CreatesDebris;
		}

		public void DrawEffectRegion(Vector3 where)
		{
			Gizmos.DrawWireSphere(where, Radius);
		}

		public void Explode(Vector3 where, bool affectplayer = true)
		{
			if (Effect != string.Empty && Effect != "none")
			{
				GameObject poolObject = GameManager.GetPool(Effect).GetPoolObject();
				poolObject.transform.position = where;
				poolObject.transform.rotation = Quaternion.identity;
				if (poolObject.GetRigidbody()) poolObject.GetRigidbody().ResetVelocity();
				poolObject.SetActive(true);
			}

			if (CreatesDebris)
			{
				int pick = 0;
				for (int i = 0; i < 30; i++)
				{
					pick = Random.Range(1, 3);

					Game.Spawn("Debris" + pick, where).GetRigidbody().RandomizeVelocity(Force);
				}
			}

			Entity.Explode(where, Radius, Force, Damage, affectplayer);
		}
	}

	[System.Serializable]
	public struct MeleeSettings
	{
		public bool Allowed, DeflectAllowed;
		public string Animation, DeflectAnimation;

		public int Damage;
		public float UpwardsKnockback, Knockback;
		public float HurtDelay;
		public float Delay;
		public float Range;

		public float CharacterStopTime;
		public float Cooldown;
	}

	//Used to define which set of animations the weapon should use.
	public enum MeleeWeaponType { None, Fists, OneHandedBlade, TwoHandedBlade, OneHandedBlunt, TwoHandedBlunt, Throwing, Knife }

	//Used to determine sounds played when swinging or hitting things.
	public enum MeleeWeaponMaterial { Metallic, Other, Sharp, Fists }

	//When should this animation be used?
	public enum AnimationContext { None, Swing, SprintEnter, SprintExit, Throw, Punch, Equip, KnifeStab }

	/// <summary>
	/// Describes a Melee Animation for Melee Weapons.
	/// Animation Contexts are used to tag an animation for when it should be played.
	/// MeleeWeaponTypes are used to define the nature of the melee weapon.
	/// </summary>
	[System.Serializable]
	public struct MeleeAnimation
	{
		public string Name;
		public MeleeWeaponType weaponType;
		public AnimationContext context;
	}


	/// <summary>
	/// Describes any object that is to be pooled.
	/// </summary>
	[System.Serializable]
	public struct Asset
	{
		public string name;
		public int AmountToPool;
		public GameObject obj;

		public Asset(string name)
		{
			this.name = name;
			obj = null;
			AmountToPool = 1;
		}

		public static implicit operator GameObject(Asset ass)
		{
			return ass.obj;
		}
	}

	/// <summary>
	/// Describes a Sound. Wrapper for AudioClips so that sounds can be assigned a "nice" name to be used by PlaySound.
	/// </summary>
	[System.Serializable]
	public struct Sound
	{
		public string name;
		public AudioClip clip;

		public Sound(AudioClip clip)
		{
			name = clip.name;
			this.clip = clip;
		}

		public static implicit operator AudioClip(Sound sound)
		{
			return sound.clip;
		}
	}

	/// <summary>
	/// Describes a Weapon so that it may be picked up and used by the Player. 
	/// Must inherit from BaseWeapon to have any degree of functionality.
	/// </summary>
	[Serializable]
	public struct Weapon
	{
		public string Name;
		public GameObject Asset;
	}

	public struct ObjectiveDescriptor
    {
		
    }

    /// <summary>
    /// Describes an Object Pool.
    /// </summary>
    [Serializable]
	public struct Pool
	{
		public string Name;
		public int capacity;
		public GameObject poolObject;
		public List<GameObject> objects;
		public bool Generated;

		public void SetGenerated() { Generated = true; } //Because apparently that's not a variable. Cunt.

		public Pool(int capacity, string name, GameObject obj)
		{
			this.capacity = capacity;
			this.Generated = false;
			this.objects = new List<GameObject>(capacity);
			this.Name = name;
			this.poolObject = obj;
		}

		public List<int> HasHoles()
		{
			List<int> emptyEntries = new List<int>(); ;
			for (int i = 0; i < objects.Count; i++)
			{
				if (objects[i] == null) emptyEntries.Add(i);
			}
			return emptyEntries;
		}

		public void FillHoles(List<int> destinations)
		{
			GameObject temp;
			for (int i = 0; i < destinations.Count; i++)
			{
				temp = GameObject.Instantiate(poolObject, Vector3.one * -999, Quaternion.identity);
				temp.SetActive(false);
				GameObject.DontDestroyOnLoad(temp);
				objects[destinations[i]] = temp;
			}
		}

		public bool AreAllObjectsActive()
		{
			if (objects == null) return false;
			if (objects[0] == null) return false;
			if (objects[objects.Count - 1] == null) return false;
			return objects[0].activeInHierarchy && objects[objects.Count - 1].activeInHierarchy;
		}

		public void Add(GameObject go)
		{
			objects?.Add(go);
		}

		public GameObject GetPoolObject()
		{
			if (AreAllObjectsActive())
			{
				Debug.LogWarning("Pool " + Name + " ran out of objects.");
				for (int i = 0; i < objects.Count; i++)
				{
					if (objects[i].activeInHierarchy)
					{
						objects[i].SetActive(false);
					}
				}
			}

			GameObject ret = null;

			for (int i = 0; i < objects.Count; i++)
			{
				if (objects[i] == null) continue;
				if (!objects[i].activeInHierarchy) // Active in hierarchy, but we're already this pool's limit.
				{
					return objects[i];
				}
			}

			return ret;
		}
	}


	[System.Serializable]
	public struct LoreEntryTextFont
	{
		public string identifier;
		public TMPro.TMP_FontAsset font;
	}

	public class LinkedList<T>
	{
		public class Node<T>
		{
			public LinkedList<T> Parent;
			public Node<T> Next;
			public T data;
		}

	}


	[Serializable]
	public struct Timer
	{
		public string name;
		public float end;

		public float elapsed;
		public bool repeat;
		public bool running;

		bool finished;
		bool use_unscaled_time;

		public Timer(float end, bool repeat, bool unscaled = false, string name = "")
		{
			this.name = name;
			this.end = end;
			this.elapsed = 0;
			this.repeat = repeat;
			this.use_unscaled_time = unscaled;
			finished = false;
			running = false;
		}

		public void Update()
		{
			if (running)
            {
				if (!finished)
				{
					if (use_unscaled_time)
					{
						elapsed += Time.unscaledDeltaTime;
					}
					else
					{
						elapsed += Time.deltaTime;
					}
				}
            }
		}

		public void Start()
        {
			running = true;
        }

		public void Stop()
        {
			running = false;
        }

		public void Reset()
		{
			elapsed = 0;
		}

		public void TryReset()
		{
			if (elapsed >= end)
			{
				if (!repeat)
				{
					running = false;
					finished = true;
				}
				elapsed = 0;
			}
		}

		public bool IsComplete()
		{
			return elapsed >= end;
		}
	}

	public class FlyingEnemies : Entity
	{
		[System.Serializable]
		public struct Configuration
		{
			public enum movementMode { MoveTo, Lerp, Slerp, MoveToSmoothed, PhysicsBased }
			public movementMode MovementMode;
			public bool Active;
			public bool LookInVelocityDirection;
			public Rigidbody PhysicsHull;
			public Transform LookAtTarget;
			public Transform origin;
			public Vector3 UnmodifiedPosition;
			public Vector3 Destination;
			public Speedometer speedometer;
			public float DampingAmount;
			public float ForceUpForce;
			public float speed;
			public float MinimumDistanceFromGround;
		}


		[Header("Flight Settings")]
		public Configuration Config;

		protected void Update()
		{
			base.Update();
			if (Config.Active)
			{
				MaintainDistanceFromGround(Config.MinimumDistanceFromGround);
			}
		}

		void MaintainDistanceFromGround(float height = 5)
		{
			RaycastHit hit;
			Physics.Raycast(transform.position, -transform.up, out hit, Mathf.Infinity);
			if (hit.collider != null)
			{
				if (Config.origin.position.y - hit.point.y < height)
				{
					Config.PhysicsHull.AddForce(Vector3.up * (Config.ForceUpForce * Config.PhysicsHull.mass * Time.fixedDeltaTime), ForceMode.Acceleration);
				}
			}
		}

		public void SetDestination(Vector3 where)
		{
			Config.Destination = where;
		}

		public void Move(Vector3 where)
		{
			if (!Config.Active || Game.Paused) return;
			//where = Destination;

			RaycastHit hit;
			Physics.Raycast(Config.origin.position, -Config.origin.up, out hit, Mathf.Infinity, LayerMask.GetMask("Default"));
			RaycastHit hit2;
			Physics.Raycast(Config.origin.position, Config.origin.forward, out hit2, 180, LayerMask.GetMask("Default"));
			if (hit.collider != null)
			{
				Config.UnmodifiedPosition = hit.point;
				while (where.y - hit.point.y < Config.MinimumDistanceFromGround)
				{
					where.y = where.y + 2;
				}


				if (where.y - hit.point.y < Config.MinimumDistanceFromGround)
				{
					where.y = where.y + 2;
					//transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, hit.point.y + MinimumDistanceFromGround, transform.position.z), 0.1f);
				}

			}

			if (hit2.collider != null)
			{
				while (where.y - hit2.point.y < Config.MinimumDistanceFromGround)
				{
					where.y = where.y + .2f;
				}


				if (where.y - hit2.point.y < Config.MinimumDistanceFromGround)
				{
					where.y = where.y + .2f;
					//transform.position = Vector3.Lerp(transform.position, new Vector3(transform.position.x, hit.point.y + MinimumDistanceFromGround, transform.position.z), 0.1f);
				}

			}


			Config.Destination = where;

			switch (Config.MovementMode)
			{
				case Configuration.movementMode.MoveTo:
					transform.position = Vector3.MoveTowards(transform.position, Config.Destination, Config.speed * Time.deltaTime);
					break;

				case Configuration.movementMode.Lerp:
					transform.position = Vector3.Lerp(transform.position, Config.Destination, Config.speed * Time.deltaTime);
					break;

				case Configuration.movementMode.Slerp:
					transform.position = Vector3.Slerp(transform.position, Config.Destination, Config.speed * Time.deltaTime);
					break;

				case Configuration.movementMode.MoveToSmoothed:
					transform.position = Vector3.SmoothDamp(transform.position, Config.Destination, ref Config.speedometer.Velocity, Config.DampingAmount, Config.speed, Time.deltaTime);
					break;

				case Configuration.movementMode.PhysicsBased:
					Vector3 desired_force = Config.Destination - Config.origin.position;
					Config.PhysicsHull.AddForce(Vector3.Normalize(desired_force) * ((Config.speed * Config.PhysicsHull.mass) * Time.fixedDeltaTime), ForceMode.Force);
					Config.PhysicsHull.AddForce(-Config.PhysicsHull.velocity * Config.DampingAmount, ForceMode.Acceleration);

					break;
			}
		}
	}

	public class SequenceUtility
	{
		[System.Serializable]
		public struct Sequence
		{
			[System.Serializable]
			public struct SequenceEvent
			{
				public int When;
				public UnityEngine.Events.UnityEvent Event;
			}
			public float FloatCurrent;
			public int Current;
			public int Start;
			public int Length;
			public bool Loop;

			public SequenceEvent[] Events;
		}

		public static void ManageSequence(Sequence input, out Sequence output)
		{
			input.FloatCurrent += Time.deltaTime;
			input.Current = (int)input.FloatCurrent;
			if ((int)input.Current >= input.Length)
			{
				if (input.Loop)
				{
					input.Current = input.Start;
				}
			}

			foreach (Sequence.SequenceEvent ev in input.Events)
			{
				if (input.Current == ev.When)
				{
					ev.Event.Invoke();
				}
			}

			output = input;
		}
	}

	[System.Serializable]
	public struct table
	{
		List<(string, object)> elements;

		public table(int d = 0)
		{
			elements = new List<(string, object)>();
		}

		public void Clear()
		{
			elements.Clear();
		}

		public object this[string key]
		{
			get
			{
				if (elements == null) elements = new List<(string, object)>();
				var found = elements.Find(entry => entry.Item1 == key);
				if (found.Item1 != null)
				{
					return found.Item1;
				}
				elements.Add((key, null));
				return null;
			}
			set
			{
				if (elements == null) elements = new List<(string, object)>();

				bool found = false;
				for (int i = 0; i < elements.Count; i++)
				{
					if (elements[i].Item1 == key)
					{
						found = true;
						(string, object) fuck_off = (key, value);
						elements[i] = fuck_off;
					}
				}

				if (!found) elements.Add((key, value));
			}
		}
	}
	/// <summary>
	/// Bulk class that contains all Events that are used in the game. It's good practice to keep new Events in this class to make sure
	/// there is only one place to look for all important events.
	/// </summary>
	public static class Events
	{
		public delegate void beforeEntityTakeDamage(Entity ent, ref int amt, GameObject from);
		public static event beforeEntityTakeDamage BeforeEntityDamaged;
		public static void OnEntityDamagedInvoke(Entity ent, ref int amt, GameObject from) { BeforeEntityDamaged?.Invoke(ent, ref amt, from); }

		public delegate void entityTakeDamage(Entity ent, int amt, int final, GameObject from);
		public static event entityTakeDamage OnEntityDamaged;
		public static void OnEntityDamagedInvoke(Entity ent, int amt, int final, GameObject from) { OnEntityDamaged?.Invoke(ent, amt, final, from); }

		public delegate void attemptedToDamageObject(GameObject victim, ref int amt, GameObject from);
		public static event attemptedToDamageObject OnDamageAttempted;
		public static void OnDamageAttemptedInvoke(GameObject victim, ref int amt, GameObject from) { OnDamageAttempted?.Invoke(victim, ref amt, from); }

		public delegate void entityDied(Entity ent);
		public static event entityDied OnEntityKilled;
		public static void OnEntityKilledInvoke(Entity ent) { OnEntityKilled?.Invoke(ent); }

		public delegate void explosionMade(Vector3 where, float radius, float power, int dmg, Collider[] victims);
		public static event explosionMade OnExplosion;
		public static void OnExplosionInvoke(Vector3 where, float radius, float power, int dmg, Collider[] victims) { OnExplosion?.Invoke(where, radius, power, dmg, victims); }

		public delegate void PhotomodeEntered();
		public static event PhotomodeEntered OnPhotomodeEnter;
		public static void InvokePhotomodeEnter() { OnPhotomodeEnter?.Invoke(); }

		public delegate void PhotomodeExited();
		public static event PhotomodeEntered OnPhotomodeExit;
		public static void InvokePhotomodeExit() { OnPhotomodeExit?.Invoke(); }

		public delegate void postGameInit();
		public static event postGameInit OnGameInitialized;
		public static void InvokeInit() { OnGameInitialized?.Invoke(); }

		public delegate void preGameInit();
		public static event preGameInit BeforeGameInitialized;
		public static void InvokePreInit() { OnGameInitialized?.Invoke(); }

		public delegate void onWeaponChanged();
		public static event onWeaponChanged OnWeaponChanged;
		public static void InvokeWeaponChanged() { OnWeaponChanged?.Invoke(); }

		public delegate void onWeaponFire(BallisticWeapon weapon);
		public static event onWeaponFire OnWeaponFired;
		public static void InvokeWeaponFired(BallisticWeapon weapon) { OnWeaponFired?.Invoke(weapon); }

		public delegate void onObjectiveActivate(Objective obj);
		public static event onObjectiveActivate OnObjectiveActivated;
		public static void InvokeObjectiveActivated(Objective obj) { OnObjectiveActivated?.Invoke(obj); }

		public delegate void onObjectiveDeactivate(Objective obj);
		public static event onObjectiveDeactivate OnObjectiveDeactivated;
		public static void InvokeObjectiveDeactivated(Objective obj) { OnObjectiveDeactivated?.Invoke(obj); }

		public delegate void onObjectiveCompleted(Objective obj);
		public static event onObjectiveCompleted OnObjectiveCompleted;
		public static void InvokeObjectiveCompleted(Objective obj) { OnObjectiveCompleted?.Invoke(obj); }

		public delegate void onWeaponSwappedTo(BaseWeapon to);
		public static event onWeaponSwappedTo OnWeaponSwapped;
		public static void InvokeOnWeaponSwapped(BaseWeapon to) { OnWeaponSwapped?.Invoke(to); }

		public delegate void hotswitchBegin(Vector3 where, GameObject target);
		public static event hotswitchBegin OnHotswitchStart;
		public static void InvokeHotswitchBegin(Vector3 where, GameObject target) { OnHotswitchStart?.Invoke(where, target); }

		public delegate void hotswitchEnd(Vector3 where, GameObject target);
		public static event hotswitchEnd OnHotswitchEnd;
		public static void InvokeHotswitchEnd(Vector3 where, GameObject target) { OnHotswitchEnd?.Invoke(where, target); }

		public delegate void hotswitchControl(GameObject target);
		public static event hotswitchControl WhilePlayerControllingObject;
		public static void InvokeHotswitchControl(GameObject target) { WhilePlayerControllingObject?.Invoke(target); }

		public delegate void hotswitchControlLeave(GameObject target);
		public static event hotswitchControl OnPlayerControlLeave;
		public static void InvokeHotswitchControlLeave(GameObject target) { OnPlayerControlLeave?.Invoke(target); }

		public delegate void defenseBreakBegin(GameObject target);
		public static event defenseBreakBegin OnDefenseBreakBegin;
		public static void InvokeDefenseBreakBegin(GameObject target) { OnDefenseBreakBegin?.Invoke(target); }

		public delegate void defenseBroken(GameObject target);
		public static event defenseBroken OnDefenseBroken;
		public static void InvokeDefenseBroken(GameObject target) { OnDefenseBroken?.Invoke(target); }

		public delegate void defenseBreakEnd(GameObject target, bool success);
		public static event defenseBreakEnd OnDefenseBreakEnd;
		public static void InvokeDefenseBreakEnd(GameObject target, bool success) { OnDefenseBreakEnd?.Invoke(target, success); }
	}

	public static class Extensions
	{
		public enum TestMode
		{
			Victim,
			VictimParent,
			VictimRoot
		}

		public static void ForcePlay(this Animator animator, string state)
		{
			animator.gameObject.SetActive(false);
			animator.gameObject.SetActive(true);
			animator.Play(state);
		}

		public static void AddOnce<T>(this List<T> to, T element)
        {
			if (!to.Contains(element)) to.Add(element);
        }

		public static T GetRandomElement<T>(this T[] from)
		{
			return from[Random.Range(0, from.Length)];
		}

		public static T GetRandomElement<T>(this List<T> from)
		{
			return from[Random.Range(0, from.Count)];
		}

		public static bool IsPlayer(this GameObject operand)
		{
			if (!operand) return false;
			return operand.CompareTag("Player") || operand == MonoBehaviorExtended.PlayerEntity.gameObject;
		}

		public static void SetActive(this GameObject[] j, bool s)
		{
			foreach (GameObject g in j)
			{
				g.SetActive(s);
			}
		}

		public static PlayerMovement GetPlayerMovement3D(this GameObject ply)
		{
			if (!ply.IsPlayer())
			{
				Debug.LogError(ply + " is not the Player.");
				return null;
			}

			ply.TryGetComponent(out PlayerMovement pm);
			if (!pm)
			{
				Debug.LogError(ply + " is not a 3D Player.");
				return null;
			}

			return pm;
		}

		public static Vector3 FarthestPoint(this BoxCollider boxCollider, Vector3 from)
		{
			Vector3 pt = Vector3.zero;

			Bounds b = boxCollider.bounds;
			return pt;
		}

		public static bool IsDynamic(this GameObject what, bool TestRoot = true)
		{
			if (!what) return false;
			Rigidbody test = null;
			if (TestRoot)
			{
				what.transform.root.TryGetComponent(out test);
				return test != null;
			}
			else
			{
				what.TryGetComponent(out test);
				return test != null;
			}
		}

		public static EnemyController GetEnemyController(this GameObject operant)
		{
			EnemyController ret;
			operant.TryGetComponent(out ret);
			return ret;
		}

		public static Bullet GetBullet(this GameObject on)
		{
			on.TryGetComponent(out Bullet bul);
			return bul;
		}

		public static void Play(this AudioSource src, int g = 1)
		{
			src.volume *= Game.Volume;
			src.Play();
		}

		public static Vector3 AverageVelocity(this Rigidbody[] bodies)
		{
			Vector3 vel = Vector3.zero;
			foreach (var item in bodies)
			{
				vel += item.velocity;
			}
			vel /= bodies.Length;
			return vel;
		}

		public static bool IsWeapon(this GameObject what)
		{
			what.TryGetComponent(out BaseWeapon v);
			return v != null;
		}

		public static bool IsEnemy(this GameObject what, bool TestRoot = true)
		{
			if (what == null) return false;
			if (TestRoot)
			{
				if (what.transform.root.CompareTag("Enemy") || what.transform.root.gameObject.layer == 17 || what.transform.root.gameObject.layer == 9)
				{
					return true;
				}
			}
			else
			{
				if (what.CompareTag("Enemy") || what.layer == 17 || what.layer == 9)
				{
					return true;
				}
			}

			return false;
		}

		public static MeshRenderer GetMeshRenderer(this GameObject on)
		{
			on.TryGetComponent(out MeshRenderer ren);
			return ren;
		}

		public static HelperHitbox GetHelperHitbox(this GameObject on)
		{
			on.TryGetComponent(out HelperHitbox ent);
			return ent;
		}

		public static Enemy GetEnemy(this GameObject on)
		{
			on.TryGetComponent(out Enemy enemy);
			return enemy;
		}

		public static Entity GetEntity(this GameObject on)
		{
			if (on.GetHelperHitbox() != null)
			{
				Entity ret;
				on.GetHelperHitbox().Parent.TryGetComponent(out ret);
				return ret;
			}

			if (on.GetHitbox() != null)
			{
				return on.GetHitbox().Parent;
			}
			else
			{
				on.TryGetComponent(out Entity ent);
				return ent;
			}
			on.TryGetComponent(out Entity ent2);
			return ent2;
		}

		public static BaseEntity GetBaseEntity(this GameObject on)
		{
			on.TryGetComponent(out BaseEntity ent);
			return ent;
		}

		public static Hitbox GetHitbox(this GameObject on)
		{
			Hitbox hitbox;
			on.TryGetComponent(out hitbox);
			return hitbox;
		}

		public static bool BelongsToEnemy(this GameObject what)
		{
			return what.IsEnemy(MonoBehaviorExtended.GetObjectOwner(what));
		}

		public static Rigidbody GetRigidbody(this GameObject on)
		{
			on.TryGetComponent(out Rigidbody rb);
			return rb;
		}

		public static SpriteRenderer GetSprite(this GameObject on)
		{
			on.TryGetComponent(out SpriteRenderer sp);
			return sp;
		}

		public static void Play(this AudioClip[] audioClips)
		{
			foreach (var item in audioClips)
			{

			}
		}

		public static void ResetVelocity(this Rigidbody on)
		{
			on.velocity = Vector3.zero;
			on.angularVelocity = Vector3.zero;
		}

		public static void RandomizeVelocity(this Rigidbody on, float limit = 50)
		{
			on.velocity = (Vector3.right * Random.Range(-limit, limit))
				+ (Vector3.up * Random.Range(-limit, limit)) +
				(Vector3.forward * Random.Range(-limit, limit));
		}



		public static void RandomizeAngularVelocity(this Rigidbody on, float limit = 50)
		{
			on.angularVelocity = (Vector3.right * Random.Range(-limit, limit))
				+ (Vector3.up * Random.Range(-limit, limit)) +
				(Vector3.forward * Random.Range(-limit, limit));
		}

		public static bool IsProjectile(this GameObject what)
		{
			what.TryGetComponent(out ExplosiveProjectile ex);
			what.TryGetComponent(out Bullet b);
			return b != null || ex != null;
		}

		public static void RandomizeVelocity(this Rigidbody[] on, float limit = 50)
		{
			foreach (Rigidbody rb in on)
			{
				rb.velocity = (Vector3.right * Random.Range(-limit, limit))
					+ (Vector3.up * Random.Range(-limit, limit)) +
					(Vector3.forward * Random.Range(-limit, limit));
			}
		}



		public static int WouldKillHowMany(int damage, Collider[] victims, TestMode mode)
		{
			int count = 0;
			for (int i = 0; i < victims.Length; i++)
			{
				if (victims[i] == null) continue;

				if (WouldKill(damage, victims[i].gameObject, mode))
				{
					count++;
				}
			}
			return count;
		}

		static Entity sentity;
		public static bool WouldKill(int operand, GameObject victim, TestMode mode)
		{
			if (!victim) return false;
			switch (mode)
			{
				case TestMode.Victim:
					victim.TryGetComponent(out sentity);
					if (!sentity) return false;
					if (sentity.Health < operand + sentity.Armor)
					{
						return true;
					}
					break;

				case TestMode.VictimParent:
					victim.transform.parent.TryGetComponent(out sentity);
					if (!sentity) return false;
					if (sentity.Health < operand + sentity.Armor)
					{
						return true;
					}
					break;

				case TestMode.VictimRoot:
					victim.transform.root.TryGetComponent(out sentity);
					if (!sentity) return false;
					if (sentity.Health < operand + sentity.Armor)
					{
						return true;
					}
					break;
			}
			return false;
		}
	}

	public class MonoBehaviorExtended : MonoBehaviour
	{
		public static GameObject Player;
		public static Entity PlayerEntity;
		public static MeshRenderer CameraFlash;
		public static ViewShake CameraShake;

		public static Camera GetViewmodelCamera()
		{
			return GameObject.FindGameObjectWithTag("ViewmodelCamera").GetComponent<Camera>();
		}

		public static Camera GetHUDCamera()
		{
			return GameObject.FindGameObjectWithTag("HUDCamera").GetComponent<Camera>();
		}

		public static Vector3 RandomVector(float limit = 10)
		{
			return new Vector3(Random.Range(-limit, limit), Random.Range(-limit, limit), Random.Range(-limit, limit));
		}

		public static void TransferRigidbodyVelocityToChildren(Vector3 owner, in Transform destination, float randomness = 0)
		{
			for (int i = 0; i < destination.childCount; i++)
			{
				destination.GetChild(i).TryGetComponent(out Rigidbody rb);
				if (!rb) continue;
				rb.velocity += owner + (RandomVector(randomness));
			}
		}

		public static void TransferRigidbodyVelocityToChildren(Vector3 owner, in Transform destination)
		{
			if (destination.GetChildCount() == 0) return;
			for (int i = 0; i < destination.childCount; i++)
			{
				destination.GetChild(i).TryGetComponent(out Rigidbody rb);
				if (!rb) continue;
				rb.velocity += owner * 1;
			}
		}

		public GameObject BecomeRagdoll(Vector3 ref_velocity, GameObject ragdoll, Transform origin)
		{
			if (!ragdoll) return null;
			GameObject doll = Instantiate(ragdoll, origin.position, origin.rotation);
			TransferRigidbodyVelocityToChildren(ref_velocity, doll.transform);
			doll.TryGetComponent(out RagdollForce rf);
			rf.InheritedVelocity = ref_velocity;
			rf.Rigidbodies.RandomizeVelocity(25);
			return doll;
		}

		public GameObject MakeThirdpersonCamera(Transform parent, Vector3 where, Quaternion rotation, float distance = 5, float fov = 105)
		{
			GameObject camera = Spawn("ThirdPersonCamera", where, rotation);
			camera.transform.SetParent(parent, true);
			BasicMouselookCamera cm = camera.GetComponent<BasicMouselookCamera>();
			cm.SetRotation(rotation);
			cm.transform.localPosition += cm.transform.forward * -Mathf.Abs(distance);
			cm.GetComponentInChildren<Camera>().fieldOfView = fov;
			camera.transform.position = where;
			return camera;
		}

		public void DestroyThirdpersonCamera(ref GameObject camera)
		{
			camera.SetActive(false);
			camera.transform.parent = null;
			camera = null;
		}

		/// <summary>
		/// Toggles Third-Person.
		/// The result of this method must be assigned to the same variable as camera_variable.
		/// Example: camera = ToggleThirdperson(ref camera, transform, transform.position, transform.rotation, 10, 110);
		/// </summary>
		/// <param name="camera_variable"></param>
		/// <param name="parent"></param>
		/// <param name="where"></param>
		/// <param name="rotation"></param>
		/// <param name="distance"></param>
		/// <param name="fov"></param>
		/// <returns></returns>
		public GameObject ToggleThirdperson(ref GameObject camera_variable, Transform parent, Vector3 where, Quaternion rotation, float distance = 5, float fov = 105)
		{
			if (camera_variable == null)
			{
				return MakeThirdpersonCamera(parent, where, rotation, distance, fov);
			}

			if (camera_variable != null)
			{
				DestroyThirdpersonCamera(ref camera_variable);
			}

			return null;
		}

		public void BecomeGibbedRagdoll(GameObject ragdoll, Transform origin, Vector3 ref_velocity)
		{
			if (ragdoll)
			{
				GameObject doll = BecomeRagdoll(Vector3.zero, ragdoll, origin);
				doll.TryGetComponent(out RagdollForce rf);
				TransferRigidbodyVelocityToChildren(ref_velocity, doll.transform);
				rf.Dismembered = true;
				rf.InheritedVelocity = ref_velocity;
				rf.Rigidbodies.RandomizeVelocity(25);
			}

			Spawn("GibbedBloodsplatter", origin.position);
		}

		public void AddScore(int howMuch)
		{
			GameManager.Score += howMuch;
			GameManager.InvokeScoreEvent(howMuch);
		}

		public static void LoadNewLevel(Scene dest)
		{
			System.GC.Collect();
			SceneManager.LoadScene(dest.name);
		}

		public static void LoadNewLevel(string dest)
		{
			System.GC.Collect();
			SceneManager.LoadScene(dest);
		}

		public float random(float lower, float upper)
		{
			return Random.Range(lower, upper);
		}

		public int random(int lower, int upper)
		{
			return Random.Range(lower, upper);
		}

		public int RandomIndex<T>(T[] from)
		{
			return random(0, from.Length);
		}

		public int RandomIndex<T>(List<T> from)
		{
			return random(0, from.Count);
		}

		public static float LoopingClamp(float val, float LowerLimit, float UpperLimit)
		{
			if (val < LowerLimit) return UpperLimit;
			if (val > UpperLimit) return LowerLimit;

			return val;
		}

		public static int LoopingClamp(int val, int LowerLimit, int UpperLimit)
		{
			if (val < LowerLimit) return UpperLimit;
			if (val > UpperLimit) return LowerLimit;

			return val;
		}

		public static float InverseLerp(Vector3 a, Vector3 b, Vector3 value)
		{
			Vector3 AB = b - a;
			Vector3 AV = value - a;
			return Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB);
		}

		public static bool Toggle(bool boole)
		{
			if (boole == true)
			{
				boole = false;
				return boole;
			}
			else
			{
				boole = true;
				return boole;
			}
		}

		public static bool WouldKill(int operand, GameObject victim, TestMode mode = TestMode.VictimRoot)
		{
			if (!victim) return false;
			switch (mode)
			{
				case TestMode.Victim:
					victim.TryGetComponent(out s_entity);
					if (!s_entity) return false;
					if (s_entity.Health < operand - s_entity.Armor)
					{
						return true;
					}
					break;

				case TestMode.VictimParent:
					victim.transform.parent.TryGetComponent(out s_entity);
					if (!s_entity) return false;
					if (s_entity.Health < operand - s_entity.Armor)
					{
						return true;
					}
					break;

				case TestMode.VictimRoot:
					victim.transform.root.TryGetComponent(out s_entity);
					if (!s_entity) return false;
					if (s_entity.Health < operand - s_entity.Armor)
					{
						return true;
					}
					break;
			}
			return false;
		}

		public static float PercentOf(float what, float iswhatpercentofwhat)
		{
			return (float)(what / iswhatpercentofwhat);
		}

		public static void DoFlash(float amt = 0.4f)
		{
			CameraFlash.material.color = new Color(CameraFlash.material.color.r, CameraFlash.material.color.g, CameraFlash.material.color.b, amt);
		}
		public static void SetObjectOwner(GameObject of, GameObject to)
		{
			if (of == null || to == null) return;
			Entity ent = of.GetEntity();
			if (ent)
			{
				ent.owner = to;
			}

			if (!ent)
			{
				BaseEntity ent2 = of.GetBaseEntity();
				if (ent2)
				{
					ent2.owner = to;
				}
			}
		}

		public static GameObject GetObjectOwner(GameObject of)
		{
			if (of == null) return null;

			Entity ent = of.GetEntity();
			if (ent)
			{
				return ent.owner;
			}
			else
			{
				BaseEntity d = of.GetBaseEntity();
				if (d)
				{
					return d.owner;
				}
			}
			return null;
		}



		/*		public static Hitbox GetHitbox(GameObject from)
				{
					from.TryGetComponent(out Hitbox box);
					if (box) return box;

					return null;
				}
		*/
		public Vector3 PlayerPosition
		{
			get
			{
				return Player.transform.position;
			}
			set
			{
				Player.transform.position = value;
			}
		}
		public Vector3 direction(Vector3 from, Vector3 to)
		{
			return -(from - to);
		}
		public float distance(Vector3 a, Vector3 b)
		{
			return Vector3.Distance(a, b);
		}
		public void Hurt(GameObject what, int damage, bool armor = false, bool shields = false)
		{
			what.TryGetComponent(out Entity ent);
			Entity.Hurt(gameObject, ent, damage, armor, shields);
		}

		private static Entity s_entity;

		public static void PlaySound(AudioClip sound, Vector3 where)
		{
			Spawn("SoundObject", where).TryGetComponent(out AudioSource source);
			source.Stop();
			source.clip = sound;
			source.volume *= Game.Volume;
			source.Play();
		}

		public static void PlaySound(string sound, Vector3 where)
		{
			Spawn("SoundObject", where).TryGetComponent(out AudioSource source);
			source.Stop();
			source.volume *= Game.Volume;
			source.clip = GameManager.GetSound(sound);
			source.Play();
		}

		public void PlaySound(AudioClip sound, Vector3 where, bool twodee = false, float volume = 1, float maxDistance = 90, float minDistance = 25, bool respectVolume = true)
		{
			Spawn("SoundObject", where).TryGetComponent(out AudioSource source);
			source.Stop();
			source.clip = sound;
			if (twodee) source.spatialBlend = 0;
			if (respectVolume) volume *= Game.Volume;
			source.volume = volume;
			source.maxDistance = maxDistance;
			source.minDistance = minDistance;
			source.Play();
		}

		public void PlaySound(string sound, Vector3 where, bool twodee = true, float volume = 1, float maxDistance = 90, float minDistance = 25, bool respectVolume = true)
		{
			Spawn("SoundObject", where).TryGetComponent(out AudioSource source);
			source.Stop();
			source.clip = GameManager.GetSound(sound);
			if (twodee) source.spatialBlend = 0;
			if (respectVolume) volume *= Game.Volume;
			source.volume = volume;
			source.maxDistance = maxDistance;
			source.minDistance = minDistance;
			source.Play();
		}

		//Here's the fun part.
		//Get pool by name. Retrieve Game Object from pool. Place it at desired location, and enable it.
		public static GameObject Spawn(string obj, Vector3 where, Quaternion rotation)
		{
			GameObject poolObject = GameManager.GetPool(obj).GetPoolObject();
			if (!poolObject)
			{
				Debug.LogError("Could not find object of name " + obj);
				return null;
			}

			poolObject.transform.position = where;
			poolObject.transform.rotation = rotation;
			if (poolObject.GetRigidbody()) poolObject.GetRigidbody().ResetVelocity();
			poolObject.SetActive(true);
			return poolObject;
		}

		public static GameObject Spawn(string obj, Vector3 where, Vector3 rotation)
		{
			GameObject poolObject = GameManager.GetPool(obj).GetPoolObject();
			if (!poolObject)
			{
				Debug.LogError("Could not find object of name " + obj);
				return null;
			}

			poolObject.transform.position = where;
			poolObject.transform.eulerAngles = rotation;
			if (poolObject.GetRigidbody()) poolObject.GetRigidbody().ResetVelocity();
			poolObject.SetActive(true);
			return poolObject;
		}

		public static GameObject Spawn(string obj, Vector3 where)
		{
			GameObject poolObject = GameManager.GetPool(obj).GetPoolObject();
			if (!poolObject)
			{
				Debug.LogError("Could not find object of name " + obj);
				return null;
			}

			poolObject.transform.position = where;
			poolObject.transform.rotation = Quaternion.identity;
			if (poolObject.GetRigidbody()) poolObject.GetRigidbody().ResetVelocity();
			poolObject.SetActive(true);
			return poolObject;
		}

		public void CreateDebrisSpray(Vector3 where, int amount = 30, float force = 50)
		{
			int pick = 0;
			for (int i = 0; i < amount; i++)
			{
				pick = random(1, 3);

				Spawn("Debris" + pick, where).GetRigidbody().RandomizeVelocity(force);
			}
		}

		public void MakeTracer(Vector3 start, Vector3 end, float width, float fadeSpeed = 12, bool StartFollowsEnd = true, float TracerSpeed = 25, Material mat = null)
		{
			Spawn("TracerObject", start).TryGetComponent(out LineRenderer line);
			line.TryGetComponent(out BulletTracer bt);
			line.SetPosition(0, start);
			line.SetPosition(1, end);
			line.endWidth = width;
			line.startWidth = width;
			bt.start = start;
			bt.end = end;
			bt.TracerSpeed = TracerSpeed;
			bt.StartFollowsEnd = StartFollowsEnd;
			bt.FadeSpeed = fadeSpeed;
			bt.width = width;
			if (mat) line.sharedMaterial = mat;
		}

		public void TryMakeHitEffects(Entity on, int init_dmg, Vector3 point)
		{
			if (!on)
			{
				Spawn("BulletSparks", point);
				return;
			}
			if (on.Armor != 0)
			{
				CreateDebrisSpray(point, Entity.GetFinalDamage(on, init_dmg), 10 + init_dmg);
			}
			if (Entity.PenetratesArmor(on, init_dmg))
			{
				if (on.CanBleed)
				{
					Spawn("HitBloodsplatter", point);
					PlaySound("Hit2", point, false, 0.22f, 50, 25);
				}
			}
			else
			{
				Spawn("BulletSparks", point);
				PlaySound("crit", transform.position, false, 0.65f, 69, 0);
				PlaySound("Hit2", point, true, 0.85f, 60, 25);
				CreateDebrisSpray(point, (int)(on.Armor * 0.4f), 10 + init_dmg);
			}
		}
	}
}
