using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace TooMuchLightTrainer.Core;

public class MonoResolver
{
	private struct MEMORY_BASIC_INFORMATION
	{
		public IntPtr BaseAddress;

		public IntPtr AllocationBase;

		public uint AllocationProtect;

		public IntPtr RegionSize;

		public uint State;

		public uint Protect;

		public uint Type;
	}

	public static class DevFields
	{
		public static int InfAmmo => _instance?.OffDev_InfAmmo ?? (-1);

		public static int InfStamina => _instance?.OffDev_InfStam ?? (-1);

		public static int EnemyAI => _instance?.OffDev_EnemyAI ?? (-1);
	}

	public static class HealthFields
	{
		public static int CurrHP => _instance?.OffHP_Current ?? (-1);

		public static int MaxHP => _instance?.OffHP_Max ?? (-1);

		public static int CurrStam => _instance?.OffStam_Current ?? (-1);

		public static int MaxStam => _instance?.OffStam_Max ?? (-1);
	}

	public static class StateFields
	{
		public static int Walk = 0x54;
		public static int Run = 0x58;
		public static int Sprint = 0x5C;
		public static int Aim = 0x7E;
	}

	public static class CameraRecoilFields
	{
		public const int CS_SecsToReturn = 0x2C;
		public const int CS_SprayEfficiency = 0x30;
		public const int CS_MaxUpperOffset = 0x34;
		public const int CS_DelayBeforeReturn = 0x38;
		public const int CS_ShootingMisstake = 0x3C;
		public const int CS_TargetOffsetShootingX = 0x40;
		public const int CS_TargetOffsetShootingY = 0x44;
		public const int CS_TargetOffsetShootingZ = 0x48;
		public const int CS_CurrentOffsetShootingX = 0x4C;
		public const int CS_CurrentOffsetShootingY = 0x50;
		public const int CS_CurrentOffsetShootingZ = 0x54;
		public const int Snappiness = 0x6C;
		public const int ReturnSpeed = 0x70;
		public const int CurrentRotationX = 0x74;
		public const int CurrentRotationY = 0x78;
		public const int CurrentRotationZ = 0x7C;
		public const int TargetRotationX = 0x80;
		public const int TargetRotationY = 0x84;
		public const int TargetRotationZ = 0x88;
		public const int CurrentRecoilX = 0x8C;
		public const int CurrentRecoilY = 0x90;
		public const int CurrentRecoilZ = 0x94;
	}

	public static class TakedownFields
	{
		public static int TimeBeforeHSceneTransition = 0x170;
		public static int CurrentResistance = 0x16C;
	}

	public static class PowerHandlerFields
	{
		public static int EvasionChance = 0x3C;
		public static int KnockdownAvoidChance = 0x4C;
	}

	[StructLayout(LayoutKind.Sequential, Size = 1)]
	public struct SkillSystemFields
	{
		public const int CurrentLevel = 232;

		public const int CurrentExp = 240;

		public const int CurrentRepLevel = 252;

		public const int CurrentRepExp = 260;

		public const int CurrentSkillPoints = 360;
	}

	/// <summary>
	/// Offsets for Shine.DateTime MonoBehaviour (from TooMuchLight_SDK.h)
	/// </summary>
	public static class DateTimeFields
	{
		// Singleton.<dateTime>k__BackingField offset from SFD
		public const int SingletonSfdOffset = 0xA8;

		// Shine.DateTime field offsets
		public const int TimeOfDay       = 0xD0;  // float  — 0.0–24.0
		public const int CanUpdateCycle  = 0x109; // bool   — true = time runs
		public const int UpdateEnabled   = 0x108; // bool   — day/night update enabled
		public const int DayNumber       = 0x128; // int32  — current day
		public const int SunriseTime     = 0xE0;  // float  — default 7.0
		public const int SunsetTime      = 0xE4;  // float  — default 18.0
		public const int BaseTimeSpeed   = 0x118; // float  — base speed multiplier
		public const int StarsMultiplier = 0xD8;  // float
	}

	public static class WeaponFields
	{
		public const int Damage = 0xD0;
		public const int RecoilX = 0xD4;
		public const int RecoilY = 0xD8;
		public const int RecoilZ = 0xDC;
		public const int MaxSpread = 0xE0;
		public const int KickBackForce = 0xE8;
		public const int ShotgunSpread = 0x100;
		public const int AnimatedRecoilX = 0x10C;
		public const int AnimatedRecoilY = 0x110;
		public const int AnimatedRecoilZ = 0x114;
		public const int AnimatedKickback = 0x118;
	}

	public static class RangeCombatFields
	{
		public const int Weapon = 0x20;
		public const int GunSpreadSpeed = 0x7C;
		public const int GunSpreadReturnSpeed = 0x80;
		public const int CurrentSpread = 0xC8;
	}

	private readonly MemoryReader _mem;

	private IntPtr _proc = IntPtr.Zero;

	private IntPtr _clsHealthComp = IntPtr.Zero;

	private IntPtr _clsDev = IntPtr.Zero;

	private IntPtr _clsSingleton = IntPtr.Zero;

	private IntPtr _clsShineSingleton = IntPtr.Zero;

	private IntPtr _clsReferencesHandler = IntPtr.Zero;

	private IntPtr _clsRangeComp = IntPtr.Zero;

	private IntPtr _clsSkillSystem = IntPtr.Zero;

	private IntPtr _clsArousal = IntPtr.Zero;

	private IntPtr _clsInvComp = IntPtr.Zero;

	private IntPtr _imagePtr = IntPtr.Zero;

	public static readonly List<string> FieldDump = new List<string>();

	public static readonly List<string> SfdSlots = new List<string>();

	private static MonoResolver? _instance;

	private const uint MEM_COMMIT = 4096u;

	private const uint PAGE_NOACCESS = 1u;

	private const uint PAGE_GUARD = 256u;

	public int OffHP_Current { get; private set; } = 0x30;
	public int OffHP_Max { get; private set; } = 0x34;
	public int OffStam_Current { get; private set; } = 0x3C;
	public int OffStam_Max { get; private set; } = 0x40;
	public int OffDev_InfAmmo { get; private set; } = 0x68;
	public int OffDev_InfStam { get; private set; } = 0x69;
	public int OffDev_EnemyAI { get; private set; } = 0x6A;


	public int OffAmmo_Current { get; private set; } = 140;


	public int OffSkill_Level { get; private set; } = 232;


	public int OffSkill_Exp { get; private set; } = 240;


	public int OffSkill_RepLv { get; private set; } = 252;


	public int OffArousal_Val { get; private set; } = 44;


	[DllImport("kernel32.dll")]
	private static extern bool ReadProcessMemory(IntPtr h, IntPtr addr, byte[] buf, int cb, out IntPtr read);

	public MonoResolver(MemoryReader mem)
	{
		_mem = mem;
	}

	public bool Initialize()
	{
		_proc = _mem.ProcessHandle;
		if (_proc == IntPtr.Zero)
		{
			TrainerLog.Error("[Mono] No process handle");
			return false;
		}
		var (intPtr, value) = _mem.GetModuleBase("mono-2.0-bdwgc.dll");
		if (intPtr == IntPtr.Zero)
		{
			TrainerLog.Error("[Mono] mono-2.0-bdwgc.dll not found");
			return false;
		}
		TrainerLog.Info($"[Mono] DLL base=0x{intPtr.ToInt64():X}  size=0x{value:X}");
		IntPtr intPtr2 = FindRootDomain(intPtr);
		if (intPtr2 == IntPtr.Zero)
		{
			TrainerLog.Error("[Mono] Root domain not found");
			return false;
		}
		TrainerLog.Info($"[Mono] Root domain: 0x{intPtr2.ToInt64():X}");
		_imagePtr = FindAssemblyCSharp(intPtr2);
		if (_imagePtr == IntPtr.Zero)
		{
			TrainerLog.Error("[Mono] Assembly-CSharp image not found");
			return false;
		}
		TrainerLog.Info($"[Mono] Assembly-CSharp image: 0x{_imagePtr.ToInt64():X}");
		_clsHealthComp = FindClass(_imagePtr, "Shine", "HealthComponent");
		_clsDev = FindClass(_imagePtr, "Shine", "Dev");
		_clsSingleton = FindClass(_imagePtr, "Shine", "Singleton`1");
		_clsShineSingleton = FindClass(_imagePtr, "Shine", "Singleton");
		_clsReferencesHandler = FindClass(_imagePtr, "Shine", "ReferencesHandler");
		_clsRangeComp = FindClass(_imagePtr, "Shine", "RangeCombatComponent");
		_clsSkillSystem = FindClass(_imagePtr, "Shine", "SkillSystem");
		_clsArousal = FindClass(_imagePtr, "Shine", "ArousalHandler");
		_clsInvComp = FindClass(_imagePtr, "Shine", "InventoryComponent");
		TrainerLog.Info($"[Mono] HealthComp=0x{_clsHealthComp.ToInt64():X}  Dev=0x{_clsDev.ToInt64():X}  Singleton=0x{_clsSingleton.ToInt64():X}");
		if (_clsHealthComp == IntPtr.Zero || _clsDev == IntPtr.Zero)
		{
			if (_clsHealthComp == IntPtr.Zero)
			{
				_clsHealthComp = FindClass(_imagePtr, "", "HealthComponent");
			}
			if (_clsDev == IntPtr.Zero)
			{
				_clsDev = FindClass(_imagePtr, "", "Dev");
			}
			TrainerLog.Warn($"[Mono] Namespace fallback: HealthComp=0x{_clsHealthComp.ToInt64():X}  Dev=0x{_clsDev.ToInt64():X}");
		}
		if (_clsHealthComp == IntPtr.Zero || _clsDev == IntPtr.Zero)
		{
			TrainerLog.Error("[Mono] Key classes not found");
			return false;
		}
		TrainerLog.Info("[Mono] === HealthComponent fields ===");
		DumpAllFields(_clsHealthComp);
		TrainerLog.Info("[Mono] === Dev fields ===");
		DumpAllFields(_clsDev);
		if (_clsSingleton != IntPtr.Zero)
		{
			TrainerLog.Info("[Mono] === Singleton fields ===");
			DumpAllFields(_clsSingleton);
		}
		ResolveFieldOffsets();
		TrainerLog.Info($"[Mono] HP={OffHP_Current:X}/{OffHP_Max:X}  Stam={OffStam_Current:X}/{OffStam_Max:X}  InfAmmo={OffDev_InfAmmo:X}  InfStam={OffDev_InfStam:X}  AI={OffDev_EnemyAI:X}");
		_instance = this;
		return true;
	}

	private void DumpAllFields(IntPtr klass, int depth = 0)
	{
		if (!IsValidPtr(klass) || depth > 8)
		{
			return;
		}
		IntPtr intPtr = ReadPtr(klass + 48);
		if (IsValidPtr(intPtr) && intPtr != klass)
		{
			DumpAllFields(intPtr, depth + 1);
		}
		IntPtr intPtr2 = ReadPtr(klass + 72);
		string text = (IsValidPtr(intPtr2) ? ReadCStr(intPtr2, 64) : "??");
		bool flag = text == "HealthComponent" || text == "Dev" || text == "Singleton`1";
		IntPtr intPtr3 = ReadPtr(klass + 152);
		int num = (int)ReadU32(klass + 256);
		if (!IsValidPtr(intPtr3) || num <= 0 || num > 2048)
		{
			string text2 = text + ": no fields";
			TrainerLog.Info("[Mono]   " + text2);
			if (flag)
			{
				FieldDump.Add(text2);
			}
			return;
		}
		for (int i = 0; i < num; i++)
		{
			IntPtr intPtr4 = intPtr3 + i * 32;
			IntPtr intPtr5 = ReadPtr(intPtr4 + 8);
			if (IsValidPtr(intPtr5))
			{
				string value = ReadCStr(intPtr5, 128);
				if (string.IsNullOrEmpty(value))
				{
					break;
				}
				int value2 = ReadI32(intPtr4 + 24);
				string text3 = $"{text}.{value} +0x{value2:X}";
				TrainerLog.Info("[Mono]   " + text3);
				if (flag)
				{
					FieldDump.Add(text3);
				}
			}
		}
	}

	public IntPtr GetReferencesHandlerPtr()
	{
		if (_clsShineSingleton == IntPtr.Zero)
		{
			return IntPtr.Zero;
		}
		IntPtr staticFieldData = GetStaticFieldData(_clsShineSingleton);
		if (!IsValidPtr(staticFieldData))
		{
			return IntPtr.Zero;
		}
		int fieldOffset = GetFieldOffset(_clsShineSingleton, "<References>k__BackingField");
		if (fieldOffset < 0)
		{
			return IntPtr.Zero;
		}
		return ReadPtr(staticFieldData + fieldOffset);
	}

	public bool TryFindSingletonInstances(out IntPtr devPtr, out IntPtr hcPtr, out IntPtr rcPtr, out IntPtr skillPtr, out IntPtr arPtr, out IntPtr invPtr, out IntPtr statePtr)
	{
		devPtr = (hcPtr = (rcPtr = (skillPtr = (arPtr = (invPtr = (statePtr = IntPtr.Zero))))));
		if (_clsShineSingleton == IntPtr.Zero)
		{
			return false;
		}
		IntPtr staticFieldData = GetStaticFieldData(_clsShineSingleton);
		if (!IsValidPtr(staticFieldData))
		{
			return false;
		}
		int fieldOffset = GetFieldOffset(_clsShineSingleton, "<DevComponent>k__BackingField");
		int fieldOffset2 = GetFieldOffset(_clsShineSingleton, "<References>k__BackingField");
		int fieldOffset3 = GetFieldOffset(_clsShineSingleton, "<Inventory>k__BackingField");
		int fieldOffset4 = GetFieldOffset(_clsShineSingleton, "<skillSystem>k__BackingField");
		int fieldOffset5 = GetFieldOffset(_clsShineSingleton, "<arousalHandler>k__BackingField");
		if (fieldOffset >= 0)
		{
			devPtr = ReadPtr(staticFieldData + fieldOffset);
		}
		if (fieldOffset3 >= 0)
		{
			invPtr = ReadPtr(staticFieldData + fieldOffset3);
		}
		if (fieldOffset4 >= 0)
		{
			skillPtr = ReadPtr(staticFieldData + fieldOffset4);
		}
		if (fieldOffset5 >= 0)
		{
			arPtr = ReadPtr(staticFieldData + fieldOffset5);
		}
		// Resolve DateTime pointer from <dateTime>k__BackingField at SFD offset 0xA8
		IntPtr dtPtr = ReadPtr(staticFieldData + DateTimeFields.SingletonSfdOffset);
		if (IsValidPtr(dtPtr))
		{
			TrainerState.DateTimePtr = dtPtr;
		}
		int fieldOffsetTakedown = GetFieldOffset(_clsShineSingleton, "<GirlTakedown>k__BackingField");
		if (fieldOffsetTakedown >= 0)
		{
			TrainerState.TakedownPtr = ReadPtr(staticFieldData + fieldOffsetTakedown);
		}
		int fieldOffsetPower = GetFieldOffset(_clsShineSingleton, "<girlPower>k__BackingField");
		if (fieldOffsetPower >= 0)
		{
			TrainerState.PowerPtr = ReadPtr(staticFieldData + fieldOffsetPower);
		}
		IntPtr intPtr = ((fieldOffset2 >= 0) ? ReadPtr(staticFieldData + fieldOffset2) : IntPtr.Zero);
		if (IsValidPtr(intPtr) && _clsReferencesHandler != IntPtr.Zero)
		{
			int fieldOffset6 = GetFieldOffset(_clsReferencesHandler, "<GirlHealth>k__BackingField");
			int fieldOffset7 = GetFieldOffset(_clsReferencesHandler, "<RangeCombat>k__BackingField");
			int fieldOffset8 = GetFieldOffset(_clsReferencesHandler, "<GirlState>k__BackingField");
			int fieldOffsetCamRecoil = GetFieldOffset(_clsReferencesHandler, "<CamRecoil>k__BackingField");
			if (fieldOffset6 >= 0)
			{
				hcPtr = ReadPtr(intPtr + fieldOffset6);
			}
			if (fieldOffset7 >= 0)
			{
				rcPtr = ReadPtr(intPtr + fieldOffset7);
			}
			if (fieldOffset8 >= 0)
			{
				statePtr = ReadPtr(intPtr + fieldOffset8);
			}
			if (fieldOffsetCamRecoil >= 0)
			{
				TrainerState.CamRecoilPtr = ReadPtr(intPtr + fieldOffsetCamRecoil);
			}
		}
		if (devPtr != IntPtr.Zero || hcPtr != IntPtr.Zero)
		{
			return true;
		}
		return false;
	}

	public bool TryFindDevInstance(out IntPtr devPtr)
	{
		devPtr = IntPtr.Zero;
		if (_clsDev == IntPtr.Zero)
		{
			return false;
		}
		IntPtr monoVTable = GetMonoVTable(_clsDev);
		if (monoVTable == IntPtr.Zero)
		{
			TrainerLog.Warn("[Mono] Dev vtable not found");
			return false;
		}
		TrainerLog.Info($"[Mono] Dev vtable: 0x{monoVTable.ToInt64():X}");
		List<IntPtr> list = ScanHeapForObjectVtable(monoVTable);
		TrainerLog.Info($"[Mono] Dev heap candidates: {list.Count}");
		using (List<IntPtr>.Enumerator enumerator = list.GetEnumerator())
		{
			if (enumerator.MoveNext())
			{
				IntPtr current = enumerator.Current;
				devPtr = current;
				TrainerLog.Info($"[Mono] Dev instance: 0x{devPtr.ToInt64():X}");
				return true;
			}
		}
		TrainerLog.Warn("[Mono] Dev instance not found in heap");
		return false;
	}

	public bool TryFindSkillSystem(out IntPtr skillPtr)
	{
		skillPtr = IntPtr.Zero;
		if (_clsSkillSystem == IntPtr.Zero)
		{
			return false;
		}
		IntPtr monoVTable = GetMonoVTable(_clsSkillSystem);
		if (monoVTable == IntPtr.Zero)
		{
			return false;
		}
		List<IntPtr> list = ScanHeapForObjectVtable(monoVTable);
		TrainerLog.Info($"[Mono] SkillSystem candidates: {list.Count}");
		foreach (IntPtr item in list)
		{
			int num = _mem.ReadInt32(item + OffSkill_Level);
			if (num >= 1 && num <= 100)
			{
				skillPtr = item;
				return true;
			}
		}
		return false;
	}

	public bool TryFindRangeComp(out IntPtr rcPtr)
	{
		rcPtr = IntPtr.Zero;
		if (_clsRangeComp == IntPtr.Zero)
		{
			return false;
		}
		IntPtr monoVTable = GetMonoVTable(_clsRangeComp);
		if (monoVTable == IntPtr.Zero)
		{
			return false;
		}
		List<IntPtr> list = ScanHeapForObjectVtable(monoVTable);
		TrainerLog.Info($"[Mono] RangeComp candidates: {list.Count}");
		foreach (IntPtr item in list)
		{
			int num = _mem.ReadInt32(item + OffAmmo_Current);
			if (num >= 0 && num <= 1000)
			{
				rcPtr = item;
				return true;
			}
		}
		return false;
	}

	public bool TryFindArousal(out IntPtr arPtr)
	{
		arPtr = IntPtr.Zero;
		if (_clsArousal == IntPtr.Zero)
		{
			return false;
		}
		IntPtr monoVTable = GetMonoVTable(_clsArousal);
		if (monoVTable == IntPtr.Zero)
		{
			return false;
		}
		List<IntPtr> list = ScanHeapForObjectVtable(monoVTable);
		TrainerLog.Info($"[Mono] Arousal candidates: {list.Count}");
		if (list.Count > 0)
		{
			arPtr = list[0];
			return true;
		}
		return false;
	}

	public bool TryFindHealthComponent(out IntPtr hcPtr)
	{
		hcPtr = IntPtr.Zero;
		if (_clsHealthComp == IntPtr.Zero || OffHP_Current < 0)
		{
			return false;
		}
		IntPtr monoVTable = GetMonoVTable(_clsHealthComp);
		if (monoVTable == IntPtr.Zero)
		{
			TrainerLog.Warn("[Mono] HC vtable not found");
			return false;
		}
		TrainerLog.Info($"[Mono] HC vtable: 0x{monoVTable.ToInt64():X}");
		List<IntPtr> list = ScanHeapForObjectVtable(monoVTable);
		TrainerLog.Info($"[Mono] HC heap candidates: {list.Count}");
		foreach (IntPtr item in list)
		{
			float num = _mem.ReadFloat(item + OffHP_Current);
			float num2 = _mem.ReadFloat(item + OffHP_Max);
			if (num > 0f && num <= 10000f && num2 > 0f && num2 <= 10000f && num <= num2)
			{
				hcPtr = item;
				TrainerLog.Info($"[Mono] HealthComponent: 0x{item.ToInt64():X}  HP={num}/{num2}");
				return true;
			}
		}
		return false;
	}

	public bool TryFindInventoryComponent(out IntPtr invPtr)
	{
		invPtr = IntPtr.Zero;
		if (_clsInvComp == IntPtr.Zero)
		{
			return false;
		}
		IntPtr monoVTable = GetMonoVTable(_clsInvComp);
		if (monoVTable == IntPtr.Zero)
		{
			return false;
		}
		List<IntPtr> list = ScanHeapForObjectVtable(monoVTable);
		TrainerLog.Info($"[Mono] InvComp candidates: {list.Count}");
		foreach (IntPtr item in list)
		{
			IntPtr p = ReadPtr(item + 952);
			if (IsValidPtr(p))
			{
				invPtr = item;
				return true;
			}
		}
		return false;
	}

	public bool TryFindSaveTempContainer(out IntPtr stcPtr)
	{
		stcPtr = IntPtr.Zero;
		if (_imagePtr == IntPtr.Zero) return false;
		IntPtr cls = FindClass(_imagePtr, "Shine", "SaveTempContainer");
		if (cls == IntPtr.Zero) cls = FindClass(_imagePtr, "", "SaveTempContainer");
		if (cls == IntPtr.Zero) return false;
		IntPtr vtable = GetMonoVTable(cls);
		if (vtable == IntPtr.Zero) return false;
		var list = ScanHeapForObjectVtable(vtable);
		if (list.Count > 0)
		{
			stcPtr = list[0];
			return true;
		}
		return false;
	}

	public bool TryFindGUIHandler(out IntPtr guiPtr)
	{
		guiPtr = IntPtr.Zero;
		if (_clsShineSingleton == IntPtr.Zero) return false;
		IntPtr staticFieldData = GetStaticFieldData(_clsShineSingleton);
		if (!IsValidPtr(staticFieldData)) return false;
		int fieldOffset = GetFieldOffset(_clsShineSingleton, "<GUI>k__BackingField");
		if (fieldOffset < 0) fieldOffset = 0x20;
		guiPtr = ReadPtr(staticFieldData + fieldOffset);
		return IsValidPtr(guiPtr);
	}

	public bool TryReadEntityPosition(IntPtr entityPtr, out float x, out float y, out float z)
	{
		x = (y = (z = 0f));
		if (!IsValidPtr(entityPtr))
		{
			return false;
		}

		// Method 1: Component.m_CachedPtr (entityPtr + 0x10) -> native Transform (+0x30) -> transformData (+0x8) -> position (+88, +92, +96)
		IntPtr nativeComp = ReadPtr(entityPtr + 16);
		if (IsValidPtr(nativeComp))
		{
			IntPtr nativeTransform = ReadPtr(nativeComp + 48);
			if (IsValidPtr(nativeTransform))
			{
				IntPtr transformData = ReadPtr(nativeTransform + 8);
				if (IsValidPtr(transformData))
				{
					x = _mem.ReadFloat(transformData + 88);
					y = _mem.ReadFloat(transformData + 92);
					z = _mem.ReadFloat(transformData + 96);
					if (x != 0f || y != 0f || z != 0f)
					{
						return true;
					}
				}

				x = _mem.ReadFloat(nativeTransform + 88);
				y = _mem.ReadFloat(nativeTransform + 92);
				z = _mem.ReadFloat(nativeTransform + 96);
				if (x != 0f || y != 0f || z != 0f)
				{
					return true;
				}
			}
		}

		// Method 2: Managed _transform field (offset 168) -> m_CachedPtr (+0x10) -> transformData (+0x8) -> position
		int offTransform = 168;
		IntPtr managedTransform = ReadPtr(entityPtr + offTransform);
		if (IsValidPtr(managedTransform))
		{
			IntPtr internalTransform = ReadPtr(managedTransform + 16);
			if (IsValidPtr(internalTransform))
			{
				IntPtr transformData2 = ReadPtr(internalTransform + 8);
				if (IsValidPtr(transformData2))
				{
					x = _mem.ReadFloat(transformData2 + 88);
					y = _mem.ReadFloat(transformData2 + 92);
					z = _mem.ReadFloat(transformData2 + 96);
					if (x != 0f || y != 0f || z != 0f)
					{
						return true;
					}
				}

				x = _mem.ReadFloat(internalTransform + 88);
				y = _mem.ReadFloat(internalTransform + 92);
				z = _mem.ReadFloat(internalTransform + 96);
				if (x != 0f || y != 0f || z != 0f)
				{
					return true;
				}
			}
		}

		// Method 3: Fallback to LastPos fields (0x2A8, 0x2AC, 0x2B0)
		x = _mem.ReadFloat(entityPtr + 680);
		y = _mem.ReadFloat(entityPtr + 684);
		z = _mem.ReadFloat(entityPtr + 688);
		return x != 0f || y != 0f || z != 0f;
	}

	private IntPtr FindRootDomain(IntPtr monoBase)
	{
		uint num = ReadU32(monoBase + 60);
		uint num2 = ReadU32(monoBase + (int)num + 136);
		if (num2 == 0)
		{
			return IntPtr.Zero;
		}
		IntPtr intPtr = monoBase + (int)num2;
		uint num3 = ReadU32(intPtr + 24);
		uint num4 = ReadU32(intPtr + 28);
		uint num5 = ReadU32(intPtr + 32);
		uint num6 = ReadU32(intPtr + 36);
		for (uint num7 = 0u; num7 < num3 && num7 < 10000; num7++)
		{
			uint num8 = ReadU32(monoBase + (int)num5 + (int)(num7 * 4));
			string text = ReadCStr(monoBase + (int)num8, 40);
			if (!(text != "mono_get_root_domain"))
			{
				ushort num9 = ReadU16(monoBase + (int)num6 + (int)(num7 * 2));
				uint num10 = ReadU32(monoBase + (int)num4 + num9 * 4);
				IntPtr intPtr2 = monoBase + (int)num10;
				TrainerLog.Info($"[Mono] mono_get_root_domain at 0x{intPtr2.ToInt64():X}");
				byte[] array = ReadBytes(intPtr2, 16);
				if (array[0] == 233)
				{
					int num11 = BitConverter.ToInt32(array, 1);
					intPtr2 = intPtr2 + 5 + num11;
					array = ReadBytes(intPtr2, 16);
					TrainerLog.Info($"[Mono] JMP stub AƒA¢A¢â,¬A\u00a0A¢â,¬â,¢ 0x{intPtr2.ToInt64():X}");
				}
				if (array[0] == 72 && array[1] == 139 && (array[2] == 5 || array[2] == 13))
				{
					int num12 = BitConverter.ToInt32(array, 3);
					IntPtr addr = intPtr2 + 7 + num12;
					IntPtr intPtr3 = ReadPtr(addr);
					if (IsValidPtr(intPtr3))
					{
						TrainerLog.Info($"[Mono] Domain global=0x{addr.ToInt64():X}  domain=0x{intPtr3.ToInt64():X}");
						return intPtr3;
					}
					break;
				}
				break;
			}
		}
		return IntPtr.Zero;
	}

	private IntPtr FindAssemblyCSharp(IntPtr rootDomain)
	{
		int[] array = new int[10] { 160, 152, 168, 176, 144, 136, 200, 192, 208, 184 };
		int[] array2 = new int[4] { 16, 8, 24, 0 };
		int[] array3 = new int[4] { 96, 88, 104, 80 };
		int[] array4 = array;
		foreach (int num in array4)
		{
			IntPtr intPtr = ReadPtr(rootDomain + num);
			if (!IsValidPtr(intPtr) || !IsValidPtr(ReadPtr(intPtr)))
			{
				continue;
			}
			int num2 = 0;
			IntPtr intPtr2 = intPtr;
			while (IsValidPtr(intPtr2) && num2++ < 512)
			{
				IntPtr intPtr3 = ReadPtr(intPtr2);
				if (!IsValidPtr(intPtr3))
				{
					break;
				}
				int[] array5 = array2;
				foreach (int num3 in array5)
				{
					IntPtr intPtr4 = ReadPtr(intPtr3 + num3);
					if (!IsValidPtr(intPtr4))
					{
						continue;
					}
					string text = ReadCStr(intPtr4, 128);
					if (!text.Contains("Assembly-CSharp"))
					{
						continue;
					}
					int[] array6 = array3;
					foreach (int num4 in array6)
					{
						IntPtr intPtr5 = ReadPtr(intPtr3 + num4);
						if (IsValidPtr(intPtr5))
						{
							TrainerLog.Info($"[Mono] Assembly-CSharp domOff=+0x{num:X} nameOff=+0x{num3:X} imgOff=+0x{num4:X} img=0x{intPtr5.ToInt64():X}");
							return intPtr5;
						}
					}
				}
				intPtr2 = ReadPtr(intPtr2 + 8);
			}
		}
		TrainerLog.Error("[Mono] Assembly-CSharp not found in domain");
		return IntPtr.Zero;
	}

	private IntPtr FindClass(IntPtr image, string ns, string className)
	{
		if (!IsValidPtr(image))
		{
			return IntPtr.Zero;
		}
		IntPtr intPtr = image + 1232;
		int num = (int)ReadU32(intPtr + 24);
		IntPtr intPtr2 = ReadPtr(intPtr + 32);
		if (!IsValidPtr(intPtr2) || num <= 0 || num > 65536)
		{
			TrainerLog.Warn($"[Mono] class_cache invalid: ptr=0x{intPtr2.ToInt64():X} size={num}");
			int[] array = new int[6] { 1216, 1232, 1200, 1168, 1152, 976 };
			int[] array2 = array;
			foreach (int num2 in array2)
			{
				intPtr = image + num2;
				num = (int)ReadU32(intPtr + 24);
				intPtr2 = ReadPtr(intPtr + 32);
				if (IsValidPtr(intPtr2) && num > 0 && num <= 65536)
				{
					break;
				}
			}
			if (!IsValidPtr(intPtr2) || num <= 0)
			{
				return IntPtr.Zero;
			}
		}
		for (int j = 0; j < num; j++)
		{
			IntPtr intPtr3 = ReadPtr(intPtr2 + j * 8);
			int num3 = 0;
			while (IsValidPtr(intPtr3) && num3++ < 128)
			{
				IntPtr intPtr4 = ReadPtr(intPtr3 + 72);
				IntPtr intPtr5 = ReadPtr(intPtr3 + 80);
				if (IsValidPtr(intPtr4))
				{
					string text = ReadCStr(intPtr4, 64);
					string text2 = (IsValidPtr(intPtr5) ? ReadCStr(intPtr5, 128) : "");
					if (text == className && (string.IsNullOrEmpty(ns) || string.IsNullOrEmpty(text2) || text2 == ns))
					{
						TrainerLog.Info($"[Mono] Found '{text2}'.'{text}' at 0x{intPtr3.ToInt64():X}");
						return intPtr3;
					}
				}
				intPtr3 = ReadPtr(intPtr3 + 264);
			}
		}
		return IntPtr.Zero;
	}

	private IntPtr GetMonoVTable(IntPtr klass)
	{
		if (!IsValidPtr(klass))
		{
			return IntPtr.Zero;
		}
		IntPtr intPtr = ReadPtr(klass + 208);
		if (!IsValidPtr(intPtr))
		{
			TrainerLog.Warn($"[Mono] GetMonoVTable: klass=0x{klass.ToInt64():X} rti=NULL");
			return IntPtr.Zero;
		}
		for (int i = 0; i < 4; i++)
		{
			IntPtr intPtr2 = ReadPtr(intPtr + 8 + i * 8);
			if (IsValidPtr(intPtr2))
			{
				IntPtr intPtr3 = ReadPtr(intPtr2);
				TrainerLog.Info($"[Mono] VTable dom[{i}] vt=0x{intPtr2.ToInt64():X}  vtKlass=0x{intPtr3.ToInt64():X}  want=0x{klass.ToInt64():X}  match={intPtr3 == klass}");
				if (intPtr3 == klass)
				{
					return intPtr2;
				}
			}
		}
		TrainerLog.Warn($"[Mono] GetMonoVTable: no matching vtable for klass=0x{klass.ToInt64():X}");
		return IntPtr.Zero;
	}

	private IntPtr GetStaticFieldData(IntPtr klass)
	{
		if (!IsValidPtr(klass))
		{
			return IntPtr.Zero;
		}
		IntPtr intPtr = ReadPtr(klass + 208);
		if (!IsValidPtr(intPtr))
		{
			return IntPtr.Zero;
		}
		for (int i = 0; i < 4; i++)
		{
			IntPtr intPtr2 = ReadPtr(intPtr + 8 + i * 8);
			IntPtr intPtr3 = (IsValidPtr(intPtr2) ? ReadPtr(intPtr2) : IntPtr.Zero);
			bool flag = intPtr3 == klass;
			byte b = (byte)(IsValidPtr(intPtr2) ? ReadByte(intPtr2 + 48) : 0);
			int num = (flag ? ReadI32(klass + 92) : (-1));
			IntPtr intPtr4 = ((flag && (b & 4u) != 0 && num >= 0 && num < 2048) ? ReadPtr(intPtr2 + 72 + num * 8) : IntPtr.Zero);
			if (flag && (b & 4u) != 0 && IsValidPtr(intPtr4))
			{
				return intPtr4;
			}
		}
		return IntPtr.Zero;
	}

	private void ResolveFieldOffsets()
	{
		WalkFields(_clsHealthComp, new(string, Action<int>)[5]
		{
			("__currentHealth", delegate(int v)
			{
				OffHP_Current = v;
			}),
			("currentHealth", delegate(int v)
			{
				if (OffHP_Current < 0)
				{
					OffHP_Current = v;
				}
			}),
			("maxHealth", delegate(int v)
			{
				OffHP_Max = v;
			}),
			("currentStamina", delegate(int v)
			{
				OffStam_Current = v;
			}),
			("maxStamina", delegate(int v)
			{
				OffStam_Max = v;
			})
		});
		WalkFields(_clsDev, new(string, Action<int>)[6]
		{
			("infinityAmmo", delegate(int v)
			{
				OffDev_InfAmmo = v;
			}),
			("infiniteAmmo", delegate(int v)
			{
				if (OffDev_InfAmmo < 0)
				{
					OffDev_InfAmmo = v;
				}
			}),
			("infinityStamina", delegate(int v)
			{
				OffDev_InfStam = v;
			}),
			("infiniteStamina", delegate(int v)
			{
				if (OffDev_InfStam < 0)
				{
					OffDev_InfStam = v;
				}
			}),
			("enemyAIEnabled", delegate(int v)
			{
				OffDev_EnemyAI = v;
			}),
			("disableAI", delegate(int v)
			{
				if (OffDev_EnemyAI < 0)
				{
					OffDev_EnemyAI = v;
				}
			})
		});
	}

	private void WalkFields(IntPtr klass, (string name, Action<int> setter)[] targets)
	{
		if (!IsValidPtr(klass))
		{
			return;
		}
		IntPtr intPtr = ReadPtr(klass + 48);
		if (IsValidPtr(intPtr) && intPtr != klass)
		{
			WalkFields(intPtr, targets);
		}
		IntPtr intPtr2 = ReadPtr(klass + 152);
		int num = (int)ReadU32(klass + 256);
		if (!IsValidPtr(intPtr2) || num <= 0 || num > 2048)
		{
			return;
		}
		for (int i = 0; i < num; i++)
		{
			IntPtr intPtr3 = intPtr2 + i * 32;
			IntPtr intPtr4 = ReadPtr(intPtr3 + 8);
			if (!IsValidPtr(intPtr4))
			{
				continue;
			}
			string text = ReadCStr(intPtr4, 128);
			if (string.IsNullOrEmpty(text))
			{
				break;
			}
			int obj = ReadI32(intPtr3 + 24);
			for (int j = 0; j < targets.Length; j++)
			{
				var (text2, action) = targets[j];
				if (text == text2)
				{
					action(obj);
					break;
				}
			}
		}
	}

	private int GetFieldOffset(IntPtr klass, string fieldName)
	{
		if (!IsValidPtr(klass))
		{
			return -1;
		}
		IntPtr intPtr = ReadPtr(klass + 48);
		if (IsValidPtr(intPtr) && intPtr != klass)
		{
			int fieldOffset = GetFieldOffset(intPtr, fieldName);
			if (fieldOffset >= 0)
			{
				return fieldOffset;
			}
		}
		IntPtr intPtr2 = ReadPtr(klass + 152);
		int num = (int)ReadU32(klass + 256);
		if (!IsValidPtr(intPtr2) || num <= 0 || num > 2048)
		{
			return -1;
		}
		for (int i = 0; i < num; i++)
		{
			IntPtr intPtr3 = intPtr2 + i * 32;
			IntPtr intPtr4 = ReadPtr(intPtr3 + 8);
			if (IsValidPtr(intPtr4))
			{
				string text = ReadCStr(intPtr4, 64);
				if (text == fieldName)
				{
					return ReadI32(intPtr3 + 24);
				}
			}
		}
		return -1;
	}

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern IntPtr OpenProcess(uint dwAccess, bool bInherit, int pid);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern bool CloseHandle(IntPtr handle);

	[DllImport("kernel32.dll", SetLastError = true)]
	private static extern int VirtualQueryEx(IntPtr hProcess, IntPtr lpAddress, out MEMORY_BASIC_INFORMATION lpBuffer, int dwLength);

	private List<IntPtr> ScanHeapForObjectVtable(IntPtr vtablePtr)
	{
		List<IntPtr> list = new List<IntPtr>();
		long num = vtablePtr.ToInt64();
		byte[] array = new byte[65536];
		IntPtr intPtr = (IntPtr)65536;
		int num2 = 65536;

		IntPtr hScan = OpenProcess(0x1F0FFF, false, _mem.ProcessId);
		if (hScan == IntPtr.Zero) hScan = _proc;

		try
		{
			MEMORY_BASIC_INFORMATION mbi;
			while (num2-- > 0 && list.Count < 64 && VirtualQueryEx(hScan, intPtr, out mbi, Marshal.SizeOf<MEMORY_BASIC_INFORMATION>()) != 0)
			{
				long num3 = (long)mbi.BaseAddress;
				long num4 = (long)mbi.RegionSize;
				if (num4 == 0)
				{
					break;
				}
				long num5 = num3 + num4;
				if (mbi.State == 4096 && (mbi.Protect & 0x101) == 0)
				{
					long num6 = num3;
					while (num6 < num5)
					{
						int cb = (int)Math.Min(65536L, num5 - num6);
						if (ReadProcessMemory(hScan, (IntPtr)num6, array, cb, out var read) && read.ToInt64() >= 8)
						{
							long bytesRead = read.ToInt64();
							for (int i = 0; i <= bytesRead - 8; i += 8)
							{
								if (BitConverter.ToInt64(array, i) == num)
								{
									list.Add((IntPtr)(num6 + i));
								}
							}
							num6 += bytesRead;
						}
						else
						{
							num6 += 4096;
						}
					}
				}
				intPtr = (IntPtr)num5;
				if ((long)intPtr <= 0 || (long)intPtr >= 0x7FFFFFFFFFFF)
				{
					break;
				}
			}
		}
		finally
		{
			if (hScan != IntPtr.Zero && hScan != _proc)
			{
				CloseHandle(hScan);
			}
		}
		return list;
	}

	private IntPtr ReadPtr(IntPtr addr)
	{
		byte[] array = new byte[8];
		IntPtr read;
		return (ReadProcessMemory(_proc, addr, array, 8, out read) && read.ToInt64() == 8) ? ((IntPtr)BitConverter.ToInt64(array, 0)) : IntPtr.Zero;
	}

	private int ReadI32(IntPtr addr)
	{
		byte[] array = new byte[4];
		IntPtr read;
		return (ReadProcessMemory(_proc, addr, array, 4, out read) && read.ToInt64() == 4) ? BitConverter.ToInt32(array, 0) : 0;
	}

	private uint ReadU32(IntPtr addr)
	{
		byte[] array = new byte[4];
		IntPtr read;
		return (ReadProcessMemory(_proc, addr, array, 4, out read) && read.ToInt64() == 4) ? BitConverter.ToUInt32(array, 0) : 0u;
	}

	private ushort ReadU16(IntPtr addr)
	{
		byte[] array = new byte[2];
		IntPtr read;
		return (ushort)((ReadProcessMemory(_proc, addr, array, 2, out read) && read.ToInt64() == 2) ? BitConverter.ToUInt16(array, 0) : 0);
	}

	private byte ReadByte(IntPtr addr)
	{
		byte[] array = new byte[1];
		IntPtr read;
		return (byte)((ReadProcessMemory(_proc, addr, array, 1, out read) && read.ToInt64() == 1) ? array[0] : 0);
	}

	private byte[] ReadBytes(IntPtr addr, int count)
	{
		byte[] array = new byte[count];
		ReadProcessMemory(_proc, addr, array, count, out var _);
		return array;
	}

	private string ReadCStr(IntPtr addr, int max)
	{
		if (!IsValidPtr(addr))
		{
			return string.Empty;
		}
		byte[] array = new byte[max];
		if (!ReadProcessMemory(_proc, addr, array, max, out var read) || read.ToInt64() == 0)
		{
			return string.Empty;
		}
		int bytesRead = (int)read.ToInt64();
		int num = Array.IndexOf(array, (byte)0);
		return Encoding.UTF8.GetString(array, 0, (num < 0) ? bytesRead : num);
	}

	private static bool IsValidPtr(IntPtr p)
	{
		long num = p.ToInt64();
		return num > 65536 && num < 140737488355327L;
	}

	public int[] ReadInventorySlotAmounts(IntPtr inv1Ptr, int maxSlots = 8)
	{
		int[] array = new int[maxSlots];
		for (int i = 0; i < maxSlots; i++)
		{
			array[i] = -1;
		}
		if (!IsValidPtr(inv1Ptr))
		{
			return array;
		}
		IntPtr intPtr = ReadPtr(inv1Ptr + 32);
		if (!IsValidPtr(intPtr))
		{
			return array;
		}
		IntPtr intPtr2 = ReadPtr(intPtr + 16);
		int num = ReadI32(intPtr + 24);
		if (!IsValidPtr(intPtr2) || num <= 0 || num > 64)
		{
			return array;
		}
		int num2 = Math.Min(num, maxSlots);
		for (int j = 0; j < num2; j++)
		{
			IntPtr intPtr3 = ReadPtr(intPtr2 + 32 + j * 8);
			if (!IsValidPtr(intPtr3))
			{
				array[j] = 0;
			}
			else
			{
				array[j] = ReadI32(intPtr3 + 24);
			}
		}
		return array;
	}

	private string MonoTypeToString(IntPtr fieldPtr)
	{
		IntPtr intPtr = ReadPtr(fieldPtr + 0);
		if (!IsValidPtr(intPtr))
		{
			return "void*";
		}
		byte[] array = new byte[1];
		if (!ReadProcessMemory(_proc, intPtr + 10, array, 1, out var read) || read.ToInt64() == 0)
		{
			return "void*";
		}
		byte b = array[0];
		if (1 == 0)
		{
		}
		string result = b switch
		{
			1 => "void", 
			2 => "bool", 
			3 => "char16_t", 
			4 => "int8_t", 
			5 => "uint8_t", 
			6 => "int16_t", 
			7 => "uint16_t", 
			8 => "int32_t", 
			9 => "uint32_t", 
			10 => "int64_t", 
			11 => "uint64_t", 
			12 => "float", 
			13 => "double", 
			14 => "MonoString*", 
			17 => "ValueType", 
			18 => "void*", 
			20 => "void*[]", 
			21 => "void*", 
			28 => "void*", 
			29 => "void*[]", 
			_ => "void*", 
		};
		if (1 == 0)
		{
		}
		return result;
	}

	public void DumpSDK(string outputPath)
	{
		if (!IsValidPtr(_imagePtr))
		{
			TrainerLog.Error("[Mono] Cannot Dump SDK: Image not loaded.");
			return;
		}
		IntPtr intPtr = _imagePtr + 1232;
		int num = (int)ReadU32(intPtr + 24);
		IntPtr intPtr2 = ReadPtr(intPtr + 32);
		if (!IsValidPtr(intPtr2) || num <= 0 || num > 65536)
		{
			TrainerLog.Error("[Mono] Cannot Dump SDK: Invalid class cache.");
			return;
		}
		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.AppendLine("// Too Much Light v0.7a - Assembly-CSharp SDK Dump");
		stringBuilder.AppendLine("// Generated by TML Trainer");
		stringBuilder.AppendLine();
		stringBuilder.AppendLine("#pragma once");
		stringBuilder.AppendLine("#include <cstdint>");
		stringBuilder.AppendLine();
		int num2 = 0;
		for (int i = 0; i < num; i++)
		{
			IntPtr intPtr3 = ReadPtr(intPtr2 + i * 8);
			int num3 = 0;
			while (IsValidPtr(intPtr3) && num3++ < 128)
			{
				IntPtr intPtr4 = ReadPtr(intPtr3 + 72);
				IntPtr intPtr5 = ReadPtr(intPtr3 + 80);
				if (IsValidPtr(intPtr4))
				{
					string value = ReadCStr(intPtr4, 64);
					string value2 = (IsValidPtr(intPtr5) ? ReadCStr(intPtr5, 128) : "");
					IntPtr intPtr6 = ReadPtr(intPtr3 + 48);
					string value3 = "";
					if (IsValidPtr(intPtr6))
					{
						IntPtr intPtr7 = ReadPtr(intPtr6 + 72);
						if (IsValidPtr(intPtr7))
						{
							value3 = ReadCStr(intPtr7, 64);
						}
					}
					StringBuilder stringBuilder2;
					StringBuilder.AppendInterpolatedStringHandler handler;
					if (!string.IsNullOrEmpty(value2))
					{
						stringBuilder2 = stringBuilder;
						StringBuilder stringBuilder3 = stringBuilder2;
						handler = new StringBuilder.AppendInterpolatedStringHandler(12, 1, stringBuilder2);
						handler.AppendLiteral("namespace ");
						handler.AppendFormatted(value2);
						handler.AppendLiteral(" {");
						stringBuilder3.AppendLine(ref handler);
					}
					stringBuilder2 = stringBuilder;
					StringBuilder stringBuilder4 = stringBuilder2;
					handler = new StringBuilder.AppendInterpolatedStringHandler(6, 1, stringBuilder2);
					handler.AppendLiteral("class ");
					handler.AppendFormatted(value);
					stringBuilder4.Append(ref handler);
					if (!string.IsNullOrEmpty(value3))
					{
						stringBuilder2 = stringBuilder;
						StringBuilder stringBuilder5 = stringBuilder2;
						handler = new StringBuilder.AppendInterpolatedStringHandler(10, 1, stringBuilder2);
						handler.AppendLiteral(" : public ");
						handler.AppendFormatted(value3);
						stringBuilder5.Append(ref handler);
					}
					stringBuilder.AppendLine(" {");
					stringBuilder.AppendLine("public:");
					IntPtr intPtr8 = ReadPtr(intPtr3 + 152);
					int num4 = (int)ReadU32(intPtr3 + 256);
					if (IsValidPtr(intPtr8) && num4 > 0 && num4 < 2048)
					{
						for (int j = 0; j < num4; j++)
						{
							IntPtr intPtr9 = intPtr8 + j * 32;
							IntPtr intPtr10 = ReadPtr(intPtr9 + 8);
							if (IsValidPtr(intPtr10))
							{
								string value4 = ReadCStr(intPtr10, 128);
								if (string.IsNullOrEmpty(value4))
								{
									break;
								}
								int value5 = ReadI32(intPtr9 + 24);
								string value6 = MonoTypeToString(intPtr9);
								stringBuilder2 = stringBuilder;
								StringBuilder stringBuilder6 = stringBuilder2;
								handler = new StringBuilder.AppendInterpolatedStringHandler(16, 3, stringBuilder2);
								handler.AppendLiteral("    // ");
								handler.AppendFormatted(value6);
								handler.AppendLiteral(" ");
								handler.AppendFormatted(value4);
								handler.AppendLiteral("; // +0x");
								handler.AppendFormatted(value5, "X");
								stringBuilder6.AppendLine(ref handler);
							}
						}
					}
					stringBuilder.AppendLine("};");
					if (!string.IsNullOrEmpty(value2))
					{
						stringBuilder.AppendLine("}");
					}
					stringBuilder.AppendLine();
					num2++;
				}
				intPtr3 = ReadPtr(intPtr3 + 264);
			}
		}
		try
		{
			File.WriteAllText(outputPath, stringBuilder.ToString());
			TrainerLog.Info($"[Mono] Dumped SDK: {num2} classes to {outputPath}");
		}
		catch (Exception ex)
		{
			TrainerLog.Error("[Mono] Dump SDK Failed: " + ex.Message);
		}
	}
}


