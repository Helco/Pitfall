using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using Pitfall.Storables;

namespace Pitfall;

[StorableType]
public class EStorable
{
    // a bit hacky but the alternative is much more boilerplate
    public ushort ReadVersion => DynTypeInfo.GetReadVersionFor(new StackFrame(1).GetMethod()!.DeclaringType!);
    public virtual void Read(BinaryReader reader) { }
}

[StorableType]
public class EResource : EStorable
{
    public string Name { get; private set; } = "";

    public override void Read(BinaryReader reader) => Name = reader.ReadCString(); 
}


[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class StorableTypeAttribute : Attribute
{}

public static class DynTypeInfo
{
    private static IReadOnlyDictionary<uint, Func<EStorable>>? allStorableCtors = null;
    private static IReadOnlyDictionary<uint, Type>? allStorableTypes = null;
    public static IReadOnlyDictionary<uint, Type> AllStorableTypes
    {
        get
        {
            if (allStorableTypes != null)
                return allStorableTypes;
            allStorableTypes = typeof(EStorable).Assembly
                .GetTypes()
                .Where(t => t.GetCustomAttribute<StorableTypeAttribute>() != null)
                .ToDictionary(
                    t => HashName(t.Name),
                    t => t);
            return allStorableTypes;
        }
    }
    public static IReadOnlyDictionary<uint, Func<EStorable>> AllStorableCtors
    {
        get
        {
            if (allStorableCtors != null)
                return allStorableCtors;
            allStorableCtors = AllStorableTypes.ToDictionary<KeyValuePair<uint, Type>, uint, Func<EStorable>>(
                kv => kv.Key,
                kv =>
                {
                    var ctor = kv.Value.GetConstructor(Array.Empty<Type>());
                    return () => (EStorable)ctor!.Invoke(null);
                });
            return allStorableCtors;
        }
    }

    private static Dictionary<BinaryReader, EStorable[]> subObjectsPerReader = new();
    private static Dictionary<Type, ushort> readVersions = new();

    public static ushort GetReadVersionFor(Type type) => readVersions.TryGetValue(type, out var version) ? version : (ushort)0;

    public static EStorable? ReadStorable(this BinaryReader reader) => reader.ReadStorable(out _);
    public static EStorable? ReadStorable(this BinaryReader reader, out EStorable[] allSubObjects)
    {
        allSubObjects = Array.Empty<EStorable>();
        var idOrCount = reader.ReadUInt32();
        if (idOrCount == 0)
            return null;
        else if (subObjectsPerReader.TryGetValue(reader, out var prevSubObjects))
            return prevSubObjects[idOrCount - 1];

        var subObjects = new EStorable[idOrCount];
        allSubObjects = subObjects;
        subObjectsPerReader[reader] = subObjects;
        readVersions.Clear();

        for (var i = 0u; i < idOrCount;)
        {
            var typeId = reader.ReadUInt32();
            var readVersion = reader.ReadUInt16();
            var untilIndex = i + reader.ReadUInt32();
            var typeCtor = GetTypeCtor(typeId);
            var type = AllStorableTypes[typeId];
            readVersions[type] = readVersion;
            for (; i < untilIndex; i++)
                subObjects[i] = typeCtor();
        }

        foreach (var subObject in subObjects)
        {
            subObject.Read(reader);
            if (reader.ReadByte() != 0xB7)
                throw new InvalidDataException($"Magic byte was not found after {subObject.GetType().Name}");
        }

        subObjectsPerReader.Remove(reader);

        var rootId = reader.ReadUInt32();
        return subObjects[rootId];
    }

    public static EInstance?[] ReadInstanceArray(this BinaryReader reader)
    {
        var allSubObjects = new EInstance?[reader.ReadInt32()];
        for (var i = 0u; i < allSubObjects.Length;)
        {
            var typeName = reader.ReadCString();
            var typeId = HashName(typeName);
            ushort readVersion = reader.ReadUInt16();
            var setEnd = reader.ReadUInt32() + i;
            var setTotalSize = reader.ReadUInt32();
            if (!AllStorableCtors.TryGetValue(typeId, out var ctor))
            {
                reader.BaseStream.Position += setTotalSize;
                i = setEnd;
                Console.WriteLine($"Warning: Could not read {typeName}");
            }
            else if (!AllStorableTypes[typeId].IsSubclassOf(typeof(EInstance)))
                throw new InvalidDataException("Instance array contains storable types that are not subclass of EInstance");
            else
            {
                readVersions[AllStorableTypes[typeId]] = readVersion;
                for (; i < setEnd; i++)
                {
                    // actuall not so much as override but this is looked up and expected to already be present
                    // in some map I have not looked into much
                    uint overrideID = reader.ReadUInt32();

                    allSubObjects[i] = (EInstance)ctor();
                    // not no Storable.Read is done, so a lot of uninitialized data. that is weird
                    allSubObjects[i]!.instanceID = overrideID;
                    allSubObjects[i]!.ReadInstanceData(reader);
                }
            }
            reader.BaseStream.Position += 4; // seems to be genuinely ignored
        }
        return allSubObjects;
    }

    private static Func<EStorable> GetTypeCtor(uint typeId)
    {
        if (AllStorableCtors.TryGetValue(typeId, out var ctor))
            return ctor;
        var knownType = KnownTypes.FirstOrDefault(n => HashName(n) == typeId);
        if (knownType == null)
            throw new NotSupportedException($"Unsupported EStorable type: {typeId:X8}");
        else
            throw new NotImplementedException($"Unimplemented EStorable type: {knownType}");
    }

    public static uint HashName(string name)
    {
        var bytes = Encoding.UTF8.GetBytes(name);
        uint hash = ~0u;
        foreach (var b in bytes)
            hash = (hash >> 8) ^ HashLookup[b ^ (byte)(hash & 0xFF)];
        return ~hash;
    }

    public static IReadOnlyList<string> KnownTypes = new[]
    {
        "EIAmbientBeast",
        "EIAmbientTucoTuco",
        "EIAnimLod",
        "EIAnimPlatform",
        "EAnimatedProjectile",
        "EIAreaBox",
        "EIBall",
        "EIBallProjectile",
        "EIBats",
        "EIBeast",
        "EIBeastShield",
        "EIBreakable",
        "EIBushNinja",
        "EIButterfly",
        "ECameraFadeInfo",
        "EICameraMagnet",
        "EICinematic",
        "EICoverObject",
        "EICrocs",
        "EICrowd",
        "EIParticleEmitDanger",
        "EIDangerZone",
        "EIDatasetHelper",
        "EICameraBossPenguin",
        "EICameraBossPusca",
        "EICameraFollow",
        "EICameraFollowPogo",
        "EICameraFollowTargetOnly",
        "EICameraLook",
        "EICameraPath",
        "EICameraPathFollow",
        "EICameraPathPlayback",
        "EICameraPit",
        "EICameraRing",
        "EICameraSlingAim",
        "EICameraSpecial",
        "EICameraSwimUfo",
        "EICameraTethered",
        "EICameraUfo",
        "EICameraWatch",
        "EIConstraint",
        "EIConstraintTether",
        "EIPlayerStart",
        "EISun",
        "EIElectricEel",
        "EIEvilHarryMM",
        "EIEvilHarryP",
        "EIEvilHarryS",
        "EIExplorer",
        "EIEyeballs",
        "EIFlockBeast",
        "EIGamePlatform",
        "EIGamesNativeDummyTarget",
        "EIGamesNativeIcePick",
        "EIGamesNativePorcupineTarget",
        "EIGamesNativeTarget",
        "EIGamesNativeTuco",
        "EIGamesNativeTucoTarget",
        "EIGamesTucoTarget",
        "EGamesUI3dBowlingScore",
        "EGamesUI3dScore",
        "EGamesUI3dTimer",
        "EGamesUIBomb",
        "EGamesUIBowling",
        "EGamesUIHogan",
        "EGamesUIIcePick",
        "EGamesUIWackATuco",
        "EIGassregion",
        "EIGrabable",
        "EIHarry",
        "EIBreakableRigidBody",
        "EIHavokBreakable",
        "EIPitfallCloth",
        "EIHavokPlatform",
        "EIPitfallRigidBody",
        "EIHavokShieldPhantom",
        "EIHavokVine",
        "EIHeatSeekProjectile",
        "EIHenchman",
        "EIHowlerMonkey",
        "EIHWPitPointLight",
        "EILadder",
        "EILever",
        "EILightbeam",
        "EILightRay",
        "EILookatPoint",
        "EIMegaMonkeyChild",
        "EIMegaMonkeyParent",
        "EIMole",
        "EINative",
        "EINinjaBat",
        "EINinjaBush",
        "EINPCBeast",
        "EIOlympicBomberman",
        "EPauseMain",
        "EIPenguinNest",
        "EIPenguins",
        "EIPhantomBody",
        "EIPiranha",
        "EIPitMonster",
        "EIPlant",
        "EIPlantNox",
        "EIPlayer",
        "EIPlayerBeast",
        "EPlayerIceCube",
        "EIPlayerMonkey",
        "EIPlayerPenguin",
        "EIPlayerScorpion",
        "EIPorcupine",
        "EIProjectile",
        "EIProjectileGen",
        "EIBombFlash",
        "EIProxAnimObj",
        "EIPusca",
        "EIPuscaMinion",
        "EIPuscaRigidBody",
        "EIResetPlane",
        "EIRigidBodyEmitter",
        "EISavePoint",
        "ESaveUi",
        "ELoadUi",
        "EStartupCheckUi",
        "ESaveUiIcon",
        "ESaveUiMenu",
        "EIScorps",
        "EIScriptAnim",
        "EIShadowSpot",
        "EIShaman",
        "EIShot",
        "EISnowball",
        "EISnowBowler",
        "EISnowScarab",
        "EISpinja",
        "EIStClair",
        "EISupai",
        "EISwingMonkey",
        "EITreasureIdol",
        "EIUserTreasureIdol",
        "EITucoTuco",
        "EUIDDAudioMenu",
        "EUIDDDialogMenu",
        "EUIDDHintCardMenu",
        "EUIDDInventoryMappingItem",
        "EUIDDLevelCardMenu",
        "EUIDDMapArrow",
        "EUIDDMapEventMenu",
        "EUIDDMapMenu",
        "EUIDDMapNode",
        "EUIDDMenu",
        "EUIDDMenuAdjustItem",
        "EUIDDMenuBtnAdjust",
        "EUIDDMenuIconItem",
        "EUIDDMenuSliderItem",
        "EUIDDMenuTextItem",
        "EUIDDOlympicIcon",
        "EUIDDShaderIcon",
        "EUIDDStoreItem",
        "EUIDDTempInventoryMenuDeleteMeSubMenu",
        "EUIDDTempInventoryMenuDeleteMe",
        "EUIDDTempInventoryMenuDeleteMeIcon",
        "EUIDDTempInventoryMenuDeleteMeSubMenuIcon",
        "EUIDDTextPages",
        "EUIHud",
        "EUIHudBoss",
        "EUIHudBossMonkey",
        "EUIHudButt",
        "EUIHudCube",
        "EUIHudInventory",
        "EUIHudInventoryMapping",
        "EUIHudInvStone",
        "EUIHudLifeMeter",
        "EUIHudMain",
        "EUIInventoryMenu",
        "EUINonDDTextItem",
        "EIUseable",
        "EIWackATuco",
        "EIWallTucoTuco",
        "EIWaterType",
        "EIWebEffector",
        "EIWell",
        "EIYoYoMonkey",
        "EICharacter",
        "EIGameInstance",
        "EInstance",
        "EResource",
        "EIDynamicModel",
        "EIStaticModel",
        "ERModel",
        "ERLevel",
        "ERShader",
        "EIPlatform",
        "EIParticleEmit",
        "ERCharacter",
        "EUIPrompt",
        "EUIGridMenu",
        "EUIMenu",
        "EUITextIcon",
        "EUIIcon",
        "EUIObjectNode",
        "EIHavokCloth",
        "EIHavokRope",
        "EIHavokRigidBody",
        "EIBezierSpline",
        "EIWaterPatch",
        "ESDAnimation",
        "ESDResource",
        "ESDFloat",
        "ESDVector",
        "ESDInt",
        "ESDString",
        "ESaveGameVar",
        "ESGVInt",
        "ESGVFloat",
        "ERFont",
        "EIEffector",
        "EIWavePatch",
        "EIParticleEmitLimpet",
        "EICamera",
        "EIHWPointLight",
        "EIMarker",
        "ESDPointer",
        "EHavokWorld",
        "EIProxy",
        "EParticleRibbon",
        "EIQuicksandPatch",
        "EIHavokPhantom",
        "EIPointLight",
        "EILight",
        "ESDScript",
        "ESDModel",
        "EParticleRain",
        "ESDSound",
        "EUIAlphaMenu",
        "ESDShader",
        "EParticleSnow",
        "EILightModifier",
        "ERParticleType",
        "ERHavokModel",
        "EColOctree",
        "EILevel",
        "EIUpdateRegion",
        "ELONode",
        "ELOLeaf",
        "ELOSplit",
        "ELightOctree",
        "ELightOctreeBig",
        "ELightOctreeSmall",
        "ERAnim",
        "ESDParticleType",
        "ESDMatrix",
        "EScriptContext",
        "EScriptData",
        "EParticle",
        "EIParticleEmitSpline",
        "EFontCharacter",
        "EFontPage",
        "EFontSize",
        "EFontData",
        "ESfx",
        "ERSfx",
        "ERDataset",
        "EParticleLine3d",
        "EParticleSpriteRot",
        "EParticleSprite",
        "EIDirLight",
        "EISpotLight",
        "EIAmbLight",
        "EIParticleEmitSurface",
        "EIStaticSubModelShader",
        "EIStaticSubModel",
        "ERTexture",
        "EParticlePoint",
        "EParticleQuad",
        "EParticleArcLine3d",
        "EParticleBubble",
        "EParticleQuadGroundAligned",
        "EParticleSwing",
        "EParticleSnowNoRot",
        "ERBinary",
        "ERScript",
        "EStorable",
        "EBoundTreeNode",
    };

    private static readonly uint[] HashLookup =
    {
        0u,
        1996959894u,
        3993919788u,
        2567524794u,
        124634137u,
        1886057615u,
        3915621685u,
        2657392035u,
        249268274u,
        2044508324u,
        3772115230u,
        2547177864u,
        162941995u,
        2125561021u,
        3887607047u,
        2428444049u,
        498536548u,
        1789927666u,
        4089016648u,
        2227061214u,
        450548861u,
        1843258603u,
        4107580753u,
        2211677639u,
        325883990u,
        1684777152u,
        4251122042u,
        2321926636u,
        335633487u,
        1661365465u,
        4195302755u,
        2366115317u,
        997073096u,
        1281953886u,
        3579855332u,
        2724688242u,
        1006888145u,
        1258607687u,
        3524101629u,
        2768942443u,
        901097722u,
        1119000684u,
        3686517206u,
        2898065728u,
        853044451u,
        1172266101u,
        3705015759u,
        2882616665u,
        651767980u,
        1373503546u,
        3369554304u,
        3218104598u,
        565507253u,
        1454621731u,
        3485111705u,
        3099436303u,
        671266974u,
        1594198024u,
        3322730930u,
        2970347812u,
        795835527u,
        1483230225u,
        3244367275u,
        3060149565u,
        1994146192u,
        31158534u,
        2563907772u,
        4023717930u,
        1907459465u,
        112637215u,
        2680153253u,
        3904427059u,
        2013776290u,
        251722036u,
        2517215374u,
        3775830040u,
        2137656763u,
        141376813u,
        2439277719u,
        3865271297u,
        1802195444u,
        476864866u,
        2238001368u,
        4066508878u,
        1812370925u,
        453092731u,
        2181625025u,
        4111451223u,
        1706088902u,
        314042704u,
        2344532202u,
        4240017532u,
        1658658271u,
        366619977u,
        2362670323u,
        4224994405u,
        1303535960u,
        984961486u,
        2747007092u,
        3569037538u,
        1256170817u,
        1037604311u,
        2765210733u,
        3554079995u,
        1131014506u,
        879679996u,
        2909243462u,
        3663771856u,
        1141124467u,
        855842277u,
        2852801631u,
        3708648649u,
        1342533948u,
        654459306u,
        3188396048u,
        3373015174u,
        1466479909u,
        544179635u,
        3110523913u,
        3462522015u,
        1591671054u,
        702138776u,
        2966460450u,
        3352799412u,
        1504918807u,
        783551873u,
        3082640443u,
        3233442989u,
        3988292384u,
        2596254646u,
        62317068u,
        1957810842u,
        3939845945u,
        2647816111u,
        81470997u,
        1943803523u,
        3814918930u,
        2489596804u,
        225274430u,
        2053790376u,
        3826175755u,
        2466906013u,
        167816743u,
        2097651377u,
        4027552580u,
        2265490386u,
        503444072u,
        1762050814u,
        4150417245u,
        2154129355u,
        426522225u,
        1852507879u,
        4275313526u,
        2312317920u,
        282753626u,
        1742555852u,
        4189708143u,
        2394877945u,
        397917763u,
        1622183637u,
        3604390888u,
        2714866558u,
        953729732u,
        1340076626u,
        3518719985u,
        2797360999u,
        1068828381u,
        1219638859u,
        3624741850u,
        2936675148u,
        906185462u,
        1090812512u,
        3747672003u,
        2825379669u,
        829329135u,
        1181335161u,
        3412177804u,
        3160834842u,
        628085408u,
        1382605366u,
        3423369109u,
        3138078467u,
        570562233u,
        1426400815u,
        3317316542u,
        2998733608u,
        733239954u,
        1555261956u,
        3268935591u,
        3050360625u,
        752459403u,
        1541320221u,
        2607071920u,
        3965973030u,
        1969922972u,
        40735498u,
        2617837225u,
        3943577151u,
        1913087877u,
        83908371u,
        2512341634u,
        3803740692u,
        2075208622u,
        213261112u,
        2463272603u,
        3855990285u,
        2094854071u,
        198958881u,
        2262029012u,
        4057260610u,
        1759359992u,
        534414190u,
        2176718541u,
        4139329115u,
        1873836001u,
        414664567u,
        2282248934u,
        4279200368u,
        1711684554u,
        285281116u,
        2405801727u,
        4167216745u,
        1634467795u,
        376229701u,
        2685067896u,
        3608007406u,
        1308918612u,
        956543938u,
        2808555105u,
        3495958263u,
        1231636301u,
        1047427035u,
        2932959818u,
        3654703836u,
        1088359270u,
        936918000u,
        2847714899u,
        3736837829u,
        1202900863u,
        817233897u,
        3183342108u,
        3401237130u,
        1404277552u,
        615818150u,
        3134207493u,
        3453421203u,
        1423857449u,
        601450431u,
        3009837614u,
        3294710456u,
        1567103746u,
        711928724u,
        3020668471u,
        3272380065u,
        1510334235u,
        755167117u
    };
}
