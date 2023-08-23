namespace Pitfall.Havok;

public abstract class HavokDeformableBodyCollection<T> : IHavokEntityCollection where T : HavokDeformableBody
{
    public string Name { get; }
    public IReadOnlyList<T> Entities { get; }
    public uint InternalSubstep { get; }

    internal HavokDeformableBodyCollection(IHavokReader hkReader, HavokWorld world)
    {
        Name = hkReader.ReadString();
        var entities = new List<T>();
        Entities = entities;
        while (true)
        {
            var token = hkReader.ReadToken();
            if (token == BeginToken)
                entities.Add(ReadEntity(hkReader, world));
            else if (token == "INTERNAL_SUBSTEP")
            {
                InternalSubstep = hkReader.ReadUInt();
                break;
            }
            else
                throw new InvalidDataException($"Unexpected token in {typeof(T).Name} collection {token}");
        }
        hkReader.ExpectToken("END_COLLECTION");
    }

    internal abstract string BeginToken { get; }
    internal abstract T ReadEntity(IHavokReader hkReader, HavokWorld world);
}

public class HavokFastClothCollection : HavokDeformableBodyCollection<HavokFastCloth>
{
    internal HavokFastClothCollection(IHavokReader hkReader, HavokWorld world) : base(hkReader, world)
    {
    }

    internal override string BeginToken => "BEGIN_FASTCLOTH";

    internal override HavokFastCloth ReadEntity(IHavokReader hkReader, HavokWorld world) => new(hkReader, world);
}

public class HavokFastRopeCollection : HavokDeformableBodyCollection<HavokFastRope>
{
    internal HavokFastRopeCollection(IHavokReader hkReader, HavokWorld world) : base(hkReader, world)
    {
    }

    internal override string BeginToken => "BEGIN_FASTROPE";

    internal override HavokFastRope ReadEntity(IHavokReader hkReader, HavokWorld _) => new(hkReader);
}
